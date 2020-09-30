
#version 330 core

struct AppData {
   vec2 UV;
   vec3 WorldPos;
   vec4 Clip;
};

in AppData data;

out vec4 FragColor;

uniform sampler2D MainTex;
uniform sampler2D GradientTex;
uniform sampler2D ScreenBuffer;

uniform vec2 gradientStart = vec2(0,0);
uniform vec2 gradientEnd = vec2(1,1);
uniform int fillType = 0;

uniform int clipBelow = 0;

float gradientAxial(vec2 uv, vec2 start, vec2 end)
{
    vec2 d1 = end - start;
    vec2 d2 = uv - start;
    float dt = dot(d1,d2);
    float dist = distance(end,start);
    float dd = dt / dist;
    return dd;
}

float gradientAxialReflected(vec2 uv, vec2 start, vec2 end)
{
    float dd = gradientAxial(uv, start, end);
    float rd = min(1.0 - dd, dd) * 2.0;
    return rd;
}

float gradientRadial(vec2 uv, vec2 start, vec2 end)
{
    float mr = distance(start,end);
    float l = length(uv - 0.5 - start) / mr;
    return 1.0 - l;
}

void main() {
    vec2 size = textureSize(GradientTex, 0);

    //solid color
    if (fillType == 0)
    {
        //gradienttex is also used to store the solid color
        vec4 gc = texture(GradientTex, data.UV);

        if (gc.a <= 0.01f) {
            discard;
        }

        gc.rgb *= gc.a;

        FragColor = gc;
    }
    //linear gradient
    else if(fillType == 1)
    {
        vec2 uv = data.UV;
        float g = gradientAxial(uv, gradientStart, gradientEnd);
        vec4 gc = texelFetch(GradientTex, ivec2(max(0, min(g * size.x, size.x - 1)), 0), 0);

        if (g < 0 || g > 1) 
        {
            discard;
        }

        if (gc.a <= 0.01f) {
            discard;
        }

         gc.rgb *= gc.a;

        FragColor = gc;
    }
    //radial gradient
    else if(fillType == 2)
    {
        vec2 uv = data.UV;
        float g = gradientRadial(uv, gradientStart, gradientEnd);
        vec4 gc = texelFetch(GradientTex, ivec2(max(0, min((1.0 - g) * size.x, size.x - 1)), 0), 0);

        if (1.0 - g < 0 || 1.0 - g > 1) 
        {
            discard;
        }

        if (gc.a <= 0.01f) {
            discard;
        }

        gc.rgb *= gc.a;

        FragColor = gc;
    }
    //linear reflected gradient
    else if(fillType == 3) 
    {
        vec2 uv = data.UV;
        float g = gradientAxialReflected(uv, gradientStart, gradientEnd);
        vec4 gc = texelFetch(GradientTex, ivec2(max(0, min((1.0 - g) * size.x, size.x - 1)), 0), 0);

        if (1.0 - g < 0 || 1.0 - g > 1) 
        {
            discard;
        }

        if (gc.a <= 0.01f) {
            discard;
        }

        gc.rgb *= gc.a;

        FragColor = gc;
    }
    //pattern + color
    else if(fillType == 4)
    {
        //gradienttex is also used to store the solid color
        vec4 gc = texture(GradientTex, data.UV);
        vec4 c = texture(MainTex, data.UV) * gc;

        if (c.a <= 0.01f) {
            discard;
        }

        c.rgb *= c.a;

        FragColor = c;
    }
}