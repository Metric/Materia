#version 410 core
layout(triangles, equal_spacing, ccw) in;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat4 normalMatrix;

uniform int displace = 0;
uniform float heightScale = 0.2;

struct AppData {
    vec3 Normal;
    vec2 UV;
    vec3 WorldPos;
    mat3 TBN;
    vec3 ObjectPos;
    vec4 ClipPos;
    vec3 T;
    vec3 B;
    vec3 N;
};

in vec3 WorldPos_ES[];
in vec3 Normal_ES[];
in vec2 UV_ES[];
in vec4 Tangent_ES[];
in vec3 ObjectPos_ES[];

out AppData data;

uniform sampler2D heightMap;

float interpolate(float f0, float f1, float f2)
{
    return gl_TessCoord.x * f0 + gl_TessCoord.y * f1 + gl_TessCoord.z * f2;
}

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2)
{
   	return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2)
{
   	return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}

void main()
{
    AppData o;
    vec3 wpos = interpolate3D(WorldPos_ES[0], WorldPos_ES[1], WorldPos_ES[2]);
    vec2 uv = interpolate2D(UV_ES[0], UV_ES[1], UV_ES[2]);
    vec3 n = interpolate3D(Normal_ES[0], Normal_ES[1], Normal_ES[2]);
    vec3 t = interpolate3D(Tangent_ES[0].xyz, Tangent_ES[1].xyz, Tangent_ES[2].xyz);
    float tw = interpolate(Tangent_ES[0].w, Tangent_ES[1].w, Tangent_ES[2].w);
    vec3 pos = interpolate3D(ObjectPos_ES[0], ObjectPos_ES[1], ObjectPos_ES[2]);

    o.UV = uv;
    o.Normal = normalize(n);
    o.ObjectPos = pos;

    o.T = t;
    o.N = n;
    
    vec3 b = mat3(normalMatrix) * (cross(n, t) * tw);
    o.B = b;
    o.TBN = mat3(normalize(t),normalize(b),normalize(n));

    //calculate displacement if displacement is active;
    if(displace == 1) 
    {
        float disp = texture(heightMap, uv).r;
        wpos += o.Normal * disp * heightScale;
    }

    o.WorldPos = wpos;
    o.ClipPos = projectionMatrix * viewMatrix * vec4(wpos, 1.0);
    data = o;
    gl_Position = o.ClipPos;
}