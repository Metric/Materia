#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D MainTex;
uniform float hue;
uniform float saturation;
uniform float lightness;

///HSL HELPERS
vec3 ToHSL(vec3 c) {
    float r = c.r;
    float g = c.g;
    float b = c.b;

    float mina = min(min(r, g), b);
    float maxa = max(max(r, g), b);
    float delta = maxa - mina;

    float h = 0;
    float s = 0;
    float l = (maxa + mina) * 0.5f;

    if (delta > 0.0001)
    {
        if (l < 0.5)
        {
            s = (delta / (maxa + mina));
        }
        else
        {
            s = (delta / (2.0 - maxa - mina));
        }

        if (r == maxa)
        {
            h = (g - b) / delta;
        }
        else if (g == maxa)
        {
            h = 2.0 + (b - r) / delta;
        }
        else if (b == maxa)
        {
            h = 4.0 + (r - g) / delta;
        }
    }

    return vec3(h,s,l);
}

float ColorCalc(float c, float t1, float t2) {
    if (c < 0) c += 1;
    if (c > 1) c -= 1;
    if (c < 1.0 / 6.0) return t1 + (t2 - t1) * 6.0 * c;
    if (c < 0.5) return t2;
    if (c < 2.0 / 3.0) return t1 + ((t2 - t1) * (2.0 / 3.0 - c) * 6.0);
    return t1;
}

vec3 FromHSL(vec3 c) {
    float H = c.r;
    float S = c.g;
    float L = c.b;

    float r;
    float g;
    float b;

    if (S <= 0.0001)
    {
        r = g = b = min(1, max(0, L));
    }
    else
    {
        float t1, t2;
        float th = H / 6.0f;

        if (L < 0.5)
        {
            t2 = L * (1.0 + S);
        }
        else
        {
            t2 = (L + S) - (L * S);
        }

        t1 = 2.0 * L - t2;

        float tr, tg, tb;
        tr = th + (1.0 / 3.0);
        tg = th;
        tb = th - (1.0 / 3.0);

        tr = ColorCalc(tr, t1, t2);
        tg = ColorCalc(tg, t1, t2);
        tb = ColorCalc(tb, t1, t2);

        r = min(1, max(0, tr));
        g = min(1, max(0, tg));
        b = min(1, max(0, tb));
    }

    return vec3(r,g,b);
}

//END HSL HELPERS

void main() {
    vec4 c = texture(MainTex, UV);
    vec3 hsl = ToHSL(c.rgb);
    hsl.r += hue;
    hsl.g += saturation;
    hsl.b += lightness;

    hsl.r = mod(hsl.r, 6.0);
    hsl.g = min(1, max(0, hsl.g));
    hsl.b = min(1, max(0, hsl.b));

    c.rgb = FromHSL(hsl);
    FragColor = c;
}