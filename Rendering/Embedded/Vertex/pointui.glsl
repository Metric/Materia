#version 330 core
layout (location = 0) in vec2 pos;

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;

uniform vec2 position;
uniform vec2 size;

out vec2 inPosition;
out vec2 inSize;

void main() {
    inPosition = position;
    inSize = size;
    vec4 clip = projectionMatrix * modelMatrix * vec4(position, 0, 1);
    gl_Position = clip;
}