#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 Brightness;

uniform vec4 color;

uniform float luminosity = 1;

void main() {
    vec4 c = color;
    Brightness = vec4(0);

    //ensure premult
    c.rgb *= c.a;

    if(length(c.rgb) > 3) {
        Brightness = vec4(clamp(c.rgb, vec3(0), vec3(1)), 1.0);
    }

    FragColor = c;
}