﻿#version 330 core

in vec4 Color;
out vec4 FragColor;

void main() {
    vec4 c = Color;
    c.rgb *= c.a;
    FragColor = c;
}