#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Other;

uniform int redChannel = 0;
uniform int greenChannel = 1;
uniform int blueChannel = 2;
uniform int alphaChannel = 3;

uniform float luminosity = 1;

void main() {
    vec4 c = texture(MainTex, UV);
    vec4 c2 = texture(Other, UV);

    vec4 final = vec4(0);

    if(redChannel > 3) 
    {
        final.r = c2[redChannel - 4];
    }
    else {
        final.r = c[redChannel];
    }

    if(greenChannel > 3) {
        final.g = c2[greenChannel - 4];
    }
    else {
        final.g = c[greenChannel];
    }

    if(blueChannel > 3) {
        final.b = c2[blueChannel - 4];
    }
    else {
        final.b = c[blueChannel];
    }

    if(alphaChannel > 3) {
        final.a = c2[alphaChannel - 4];
    }
    else {
        final.a = c[alphaChannel];
    }

    FragColor = final;
}