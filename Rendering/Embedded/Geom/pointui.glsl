
#version 330 core
layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

uniform mat4 projectionMatrix;
uniform mat4 modelMatrix;

in vec2 inPosition[];
in vec2 inSize[];

out vec2 uv;

void main() {
	gl_Position = projectionMatrix * modelMatrix * vec4(inPosition[0], 0, 1);
	uv = vec2(0,0);
	EmitVertex();
	gl_Position = projectionMatrix * modelMatrix * vec4(inPosition[0] + vec2(inSize[0].x, 0), 0, 1);
	uv = vec2(1,0);
	EmitVertex();
	gl_Position = projectionMatrix * modelMatrix * vec4(inPosition[0] + vec2(0, inSize[0].y), 0, 1);
	uv = vec2(0,1);
	EmitVertex();
	gl_Position = projectionMatrix * modelMatrix * vec4(inPosition[0] + inSize[0], 0, 1);
	uv = vec2(1,1);
	EmitVertex();
	EndPrimitive();
}