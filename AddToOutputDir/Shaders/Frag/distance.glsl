////The algorithm for EDT is from: http://cs.brown.edu/people/pfelzens/papers/dt-final.pdf
////the above algorithm is licensed under https://creativecommons.org/licenses/by/3.0/
////It has been adopted to run with rgba32F textures 
////in this glsl compute shader for parallel processing
////on the GPU

#version 430
layout (local_size_x = 8, local_size_y = 1, local_size_z = 1) in;
layout ({0}, binding = 0) uniform image2D _out_put;
layout ({0}, binding = 1) uniform image2D _in_put;
layout ({0}, binding = 2) uniform image2D _source;
layout (rgba32f, binding = 3) uniform image2D _vz;

uniform float maxDistance = 0.2;
uniform int sourceOnly = 0;

uniform float width;
uniform float height;

uniform int stage = 0;

float INF = 100000000000000.0f;

float uvtch(float uv, ivec2 pos, float length) {
	ivec2 npos = ivec2(uv, pos.x);
	return imageLoad(_out_put, npos).r;
}

float uvtcv(float uv, ivec2 pos, float length) {
	ivec2 npos = ivec2(pos.x, uv);
	return imageLoad(_in_put, npos).r;
}

float FH(float x, float i, ivec2 pos, float length) {
	float diff = x - i;
	diff = diff * diff;
	float v = uvtch(i, pos, length);
	return diff + v;
}

float FV(float x, float i, ivec2 pos, float length) {
	float diff = x - i;
	diff = diff * diff;
	float v = uvtcv(i, pos, length);
	return diff + v;
}

float SepH(float uv, float ov, ivec2 pos, float length) {
	float cv = uvtch(ov, pos, length);
	float cuv = uvtch(uv, pos, length);
	return ((cuv + uv * uv) - (cv + ov * ov)) / (2 * uv - 2 * ov);
}

float SepV(float uv, float ov, ivec2 pos, float length) {
	float cv = uvtcv(ov, pos, length);
	float cuv = uvtcv(uv, pos, length);
	return ((cuv + uv * uv) - (cv + ov * ov)) / (2 * uv - 2 * ov);
}

void edth(ivec2 pos, float length) {
	int k = 0;
	
	//c.g = v[]
	//c.b = z[]
	
	ivec2 svec = ivec2(0, pos.x);
	vec4 sc = vec4(0,0,-INF,1);
	imageStore(_vz, svec, sc);
	
	svec = ivec2(1, pos.x);
	sc.b = INF;
	imageStore(_vz, svec, sc);
	
	for(int q = 1; q < length; ++q) {
		ivec2 kpos = ivec2(k, pos.x);
		vec4 kc = imageLoad(_vz, kpos);
		float s = SepH(q, kc.g, pos, length);
		while(s <= kc.b && k >= 0) {
			--k;
			kpos = ivec2(k, pos.x);
			kc = imageLoad(_vz, kpos);
			s = SepH(q, kc.g, pos, length);
		}
		
		k++;
		kpos = ivec2(k, pos.x);
		kc = imageLoad(_vz, kpos);
		kc.g = q;
		kc.b = s;
		imageStore(_vz, kpos, kc);
		kpos = ivec2(k + 1, pos.x);
		kc = imageLoad(_vz, kpos);
		kc.b = INF;
		imageStore(_vz, kpos, kc);
	}
	k = 0;
	for(int q = 0; q < length; ++q) {
		ivec2 kpos = ivec2(k + 1, pos.x);
		vec4 kc = imageLoad(_vz, kpos);
		while (kc.b < q && k < length - 1) {
			++k;
			kpos = ivec2(k + 1, pos.x);
			kc = imageLoad(_vz, kpos);
		}
		
		kpos = ivec2(k, pos.x);
		kc = imageLoad(_vz, kpos);
		float d = FH(q, kc.g, pos, length);
		ivec2 npos = ivec2(q, pos.x);
		vec4 c = imageLoad(_vz, npos);
		c.r = d;
		imageStore(_vz, npos, c);
	}
}

void edtv(ivec2 pos, float length) {
	int k = 0;
	
	//c.g = v[]
	//c.b = z[]
	
	ivec2 svec = ivec2(pos.x, 0);
	vec4 sc = vec4(0,0,-INF,1);
	imageStore(_vz, svec, sc);
	
	svec = ivec2(pos.x, 1);
	sc.b = INF;
	imageStore(_vz, svec, sc);
	
	for(int q = 1; q < length; ++q) {
		ivec2 kpos = ivec2(pos.x, k);
		vec4 kc = imageLoad(_vz, kpos);
		float s = SepV(q, kc.g, pos, length);
		while(s <= kc.b && k >= 0) {
			--k;
			kpos = ivec2(pos.x, k);
			kc = imageLoad(_vz, kpos);
			s = SepV(q, kc.g, pos, length);
		}
		
		k++;
		kpos = ivec2(pos.x, k);
		kc = imageLoad(_vz, kpos);
		kc.g = q;
		kc.b = s;
		imageStore(_vz, kpos, kc);
		kpos = ivec2(pos.x, k + 1);
		kc = imageLoad(_vz, kpos);
		kc.b = INF;
		imageStore(_vz, kpos, kc);
	}
	k = 0;
	for(int q = 0; q < length; ++q) {
		ivec2 kpos = ivec2(pos.x, k + 1);
		vec4 kc = imageLoad(_vz, kpos);
		while (kc.b < q && k < length - 1) {
			++k;
			kpos = ivec2(pos.x, k + 1);
			kc = imageLoad(_vz, kpos);
		}
		
		kpos = ivec2(pos.x, k);
		kc = imageLoad(_vz, kpos);
		float d = FV(q, kc.g, pos, length);
		ivec2 npos = ivec2(pos.x, q);
		vec4 c = vec4(d,0,0,1);
		imageStore(_out_put, npos, c);
	}
}

void main() {
	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	 if (stage == 0) {
		//perform column transforms
		edtv(pos, height);
	}
	else if(stage == 1) {
		//perform row transforms
		edth(pos, width);
	}
}