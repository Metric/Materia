#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float luminosity = 1.0;

void main() {
    vec2 uv = UV;
    //flip y uv
    uv.y = 1.0 - uv.y;
    vec4 c = texture(MainTex, uv);
    c.rgb *= luminosity;
    FragColor = c;
}