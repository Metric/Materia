#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform vec4 color;
uniform float luminosity = 1.0;
uniform sampler2D MainTex;
uniform int flipY = 1;
uniform vec2 tiling = vec2(1, 1);
uniform vec2 uvoffset = vec2(0, 0);

void main() {
    vec2 offset = uvoffset;
    vec2 ruv = uv;
    
    if (flipY == 1) 
    {
        ruv = vec2(uv.x, 1.0 - uv.y);
    }

    ruv += offset;
    ruv *= tiling;

    vec4 c = texture(MainTex, ruv);

    if (c.a <= 0.01f) {
        discard;
    }

    vec4 mult = c * color * luminosity;
    mult.rgb *= mult.a;
    FragColor = mult;
}