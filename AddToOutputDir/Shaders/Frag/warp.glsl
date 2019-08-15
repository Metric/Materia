#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform sampler2D Warp;
uniform float intensity = 1.0;

vec3 createNormal(vec2 uv) {
    ivec2 size = textureSize(Warp, 0);
    
    float width = float(size.x);
    float height = float(size.y);

    vec2 rpos = vec2(uv.x * width, uv.y * height);

    //uses the normal algorithm kernel
    //to calculate proper direction vectors
    //automagically converts colored to an average grayscale
    float left = (rpos.x - 1) / width;
    float right = (rpos.x + 1) / width;
    float top = (rpos.y - 1) / height;
    float bottom = (rpos.y + 1) / height;

    vec4 t = texture(Warp, vec2(uv.x, top));
    vec4 b = texture(Warp, vec2(uv.x, bottom));
    vec4 l = texture(Warp, vec2(left, uv.y));
    vec4 r = texture(Warp, vec2(right, uv.y));

    vec3 norm = vec3(0, 0, 0.1);

    if(uv.x == 0 || uv.y == 0 || uv.x == 1 || uv.y == 1) 
    {
        norm.x = 0;
        norm.y = 0;
    }
    else 
    {
        vec4 cx = (l - r);
        vec4 cy = (t - b);

        norm.x = (cx.r + cx.g + cx.b) / 3.0;
        norm.y = (cy.r + cy.g + cy.b) / 3.0;
    }

    norm = normalize(norm);
    return norm;
}

void main() {
    vec2 uv = UV;
    vec2 n = createNormal(uv).xy * 0.5;
    FragColor = texture(MainTex, uv + n * intensity);
}