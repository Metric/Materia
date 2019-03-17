#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform vec4 color;

void main() {
    FragColor = color;
}