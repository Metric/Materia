#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D ColorLUT;
uniform sampler2D Mask;

uniform int useMask = 0;

void main() {
    vec4 rgba = texture(MainTex, UV);
    vec4 c = texture(ColorLUT, vec2(rgba.r, 0.5));

    if(useMask == 1) {
        float m = texture(Mask, UV).r;
        c *= m;
    }

    c.a = rgba.a * c.a;

    FragColor = c;
}