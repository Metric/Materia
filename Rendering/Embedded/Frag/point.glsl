#version 330 core

struct AppData {
   vec4 Color;
   mat4 Scale;
   vec3 WorldPos;
   mat4 Rotation;
   vec4 ClipPos;
   float Size;
};

in AppData appData;
in vec2 uv;

out vec4 FragColor;

uniform sampler2D MainTex;
uniform float hardness = 1.0;

void main() {
    vec2 uvf = uv - 0.5;
    float len = length(uvf);
    vec4 c = texture(MainTex, uv);
    c.a *= 1.0 - 1.0 * pow(len, hardness * 10.0);

    if (c.a <= 0.01f) {
        discard;
    }

    vec4 mult = c * appData.Color;
    mult.rgb *= mult.a;
    FragColor = mult;
}