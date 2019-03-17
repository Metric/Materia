#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

uniform int invertRed = 1;
uniform int invertGreen = 1;
uniform int invertBlue = 1;
uniform int invertAlpha = 0;

void main() {
    vec4 c = texture(MainTex, UV);
    float r = c.r;
    float g = c.g;
    float b = c.b;
    float a = c.a;

    if(invertRed > 0) {
        r = 1.0 - r;
    }

    if(invertGreen > 0) {
        g = 1.0 - g;
    }

    if(invertBlue > 0) {
        b = 1.0 - b;
    }

    if(invertAlpha > 0) {
        a = 1.0 - a;
    }

    FragColor = vec4(r,g,b,a);
}