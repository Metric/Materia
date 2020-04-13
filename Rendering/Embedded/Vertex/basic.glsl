#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 localPos;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main()
{
    localPos = aPos;  
    gl_Position =  projectionMatrix * viewMatrix * vec4(localPos, 1.0);
}