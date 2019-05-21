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
//LinearLight = 21
//PinLight = 22
//HardMix = 23
//Exclusion = 24

uniform int blendMode = 1;
uniform float alpha = 1;
uniform int hasMask = 0;

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

vec3 Copy(vec4 a, vec4 b) {
    if(a.a >= b.a) {
        return a.rgb;
    }
    else {
        return b.rgb;
    }
}

float AddSub(float a, float b) {
    if(a < 0.5) {
        return min(1, max(0, a + b));
    }
    else {
        return min(1, max(0, b - a));
    }
}

float Multiple(float a, float b) {
    return min(1, max(0, a * b));
}

float Screen(float a, float b) {
    return min(1, max(0, 1 - (1 - a) * (1 - b)));
}

float Divide(float a, float b) {
    return min(1, max(0, b / a));
}

float ColorDodge(float a, float b) {
    return min(1, max(0, b / (1 - a)));
}

float LinearDodge(float a, float b) {
    return min(1, max(0, a + b));
}

float ColorBurn(float a, float b) {
    return min(1, max(0, 1 - (1 - b) / a));
}

float LinearBurn(float a, float b) {
    return min(1, max(0, a + b - 1));
}

float Overlay(float a, float b) {
    if(b < 0.5) {
        return 2 * a * b;
    }
    else {
        return 1 - 2 * (1 - a) * (1 - b);
    }
}

float SoftLight(float a, float b) {
    if(a < 0.5) {
        return (2 * a - 1) * (b * (b * b)) + b;
    }
    else {
        return (2 * a - 1) * (sqrt(b) - b) + b;
    }
}

float HardLight(float a, float b) {
    if(a < 0.5) {
        return 2 * a * b;
    }
    else {
        return 1 - 2 * (1 - a) * (1- b);
    }
}

float LinearLight(float a, float b) {
    return b + 2 * a - 1;
}

float VividLight(float a, float b) {
   if(a < 0.5) {
       return 1 - (1 - b) / (2 * a);
   }
   else {
       return b / (2 * (1 - a));
   }
}

float PinLight(float a, float b) {
    if(b < 2 * a - 1) {
        return 2 * a - 1;
    }
    else if(2 * a - 1 < b && b < 2 * a) {
        return b;
    } 
    else {
        return 2 * a;
    }
}

float HardMix(float a, float b) {
    if(a < 1 - b) {
        return 0;
    }
    else {
        return 1;
    }
}

float Exclusion(float a, float b) {
    return a + b - 2 * a * b;
}

float Subtract(float a, float b) {
    return min(1, max(0, b - a));
}

float Difference(float a, float b) {
    return min(1, max(0, abs(a - b)));
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

    h.r = h2.r;

    return FromHSL(h2);
}

vec3 Saturation(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.g = h2.g;

    return FromHSL(h2);
}

vec3 Color(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.r = h2.r;
    h.g = h2.g;

    return FromHSL(h2);
}

vec3 Luminosity(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.b = h2.b;

    return FromHSL(h2);
}

void main() {
    vec4 a = texture(Foreground, UV);
    vec4 b = texture(Background, UV);

    vec4 final = vec4(0);

    if(blendMode == 0) {
        final.r = AddSub(a.r * alpha, b.r);
        final.g = AddSub(a.g * alpha, b.g);
        final.b = AddSub(a.b * alpha, b.b);
    }
    else if(blendMode == 1) {
        final.rgb = Copy(a,b);
    }
    else if(blendMode == 2) {
        final.r = Multiple(a.r * alpha, b.r);
        final.g = Multiple(a.g * alpha, b.g);
        final.b = Multiple(a.b * alpha, b.b);
    }
    else if(blendMode == 3) {
        final.r = Screen(a.r * alpha, b.r);
        final.g = Screen(a.g * alpha, b.g);
        final.b = Screen(a.b * alpha, b.b);
    }
    else if(blendMode == 4) {
        final.r = Overlay(a.r * alpha, b.r);
        final.g = Overlay(a.g * alpha, b.g);
        final.b = Overlay(a.b * alpha, b.b);
    }
    else if(blendMode == 5) {
        final.r = HardLight(a.r * alpha, b.r);
        final.g = HardLight(a.g * alpha, b.g);
        final.b = HardLight(a.b * alpha, b.b);
    }
    else if(blendMode == 6) {
        final.r = SoftLight(a.r * alpha, b.r);
        final.g = SoftLight(a.g * alpha, b.g);
        final.b = SoftLight(a.b * alpha, b.b);
    }
    else if(blendMode == 7) {
        final.r = ColorDodge(a.r * alpha, b.r);
        final.g = ColorDodge(a.g * alpha, b.g);
        final.b = ColorDodge(a.b * alpha, b.b);
    }
    else if(blendMode == 8) {
        final.r = LinearDodge(a.r * alpha, b.r);
        final.g = LinearDodge(a.g * alpha, b.g);
        final.b = LinearDodge(a.b * alpha, b.b);
    }
    else if(blendMode == 9) {
        final.r = ColorBurn(a.r * alpha, b.r);
        final.g = ColorBurn(a.g * alpha, b.g);
        final.b = ColorBurn(a.b * alpha, b.b);
    }
    else if(blendMode == 10) {
        final.r = LinearBurn(a.r * alpha, b.r);
        final.g = LinearBurn(a.g * alpha, b.g);
        final.b = LinearBurn(a.b * alpha, b.b);
    }
    else if(blendMode == 11) {
        final.r = VividLight(a.r * alpha, b.r);
        final.g = VividLight(a.g * alpha, b.g);
        final.b = VividLight(a.b * alpha, b.b);
    }
    else if(blendMode == 12) {
        final.r = Divide(a.r * alpha, b.r);
        final.g = Divide(a.g * alpha, b.g);
        final.b = Divide(a.b * alpha, b.b);
    }
    else if(blendMode == 13) {
        final.r = Subtract(a.r * alpha, b.r);
        final.g = Subtract(a.g * alpha, b.g);
        final.b = Subtract(a.b * alpha, b.b);
    }
    else if(blendMode == 14) {
        final.r = Difference(a.r * alpha, b.r);
        final.g = Difference(a.g * alpha, b.g);
        final.b = Difference(a.b * alpha, b.b);
    }
    else if(blendMode == 15) {
        final.r = Darken(a.r * alpha, b.r);
        final.g = Darken(a.g * alpha, b.g);
        final.b = Darken(a.b * alpha, b.b);
    }
    else if(blendMode == 16) {
        final.r = Lighten(a.r * alpha, b.r);
        final.g = Lighten(a.g * alpha, b.g);
        final.b = Lighten(a.b * alpha, b.b);
    }
    else if(blendMode == 17) {
        final.rgb = Hue(a.rgb * alpha,b.rgb);
    }
    else if(blendMode == 18) {
        final.rgb = Saturation(a.rgb * alpha, b.rgb);   
    }
    else if(blendMode == 19) {
        final.rgb = Color(a.rgb * alpha, b.rgb);
    }
    else if(blendMode == 20) {
        final.rgb = Luminosity(a.rgb * alpha, b.rgb);
    }
    else if(blendMode == 21) {
        final.r = LinearLight(a.r * alpha, b.r);
        final.g = LinearLight(a.g * alpha, b.g);
        final.b = LinearLight(a.b * alpha, b.b);
    }
    else if(blendMode == 22) {
        final.r = PinLight(a.r * alpha, b.r);
        final.g = PinLight(a.g * alpha, b.g);
        final.b = PinLight(a.b * alpha, b.b);
    }
    else if(blendMode == 23) {
        final.r = HardMix(a.r * alpha, b.r);
        final.g = HardMix(a.g * alpha, b.g);
        final.b = HardMix(a.b * alpha, b.b);
    }
    else if(blendMode == 24) {
        final.r = Exclusion(a.r * alpha, b.r);
        final.g = Exclusion(a.g * alpha, b.g);
        final.b = Exclusion(a.b * alpha, b.b);
    }

    float m = 1;
    if(hasMask == 1) {
        m = texture(Mask, UV).r;        
    }
    final.a = max(b.a,a.a);
    final *= m;
    FragColor = final;
}