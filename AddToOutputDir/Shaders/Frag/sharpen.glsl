#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float intensity;
float kernel[9] = float[9](
                    0.077847,0.123317,0.077847,
                    0.123317,0.195346,0.123317,
                    0.077847,0.123317,0.077847
                  );

//this is an unsharp mask sharpen 
//it uses a 3x3 gaussian blur kernel
//which is then subtracted from the original
//pixel and multiplied by intensity
//then added back to the original pixel

void main() {
    vec2 offset = 1.0 / textureSize(MainTex, 0);
    
    vec4 sum = vec4(0);
    int oidx = 0;
    for(int y = -1; y <= 1; y++) {
        for(int x = -1; x <= 1; x++) {
            vec4 c = texture(MainTex, min(vec2(1.0), max(vec2(0), UV + offset * vec2(x,y))));
            sum += c * kernel[oidx];
            oidx += 1;
        }
    }

    vec4 o = texture(MainTex, UV);
    FragColor = o + (o - sum) * intensity;
}