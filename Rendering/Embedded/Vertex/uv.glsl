#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 uv0;
layout (location = 3) in vec4 tangent;

void main() {
    //center uv coords
    gl_Position = vec4(uv0 * 2.0 - 1.0, 0, 1);
}