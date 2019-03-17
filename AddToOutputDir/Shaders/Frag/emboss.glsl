#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform float width;
uniform float height;

uniform float azimuth;
uniform float elevation;

uniform sampler2D MainTex;

void main() {
    //azimuth and elevation are in radians already
    vec3 ldir = normalize(vec3(cos(azimuth),sin(azimuth),sin(elevation)));
    vec2 rpos = vec2(UV.x * width, UV.y * height);

    //uses the normal algorithm kernel
    //to calculate proper direction vectors
    //automagically converts colored to an average grayscale
    float left = (rpos.x - 1) / width;
    float right = (rpos.x + 1) / width;
    float top = (rpos.y - 1) / height;
    float bottom = (rpos.y + 1) / height;

    vec4 t = texture(MainTex, vec2(UV.x, top));
    vec4 b = texture(MainTex, vec2(UV.x, bottom));
    vec4 l = texture(MainTex, vec2(left, UV.y));
    vec4 r = texture(MainTex, vec2(right, UV.y));

    vec3 norm = vec3(0,0,1);

    if(UV.x == 0 || UV.y == 0 || UV.x == 1 || UV.y == 1) 
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

    float NDotL = min(1, max(0, dot(norm, ldir)));
    FragColor = vec4(NDotL,NDotL,NDotL, 1);
}