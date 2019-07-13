#version 410 core
layout (vertices = 3) out;

uniform vec3 cameraPosition;

in vec3 WorldPos[];
in vec3 Normal[];
in vec2 UV[];
in vec4 Tangent[];
in vec3 ObjectPos[];

out vec3 WorldPos_ES[];
out vec3 Normal_ES[];
out vec2 UV_ES[];
out vec4 Tangent_ES[];
out vec3 ObjectPos_ES[];

float GetDynamicTessLevel(float Distance0, float Distance1) 
{
    float dist = (Distance0 + Distance1) * 0.5;
    return min(10, max(1, 10 - dist));
}

void main() 
{
    WorldPos_ES[gl_InvocationID] = WorldPos[gl_InvocationID];
    Normal_ES[gl_InvocationID] = Normal[gl_InvocationID];
    UV_ES[gl_InvocationID] = UV[gl_InvocationID];
    Tangent_ES[gl_InvocationID] = Tangent[gl_InvocationID];
    ObjectPos_ES[gl_InvocationID] = ObjectPos[gl_InvocationID];

    float dist0 = distance(cameraPosition, WorldPos[0]);
    float dist1 = distance(cameraPosition, WorldPos[1]);
    float dist2 = distance(cameraPosition, WorldPos[2]);

    gl_TessLevelOuter[0] = GetDynamicTessLevel(dist1, dist2);
    gl_TessLevelOuter[1] = GetDynamicTessLevel(dist2, dist0);
    gl_TessLevelOuter[2] = GetDynamicTessLevel(dist0, dist1);
    gl_TessLevelInner[0] = gl_TessLevelOuter[2];
}

