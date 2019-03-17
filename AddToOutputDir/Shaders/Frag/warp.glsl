#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Warp;
uniform float intensity = 1.0;

void main() {
    vec2 uv = UV;

    float w = texture(Warp, UV).r;

    uv += vec2(w) * intensity;

    vec4 c = texture(MainTex, uv);
    FragColor = c;
}