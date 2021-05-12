#version 330 core
layout(location = 0) in vec2 pos;
layout(location = 1) in vec2 size;
layout(location = 2) in vec4 uv;

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform vec2 offset;

out vec2 inPosition;
out vec2 inSize;
out vec4 inUV;

void main() {
    inPosition = pos + offset;
    inSize = size;
    inUV = uv;
    vec4 clip = projectionMatrix * modelMatrix * vec4(pos + offset, 0, 1);
    gl_Position = clip;
}