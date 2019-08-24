#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Source;
uniform float maxDistance = 0.2;
uniform int sourceOnly = 0;

void main() {
    //note to self: the below can actually
    //be converted to work as an euclidean distance transform
    //simply change the >= to < 0.5
    //and remove 1.0 - on final dist calc
    vec2 offset = 1.0 / textureSize(MainTex, 0);
    vec3 rgb = texture(MainTex, UV).rgb;
    float r = (rgb.r + rgb.g + rgb.b) / 3.0;
    vec4 c = texture(Source, UV);

    if(r >= 0.5) {
        if(sourceOnly == 0) {
            FragColor = c + r;
        }
        else {
            FragColor = c;
        }
        return;
    }

    float dist = 1;
    vec4 last = c;
    for(float y = -0.5; y <= 0.5; y+=offset.y) {
        for(float x = -0.5; x <= 0.5; x+=offset.x) {
            vec2 pos = UV + vec2(x,y);
            if(pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1) {
                if(texture(MainTex, pos).r >= 0.5) {
                    float mdist = distance(pos,UV);
                    if(mdist < dist) {
                        last = texture(Source, pos);
                        dist = mdist;
                    }
                }
            }
        }
    }

    if(dist > maxDistance * maxDistance) {
        dist = 0;
        last = vec4(vec3(0), 1);
    }
    else {
        dist = 1.0 - dist / (maxDistance * maxDistance);
    }

    if(sourceOnly == 0) {
        FragColor = vec4(vec3(dist), 1) + c;
    }
    else {
        FragColor = last;
    }
}