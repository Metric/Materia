#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 Brightness;

void main() {
    Brightness = vec4(0);
    FragColor = vec4(vec3(gl_FragCoord.w), 1);
}