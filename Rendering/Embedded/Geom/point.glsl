#version 330 core
layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

struct AppData {
   vec4 Color;
   mat4 Scale;
   vec3 WorldPos;
   mat4 Rotation;
   vec4 ClipPos;
   float Size;
};

in AppData data[];
out vec2 uv;
out AppData appData;

void main() {
	AppData d = data[0];
	appData = d;
	//calculate local scale + rotation of vertex
	vec3 pos = (d.Scale * d.Rotation * vec4(-0.5 * d.Size, -0.5 * d.Size, 0, 1)).xyz;
	//apply world pos translation and convert to clip space
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos + d.WorldPos, 1);
	uv = vec2(0,0);
	EmitVertex();
	pos = (d.Scale * d.Rotation * vec4(0.5 * d.Size, -0.5 * d.Size, 0, 1)).xyz;
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos + d.WorldPos, 1);
	uv = vec2(1,0);
	appData = d;
	EmitVertex();
	pos = (d.Scale * d.Rotation * vec4(-0.5 * d.Size, 0.5 * d.Size, 0, 1)).xyz;
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos + d.WorldPos, 1);
	uv = vec2(0,1);
	appData = d;
	EmitVertex();
	pos = (d.Scale * d.Rotation * vec4(0.5 * d.Size, 0.5 * d.Size, 0, 1)).xyz;
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(pos + d.WorldPos, 1);
	uv = vec2(1,1);
	appData = d;
	EmitVertex();
	EndPrimitive();
}