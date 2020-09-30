#version 330 core
layout (location = 0) in vec2 pos;
layout (location = 1) in vec2 scale;
layout (location = 2) in vec4 color;
layout (location = 3) in float rotation;
layout (location = 4) in float size;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

uniform float zIndex = 0;

struct AppData {
   vec4 Color;
   mat4 Scale;
   vec3 WorldPos;
   mat4 Rotation;
   vec4 ClipPos;
   float Size;
};

out AppData data;

void main() {
    AppData o;
  
    o.Color = color;
    o.Rotation = mat4(cos(rotation), -sin(rotation), 0, 0,
                        sin(rotation), cos(rotation), 0, 0,
                        0,0,1,0,
                        0,0,0,1);
    o.Scale = mat4(scale.x, 0, 0, 0,
                        0, scale.y, 0, 0,
                        0,0,1,0,
                        0,0,0,1);
    o.Size = size;

    o.WorldPos = vec3(pos, zIndex);

    vec4 clip = projectionMatrix * viewMatrix * modelMatrix * vec4(o.WorldPos, 1);

    o.ClipPos = clip;
    data = o;

    gl_Position = clip;
}