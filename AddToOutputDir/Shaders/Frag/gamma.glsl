#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

uniform float gamma;

void main() {
    vec4 c = texture(MainTex, UV);
    FragColor.rgb = pow(c.rgb, vec3(1.0 / gamma));
    FragColor.a = c.a;
}