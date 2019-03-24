#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Warp;
uniform float intensity = 1.0;

void main() {
    vec2 uv = UV;

    vec2 offset = 1.0 / textureSize(Warp, 0);

    float grad1 = texture(Warp, UV - vec2(0,offset.y)).r * 2.0 - 1.0;
    float grad2 = texture(Warp, UV - vec2(offset.x,0)).r * 2.0 - 1.0;
    float grad3 = texture(Warp, UV + vec2(0,offset.y)).r * 2.0 - 1.0;
    float grad4 = texture(Warp, UV + vec2(offset.x, 0)).r * 2.0 - 1.0;

    uv.x += (grad2 - grad4) * intensity;
    uv.y += (grad1 - grad3) * intensity;

    vec4 c = texture(MainTex, uv);
    FragColor = c;
}