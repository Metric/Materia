#version 330 core

struct AppData {
   vec4 Color;
   mat4 Scale;
   vec3 WorldPos;
   mat4 Rotation;
   vec4 ClipPos;
   float Size;
};

in AppData appData;
in vec2 uv;

out vec4 FragColor;

uniform sampler2D MainTex;
uniform float layerOpacity = 1;

void main() {
   float len = length(uv - 0.5);
   if (len >= 0.45 && len <= 0.5)
   {
		FragColor = vec4(0.5,0.5,0.5,1);
   }
   else 
   {
		FragColor = vec4(0,0,0,0);
   }
}