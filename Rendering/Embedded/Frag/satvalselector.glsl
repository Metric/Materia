
#version 330 core
in vec2 uv;

out vec4 FragColor;

uniform vec4 color;
uniform sampler2D MainTex;
uniform vec2 selected = vec2(0,0);
uniform float hue = 0;
uniform int mode = 0;

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

//HSV HELPERS
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
//END HSV HELPERS


void main() {
    float distance = (uv.x - selected.x) * (uv.x - selected.x) + (uv.y - selected.y) * (uv.y - selected.y);

    bool ring3 = (distance >= 0.0005 && distance < 0.001);
    bool ring2 = (distance >= 0.00025 && distance < 0.0005);
    bool ring1 = (distance >= 0.0001 && distance < 0.00025);

    vec3 rgbhue = vec3(0);
    
    if (mode == 0) {
        rgbhue = hsv2rgb(vec3(hue, uv.x, uv.y));
    }
    else {
        rgbhue = FromHSL(vec3(hue * 6.0f, uv.x, uv.y));
    }

    FragColor = (ring3 || ring1 ? vec4(0,0,0,1) : (ring2 ? vec4(1,1,1,1) : vec4(rgbhue, 1)));
}