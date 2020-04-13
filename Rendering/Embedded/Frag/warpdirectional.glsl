#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Warp;
uniform float intensity = 1.0;
uniform float angle = 0;

void main() {
    vec2 uv = UV;
    float cs = cos(angle);
    float si = sin(angle);
    float r = texture(Warp, uv).r;
    vec2 n = vec2(r * cs, r * si);
    FragColor = texture(MainTex, uv + n * intensity);
}