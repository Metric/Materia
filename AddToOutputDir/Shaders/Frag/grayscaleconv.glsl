#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;

uniform vec4 weight;

void main() {
    vec4 c = texture(MainTex, UV);

    float d = (c.r * weight.r + c.g * weight.g + c.b * weight.b + c.a * weight.a) / max(0.001, (weight.r + weight.g + weight.b + weight.a));

    FragColor = vec4(d,d,d,1); 
}