
#version 330 core
layout (location = 0) in vec2 pos;
layout (location = 1) in vec2 uv;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

uniform float zIndex = 0;
uniform float tiling = 1;

struct AppData {
   vec2 UV;
   vec3 WorldPos;
   vec4 Clip;
};

out AppData data;

void main() {

    AppData o;

    o.UV = uv * tiling;
    o.WorldPos = vec3(pos, zIndex);
    o.Clip = projectionMatrix * viewMatrix * modelMatrix * vec4(o.WorldPos, 1);

    data = o;

    gl_Position = o.Clip;
}