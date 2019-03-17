#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D CurveLUT;

void main() {
    vec4 c = texture(MainTex, UV);

    //convert to int in the 0-255 curve range
    int rx = int(min(1, max(0, c.r)) * 255);
    int gx = int(min(1, max(0, c.g)) * 255);
    int bx = int(min(1, max(0, c.b)) * 255);

    //texelFetch instead of texture() so no filtering is applied whatsoever
    //otherwise if using texture() then it is possible that the
    //curve lookup will be off if using linear or way off if using nearest
    vec4 rr = texelFetch(CurveLUT, ivec2(rx, 0), 0);
    vec4 gg = texelFetch(CurveLUT, ivec2(gx, 0), 0);
    vec4 bb = texelFetch(CurveLUT, ivec2(bx, 0), 0);

    //calculate midtones in 255 range
    //alpha channel is used as midtone value
    //from curve lut
    float rmid = (rr.r * 255) / (rr.a * 255);
    float gmid = (gg.g * 255) / (gg.a * 255);
    float bmid = (bb.b * 255) / (bb.a * 255);

    //apply midtones and return as final color
    FragColor = vec4(rr.r * rmid, gg.g * gmid, bb.b * bmid, c.a);
}