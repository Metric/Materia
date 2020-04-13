#version 430
layout (local_size_x = 8, local_size_y = 8, local_size_z = 1) in;
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

void main() {
	ivec2 pos = ivec2(gl_GlobalInvocationID.xy);
	if (stage == 0) {
		//convert the temp input into
		//the proper format for distance transform
		vec4 c = imageLoad(_in_put, pos);
		imageStore(_in_put, pos, vec4(c.r == 1 ? 0 : c.r == 0 ? INF : pow(max(0, 0.5 - c.r), 2), 0, 0, c.a));
	}
	else if(stage == 1) {
		//finalize by converting 
		//back to 0-1 range for textures,
		//then applying our max distance limitation
		//then sqrt
		//finally inverting to produce the needed result
		//that we want
		vec4 c = imageLoad(_vz, pos);
		float f = 1.0 - sqrt(c.r  / (width * height) / (maxDistance * maxDistance));
		if (sourceOnly == 0) {
			vec4 c = imageLoad(_in_put, pos);
			vec4 c2 = imageLoad(_source, pos);
			c2.rgb += vec3(f);
			imageStore(_out_put, pos, vec4(c2.rgb, c.a + f));
		}
		else {
			vec4 c2 = imageLoad(_source, pos);
			if (f > 0) {
				c2.rgb += vec3(1);
				imageStore(_out_put, pos, c2);
			}
			else {
				imageStore(_out_put, pos, vec4(0, 0, 0, c2.a));
			}
		}
	}
}