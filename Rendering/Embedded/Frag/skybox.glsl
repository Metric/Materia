#version 330 core
layout(location = 0) out vec4 FragColor;
layout(location = 1) out vec4 Brightness;

in vec3 localPos;
  
uniform samplerCube hdrMap;
  
void main()
{
    Brightness = vec4(0);
    vec3 envColor = texture(hdrMap, localPos).rgb;
    FragColor = vec4(envColor, 1.0);
}