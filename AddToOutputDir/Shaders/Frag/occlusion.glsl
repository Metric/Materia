#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Original;

//this is part two of the final occlusion pass
//occlusion pass requires two passes
//one for blur and the other for the inversion of blur
//to white black based on original grayscale
void main() {
    vec4 c = texture(MainTex, UV);
    vec4 c2 = texture(Original, UV);

    ///main tex is the blurrred
    //and this only works on grayscale images
    //do not try with full color
    c.rgb = min(vec3(1), max(vec3(0),1.0 + c.rrr - c2.r));

    FragColor = vec4(c.rgb,1); 
}