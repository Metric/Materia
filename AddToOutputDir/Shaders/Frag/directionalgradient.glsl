#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D ColorLUT;
uniform int horizontal = 1
uniform int useColor = 0;

void main() 
{
    if(horizontal == 1) 
    {
        FragColor = vec4(vec3(uv.x), 1);
    }
    else 
    {
        FragColor = vec4(vec3(uv.y), 1);
    }

    if(useColor == 1) {
        FragColor.rgb *= texture(ColorLUT, vec2(uv.x, 0.5)).rgb;
    }
}