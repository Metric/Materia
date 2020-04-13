#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Bloom;

void main() {
    //c is background real render
    vec4 c = texture(MainTex, UV);
    //b is foreground bloom
    vec4 b = texture(Bloom, UV);
	
    vec4 final = vec4(0,0,0, clamp(c.a + b.a, 0, 1));
	final.rgb = c.rgb + b.rgb;

    //since we are not doing anything else
    //in the render pipeline besides bloom
    //we can apply HDR & Gamma correction for now
    //later on we will have another pass specifically
    //for this in the PBR rendering

    //HDR
    final.rgb = final.rgb / (final.rgb + vec3(1.0)); 

    //GAMMA
    final.rgb = pow(final.rgb, vec3(1.0/2.2));

    FragColor = final;
}