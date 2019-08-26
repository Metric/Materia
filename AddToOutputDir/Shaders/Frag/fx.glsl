#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Background;
uniform int blendMode;

float AddSub(float a, float b) {
    if(a >= 0.5) {
        return min(1, max(0, a + b));
    }
    else {
        return min(1, max(0, b - a));
    }
}

void main() {
    vec4 f = texture(MainTex, UV);
    vec4 b = texture(Background, UV);
    vec3 rgb = vec3(0);
    float alpha = b.a;

    //Alpha blend
    if(blendMode == 0) {
        rgb = f.rgb * f.a + b.rgb * (1.0 - f.a);
        alpha = min(f.a + b.a, 1);
    }
    //add
    else if(blendMode == 1) {
        rgb = min(vec3(1), f.rgb + b.rgb);
        alpha = min(f.a + b.a, 1);
    }
    //max
    else if(blendMode == 2) {
        rgb = vec3(max(f.r,b.r), max(f.g, b.g), max(f.b, b.b));
        alpha = max(f.a, b.a);
    }
    //add sub
    else if(blendMode == 3) {
        rgb = vec3(AddSub(f.r, b.r), AddSub(f.g, b.g), AddSub(f.b, b.b));
        alpha = min(f.a + b.a, 1);
    }

    FragColor = vec4(rgb, alpha);    
}