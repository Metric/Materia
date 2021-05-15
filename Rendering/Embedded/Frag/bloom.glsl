#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Bloom;

void main() {
    //c is background real render
    vec4 c = texture(MainTex, UV);
    //b is foreground bloom
    vec3 b = texture(Bloom, UV).rgb;

    vec4 final = c;

    final.rgb += b;

    //since we are not doing anything else
    //in the render pipeline besides bloom
    //we can apply HDR & Gamma correction for now
    //later on we will have another pass specifically
    //for this in the PBR rendering

    //HDR
    final.rgb = final.rgb / (final.rgb + vec3(1.0)); 

    //GAMMA + premult
    final.rgb = pow(final.rgb, vec3(1.0 / 2.2)) * final.a;

    FragColor = clamp(final, vec4(0), vec4(1));
}