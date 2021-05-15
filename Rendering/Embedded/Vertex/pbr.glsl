#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 uv0;
layout (location = 3) in vec4 tangent;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform vec2 tiling;

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

out AppData data;

void main() 
{
    AppData o;
    mat3 normalMatrix = mat3(modelMatrix);

    vec3 T = normalMatrix * tangent.xyz;
    vec3 N = normalMatrix * normal; //normalMatrix * normal;
    vec3 B = normalMatrix * (cross(N, T) * tangent.w);

    o.TBN = mat3(normalize(T),normalize(B),normalize(N));
	o.WorldPos = (modelMatrix * vec4(pos, 1)).xyz;
   
    //flip y for opengl textures
    o.UV = uv0;

    o.UV.y = 1.0 - o.UV.y;

    o.UV *= tiling;
    
    o.ObjectPos = pos;
    o.Normal = N;

    o.T = T;
    o.B = B;
    o.N = N;

    vec4 clip = projectionMatrix * viewMatrix * vec4(o.WorldPos, 1);

    o.ClipPos = clip;
    data = o;
    
    gl_Position = clip;
}

