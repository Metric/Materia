#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform mat3 rotation;
uniform mat3 scale;
uniform vec3 translation;

void main() {
    vec2 size = textureSize(MainTex, 0);
    vec3 runits = vec3(size * (UV - 0.5), 0);

    runits = rotation * runits;
    runits = scale * runits;
    runits += translation;

    vec2 fpos = runits.xy / size + 0.5;

    vec4 c = texture(MainTex, fpos);

    FragColor = c;
}