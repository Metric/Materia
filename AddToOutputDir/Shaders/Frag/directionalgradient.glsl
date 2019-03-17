#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform int horizontal = 1

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
}