#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 uv0;
layout (location = 3) in vec4 tangent;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform mat4 normalMatrix;
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
    vec3 T = mat3(normalMatrix) * tangent.xyz;
    vec3 N = mat3(normalMatrix) * normal;
    vec3 B = mat3(normalMatrix) * (cross(N, T) * tangent.w);

    o.TBN = mat3(normalize(T),normalize(B),normalize(N));
	o.WorldPos = (modelMatrix * vec4(pos, 1)).xyz;
    o.UV = uv0 * tiling;

    //flip y for opengl textures
    o.UV.y = 1.0 - o.UV.y;
    
    o.ObjectPos = pos;
    o.Normal = normalize(N);

    o.T = T;
    o.B = B;
    o.N = N;

    vec4 clip = projectionMatrix * viewMatrix * modelMatrix * vec4(pos, 1);

    data.ClipPos = clip;
    data = o;
    
    gl_Position = clip;
}

