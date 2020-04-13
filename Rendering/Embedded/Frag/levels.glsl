#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform vec3 minValues;
uniform vec3 maxValues;
uniform vec3 midValues;
uniform vec2 value;

float Gamma(float mid) {
    float gamma = 1;

    if(mid < 0.5) {
        mid = mid * 2;
        gamma = 1 + (9 * (1 - mid));
        gamma = min(gamma, 9.99);
    }
    else if(mid > 0.5) {
        mid = (mid * 2) - 1;
        gamma = 1 - mid;
        gamma = max(gamma, 0.01);
    }

    return 1.0 / gamma;
}

void main() {
    vec4 c = texture(MainTex, UV);
    
    float rgamma = 1;
    float ggamma = 1;
    float bgamma = 1;

    rgamma = Gamma(midValues.r);
    ggamma = Gamma(midValues.g);
    bgamma = Gamma(midValues.b);

    vec3 adjusted = (c.rgb - minValues) / (maxValues - minValues);

    adjusted = min(vec3(1), max(vec3(0), adjusted));

    if(rgamma < 1 || rgamma > 1) {
        adjusted.r = min(1, max(0, pow(adjusted.r, rgamma)));
    }
    if(ggamma > 1 || ggamma < 1) {
        adjusted.g = min(1, max(0, pow(adjusted.g, ggamma)));
    }
    if(bgamma > 1 || bgamma < 1) {
        adjusted.b = min(1, max(0, pow(adjusted.b, bgamma)));
    }

    //apply min max value range
    adjusted = min(vec3(1), max(vec3(0), adjusted * (value.y - value.x) + value.x));

    FragColor = vec4(adjusted, c.a);
}