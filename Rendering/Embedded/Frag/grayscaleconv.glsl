#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

uniform vec4 weight;

void main() {
    vec4 c = texture(MainTex, UV);

    int one = 0;
    if(weight.r > 0) {
        one = one + 1;
    }
    if (weight.g > 0) {
        one = one + 1;
    }
    if (weight.b > 0) {
        one = one + 1;
    }
    if (weight.a > 0) {
        one = one + 1;
    }

    if(one == 0) {
        one = 1;
    }

    float d = (c.r * weight.r + c.g * weight.g + c.b * weight.b + c.a * weight.a) / one;

    FragColor = vec4(d,d,d,1); 
}