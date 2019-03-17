#version 330 core
out vec4 FragColor;

const float near = 0.03;
const float far = 1000.0;

float linearizeDepth(float depth) {
    float z = depth * 2.0 - 1.0;
    return (2.0 * near * far) / (far + near - z * (far - near));
}

void main() {
    float d = 1.0 - linearizeDepth(gl_FragCoord.z) / far;
    FragColor = vec4(vec3(d), 1);
}