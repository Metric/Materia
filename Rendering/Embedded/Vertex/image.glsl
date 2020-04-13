#version 330 core
layout (location = 0) in vec3 pos;
layout (location = 1) in vec2 uv0;

out vec2 UV;

uniform vec2 tiling = vec2(1);

void main() {
    UV = uv0 * tiling;
    gl_Position = vec4(pos.x,pos.y, 0, 1);
}