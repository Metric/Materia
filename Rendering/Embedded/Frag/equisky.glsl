#version 330 core
layout(location = 0) out vec4 FragColor;
layout(location = 1) out vec4 Brightness;

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
    Brightness = vec4(0);
    vec2 uv = toRadialCoords(localPos); // make sure to normalize localPos
    vec3 envColor = texture(hdrMap, uv).rgb;

    envColor = envColor / (envColor + vec3(1.0));
    envColor = pow(envColor, vec3(1.0 / 2.2));

    FragColor = vec4(envColor, 1.0);
}
