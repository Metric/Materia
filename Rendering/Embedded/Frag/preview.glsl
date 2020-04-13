#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float luminosity = 1.0;
uniform int flipY = 0;

void main() {
    vec4 c = texture(MainTex, UV);
    c.rgb *= luminosity;
    FragColor = c;
}