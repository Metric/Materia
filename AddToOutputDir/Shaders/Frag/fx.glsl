#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

uniform mat4 model;
uniform vec2 pivot;

uniform float luminosity = 1;

void main() {
    vec4 rpos = vec4(UV - pivot, 0, 1);
    rpos = model * rpos; 
    vec2 pos = rpos.xy + pivot;

    if(pos.x > 1 || pos.x < 0 || pos.y > 1 || pos.y < 0) {
        discard;
    }

    vec4 c = texture(MainTex, pos);
    c.rgb *= luminosity;
    FragColor = c;
}