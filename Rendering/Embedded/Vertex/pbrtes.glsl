#version 410 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 uv0;
layout (location = 3) in vec4 tangent;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform vec2 tiling;

out vec3 WorldPos;
out vec2 UV;
out vec3 Normal;
out vec4 Tangent;
out vec3 ObjectPos;

void main() 
{
    mat3 normalMatrix = mat3(modelMatrix);
    Tangent = vec4(normalMatrix * tangent.xyz, tangent.w);
    Normal = normalMatrix * normal;
    WorldPos = (modelMatrix * vec4(pos, 1)).xyz;
    ObjectPos = pos;
    UV = uv0;

    //flip y for opengl textures
    UV.y = 1.0 - UV.y;

    UV *= tiling;
}