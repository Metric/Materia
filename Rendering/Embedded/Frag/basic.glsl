#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float alpha = 1;
uniform int invertMult = 0;

void main() {
    vec4 c = texture(MainTex, UV);

    c.a *= alpha;
    c.rgb *= alpha;

    if (invertMult > 0) 
    {
        c.rgb /= c.a;
    }

    FragColor = c;
}