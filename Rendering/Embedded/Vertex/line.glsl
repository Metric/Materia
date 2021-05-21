#version 330 core

layout(location = 0) in vec3 pos;
layout(location = 1) in vec4 color;

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;
uniform vec2 offset;

out vec4 Color;

void main() {
    Color = color;
    vec4 clip = projectionMatrix * modelMatrix * vec4(pos + vec3(offset, 0), 1);
    gl_Position = clip;
}
