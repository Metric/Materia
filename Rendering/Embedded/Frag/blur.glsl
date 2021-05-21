#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float intensity;
uniform int horizontal;
uniform float luminosity = 1;

void main() {
    vec2 offset = 1.0 / textureSize(MainTex, 0);

    float whalf = intensity * 0.5;
    vec4 result = vec4(0);

    if(horizontal == 1) {
        offset.y = 0;
    }
    else {
        offset.x = 0;
    }

    for(float j = -whalf; j <= whalf; j++) {
        vec4 c = texture(MainTex, UV + (j * offset));
        result += c;
    }

    vec4 final = result / intensity;
    FragColor = final;
}