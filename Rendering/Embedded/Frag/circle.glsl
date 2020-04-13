#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform float width;
uniform float height;

uniform float radius;
uniform float outline;

void main() {
    vec2 rpos = vec2((UV.x - 0.5) * width, (UV.y - 0.5) * height);
    float sqr = rpos.x * rpos.x + rpos.y * rpos.y;

    float rad = (radius * (min(width,height) * 0.5));
    float radsqr = rad * rad;

    if(outline > 0) 
    {
        if(sqr >= radsqr - outline * radsqr && sqr <= radsqr) 
        {
            FragColor = vec4(1,1,1,1);
        }
        else
        {
            FragColor = vec4(0);
        }
    }
    else 
    {
        if (sqr <= radsqr) 
        {
            FragColor = vec4(1,1,1,1);
        }
        else {
            FragColor = vec4(0);
        }
    }
}