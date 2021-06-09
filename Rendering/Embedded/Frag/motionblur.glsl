#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform float width;
uniform float height;

uniform float tx;
uniform float ty;
uniform float magnitude;

uniform float luminosity = 1;

uniform sampler2D MainTex;

void main() {
    vec2 dir = normalize(vec2(tx,ty));
    vec2 offset = 1.0 / textureSize(MainTex, 0);
    float whalf = magnitude * 0.5;
    vec4 result = vec4(0);

    for(float j = -whalf; j <= whalf; j++) 
    {
        vec4 c = texture(MainTex, UV + (j * offset * dir));
        result += c;
    }

    vec4 final = result / magnitude;
    //clamp it
    FragColor = clamp(final, vec4(0), vec4(1));
}