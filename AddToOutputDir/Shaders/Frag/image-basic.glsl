#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

void main() {
    vec4 c = texture(MainTex, UV);
    FragColor = c;
}