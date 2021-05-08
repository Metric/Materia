#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform vec4 color;
uniform sampler2D MainTex;
uniform int flipY = 1;
uniform vec2 tiling = vec2(1, 1);

void main() {
    vec2 ruv = uv * tiling;
    
    if (flipY == 1) 
    {
        ruv = vec2(uv.x, 1.0 - uv.y);
    }

    vec4 c = texture(MainTex, ruv);

    if (c.a <= 0.01f) {
        discard;
    }

    vec4 mult = c * color;
    mult.rgb *= mult.a;
    FragColor = mult;
}