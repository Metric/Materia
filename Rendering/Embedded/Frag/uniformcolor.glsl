#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 Brightness;

uniform vec4 color;

uniform float luminosity = 1;

float lengthSqr(vec3 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z;
}

void main() {
    vec4 c = color;
    Brightness = vec4(0);

    //ensure premult
    c.rgb *= c.a;

    if(lengthSqr(c.rgb) > 3) {
        Brightness = vec4(c.rgb, 1.0);
    }

    FragColor = c;
}