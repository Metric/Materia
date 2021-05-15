#version 330 core
out vec4 FragColor;
in vec3 localPos;

uniform sampler2D hdrMap;

const float M_PI = 3.14159265359;

vec2 toRadialCoords(vec3 coords)
{
    vec3 normalizedCoords = normalize(coords);
    float latitude = acos(normalizedCoords.y);
    float longitude = atan(normalizedCoords.x, normalizedCoords.z);
    vec2 sphereCoords = vec2(longitude, latitude) * vec2(0.5F / M_PI, 1.0F / M_PI);
    return vec2(0.5F, 1.0F) - sphereCoords;
}

void main()
{		
    vec2 uv = toRadialCoords(localPos); // make sure to normalize localPos
    vec3 color = texture(hdrMap, uv).rgb;
    
    FragColor = vec4(color, 1.0);
}