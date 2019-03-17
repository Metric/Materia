#version 330 core
out vec4 FragColor;
in vec2 UV;

uniform sampler2D Foreground;
uniform sampler2D Background;
uniform sampler2D Mask;

//these are more advanced than what
//opengl provides by default

//AddSub is based on substance designer definition
//AddSub = 0,
//Copy = 1,
//Multiply = 2,
//Screen = 3,
//Overlay = 4,
//HardLight = 5,
//SoftLight = 6,
//ColorDodge = 7,
//LinearDodge = 8,
//ColorBurn = 9,
//LinearBurn = 10,
//VividLight = 11,
//Divide = 12,
//Subtract = 13,
//Difference = 14,
//Darken = 15,
//Lighten = 16,
//Hue = 17,
//Saturation = 18,
//Color = 19,
//Luminosity = 20

uniform int blendMode;
uniform float alpha;
uniform int hasMask;

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
        if (l < 0.5f)
        {
            s = (delta / (maxa + mina));
        }
        else
        {
            s = (delta / (2.0f - maxa - mina));
        }

        if (r == maxa)
        {
            h = (g - b) / delta;
        }
        else if (g == maxa)
        {
            h = 2f + (b - r) / delta;
        }
        else if (b == maxa)
        {
            h = 4f + (r - g) / delta;
        }
    }

    return vec3(h,s,l);
}

float ColorCalc(float c, float t1, float t2) {
    if (c < 0) c += 1;
    if (c > 1) c -= 1;
    if (6f * c < 1f) return t1 + (t2 - t1) * 6f * c;
    if (2f * c < 1f) return t2;
    if (3f * c < 2f) return t1 + (t2 - t1) * (2f / 3f - c) * 6f;
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

        if (L < 0.5f)
        {
            t2 = L * (1f + S);
        }
        else
        {
            t2 = (L + S) - (L * S);
        }

        t1 = 2f * L - t2;

        float tr, tg, tb;
        tr = th + (1f / 3f);
        tg = th;
        tb = th - (1f / 3f);

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

float AddSub(float a, float b) {
    if(b > 0.5) {
        return min(1, max(0, a + b));
    }
    else {
        return min(1, max(0, b - a));
    }
}

float Copy(float a, float b, float ba) {
    return mix(a,b,ba);
}

float Multiple(float a, float b) {
    return min(1, max(0, a * b));
}

float Screen(float a, float b) {
    return min(1, max(0, 1 - (1 - a) * (1 - b)));
}

float Divide(float a, float b) {
    return min(1, max(0, b / (a + 0.0001)));
}

float Overlay(float a, float b) {
    if(a < 0.5) {
        return Multiple(a,b);
    }
    else {
        return Screen(a,b);
    }
}

float HardLight(float a, float b) {
    float r = Multiple(a,b);
    return Screen(r, a);
}

float SoftLight(float a, float b) {
    return min(1, max(0, (1.0 - 2.0 * b) * (a * a) + 2 * b * a));
}

//0.0001 to prevent division by 0
float ColorDodge(float a, float b) {
    return min(1, max(0, b / (1 - (a + 0.0001))));
}

float LinearDodge(float a, float b) {
    return min(1, max(0, a + b));
}

float ColorBurn(float a, float b) {
    return min(1, max(0, 1 - (1 - b) / (a + 0.0001)));
}

float LinearBurn(float a, float b) {
    return min(1, max(0, a + b - 1));
}

float VividLight(float a, float b) {
    float r = ColorDodge(a ,b);
    return ColorBurn(r, a);
}

float Subtract(float a, float b) {
    return min(1, max(0, b - a));
}

float Difference(float a, float b) {
    if(a > b) {
        return a - b;
    }
    else {
        return b - a;
    }
}

float Darken(float a, float b) {
    return min(a,b);
}

float Lighten(float a, float b) {
    return max(a,b);
}

vec3 Hue(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h2.r = h.r;

    return FromHSL(h2);
}

vec3 Saturation(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h2.g = h.g;

    return FromHSL(h2);
}

vec3 Color(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h2.r = h.r;
    h2.g = h.g;

    return FromHSL(h2);
}

vec3 Luminosity(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h2.b = h.b;

    return FromHSL(h2);
}

void main() {
    vec4 a = texture(Foreground, UV);
    vec4 b = texture(Background, UV);

    vec4 final = vec4(0);

    if(blendMode == 0) {
        final.r = AddSub(b.r * alpha, a.r * alpha);
        final.g = AddSub(b.g * alpha, a.g * alpha);
        final.b = AddSub(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 1) {
        final.r = Copy(b.r * alpha, a.r * alpha, a.a);
        final.g = Copy(b.g * alpha, a.g * alpha, a.a);
        final.b = Copy(b.b * alpha, a.b * alpha, a.a);
    }
    else if(blendMode == 2) {
        final.r = Multiple(b.r * alpha, a.r * alpha);
        final.g = Multiple(b.g * alpha, a.g * alpha);
        final.b = Multiple(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 3) {
        final.r = Screen(b.r * alpha, a.r * alpha);
        final.g = Screen(b.g * alpha, a.g * alpha);
        final.b = Screen(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 4) {
        final.r = Overlay(b.r * alpha, a.r * alpha);
        final.g = Overlay(b.g * alpha, a.g * alpha);
        final.b = Overlay(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 5) {
        final.r = HardLight(b.r * alpha, a.r * alpha);
        final.g = HardLight(b.g * alpha, a.g * alpha);
        final.b = HardLight(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 6) {
        final.r = SoftLight(b.r * alpha, a.r * alpha);
        final.g = SoftLight(b.g * alpha, a.g * alpha);
        final.b = SoftLight(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 7) {
        final.r = ColorDodge(b.r * alpha, a.r * alpha);
        final.g = ColorDodge(b.g * alpha, a.g * alpha);
        final.b = ColorDodge(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 8) {
        final.r = LinearDodge(b.r * alpha, a.r * alpha);
        final.g = LinearDodge(b.g * alpha, a.g * alpha);
        final.b = LinearDodge(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 9) {
        final.r = ColorBurn(b.r * alpha, a.r * alpha);
        final.g = ColorBurn(b.g * alpha, a.g * alpha);
        final.b = ColorBurn(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 10) {
        final.r = LinearBurn(b.r * alpha, a.r * alpha);
        final.g = LinearBurn(b.g * alpha, a.g * alpha);
        final.b = LinearBurn(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 11) {
        final.r = VividLight(b.r * alpha, a.r * alpha);
        final.g = VividLight(b.g * alpha, a.g * alpha);
        final.b = VividLight(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 12) {
        final.r = Divide(b.r * alpha, a.r * alpha);
        final.g = Divide(b.g * alpha, a.g * alpha);
        final.b = Divide(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 13) {
        final.r = Subtract(b.r * alpha, a.r * alpha);
        final.g = Subtract(b.g * alpha, a.g * alpha);
        final.b = Subtract(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 14) {
        final.r = Difference(b.r * alpha, a.r * alpha);
        final.g = Difference(b.g * alpha, a.g * alpha);
        final.b = Difference(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 15) {
        final.r = Darken(b.r * alpha, a.r * alpha);
        final.g = Darken(b.g * alpha, a.g * alpha);
        final.b = Darken(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 16) {
        final.r = Lighten(b.r * alpha, a.r * alpha);
        final.g = Lighten(b.g * alpha, a.g * alpha);
        final.b = Lighten(b.b * alpha, a.b * alpha);
    }
    else if(blendMode == 17) {
        final.rgb = Hue(b.rgb * alpha,a.rgb * alpha);
    }
    else if(blendMode == 18) {
        final.rgb = Saturation(b.rgb * alpha, a.rgb * alpha);
    }
    else if(blendMode == 19) {
        final.rgb = Color(b.rgb * alpha, a.rgb * alpha);
    }
    else if(blendMode == 20) {
        final.rgb = Luminosity(b.rgb * alpha, a.rgb * alpha);
    }

    float m = 1;
    if(hasMask == 1) {
        m = texture(Mask, UV);        
    }
    final.a = Copy(b.a, a.a, a.a);
    final *= m;
    FragColor = final;
}