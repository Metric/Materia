#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec2 uv0;

out vec2 UV;

uniform vec2 tiling = vec2(1);
uniform mat4 modelMatrix;
uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main() {
    UV = uv0 * tiling;
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos.x,pos.y,pos.z, 1);
}