#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D ColorLUT;
uniform sampler2D Mask;

uniform int useMask = 0;
uniform int horizontal = 1;

void main() {
    vec2 msize = textureSize(MainTex, 0);
    vec4 rgba = texture(MainTex, UV);
    vec2 size = textureSize(ColorLUT, 0);
    vec4 c = vec4(0);
    
    if(horizontal == 1) {
        c = texelFetch(ColorLUT, ivec2(min(rgba.r * size.x, size.x - 1), 0), 0);
    }
    else {
        c = texelFetch(ColorLUT, ivec2(min(rgba.r * size.y, size.y - 1), 0), 0);
    }

    if(useMask == 1) {
        vec2 maskSize = textureSize(Mask, 0);
        vec2 m2 = texture(Mask, UV).ra;
        if(m2.y >= 1) {
            float m = min(m2.x, 1);
            c *= m;
        }
        else {
            float m = min(m2.x + m2.y, 1);
            c *= m;
        }
    }

    c.a = rgba.a * c.a;
    FragColor = c;
}