#version 330 core
out vec4 FragColor;
in vec3 localPos;

uniform sampler2D hdrMap;

const vec2 invAtan = vec2(0.1591, 0.3183);
vec2 toRadial(vec3 v)
{
    vec2 uv = vec2(atan(v.z, v.x), asin(v.y));
    uv *= invAtan;
    uv += 0.5;
    return uv;
}

void main()
{		
    vec2 uv = toRadial(normalize(localPos)); // make sure to normalize localPos
    vec3 color = texture(hdrMap, uv).rgb;
    
    FragColor = vec4(color, 1.0);
}