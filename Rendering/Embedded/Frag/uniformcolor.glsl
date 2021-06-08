#version 330 core
layout (location = 0) out vec4 FragColor;

uniform vec4 color;
uniform float luminosity = 1;

void main() {
    vec4 c = color;
    c.rgb *= luminosity;
    FragColor = c;
}