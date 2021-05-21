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

///alpha modes
//Background = 0
//Foregound = 1
//Min = 2
//Max = 3
//Average = 4
//Add = 5

uniform int alphaMode = 5;
uniform float luminosity = 1;

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
    float l = (maxa + mina) * 0.5;

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
        float th = H / 6.0;

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

//One, OneMinusSrcAlpha
vec3 Copy(vec3 a, vec3 b, float alpha) {
    return clamp(a + b * (1.0 - alpha), vec3(0), vec3(1));
}

float AddSub(float a, float b) {
    if(a >= 0.5) {
        return clamp(a + b, 0, 1);
    }
    else {
        return clamp(b - a, 0, 1);
    }
}

float Multiply(float a, float b) {
    return clamp(a * b, 0, 1);
}

float Screen(float a, float b) {
    return clamp(1 - (1 - a) * (1 - b), 0, 1);
}

float Divide(float a, float b) {
    return clamp(b / a, 0, 1);
}

float ColorDodge(float a, float b) {
    return clamp(b / (1 - a), 0, 1);
}

float LinearDodge(float a, float b) {
    return clamp(a + b, 0, 1);
}

float ColorBurn(float a, float b) {
    return clamp(1 - (1 - b) / a, 0, 1);
}

float LinearBurn(float a, float b) {
    return clamp(a + b - 1, 0, 1);
}

float Overlay(float a, float b) {
    if(b < 0.5) {
        return clamp(2 * a * b, 0, 1);
    }
    else {
        return clamp(1 - 2 * (1 - a) * (1 - b), 0, 1);
    }
}

float SoftLight(float a, float b) {
    if(a < 0.5) {
        return clamp((2 * a - 1) * (b * (b * b)) + b, 0, 1);
    }
    else {
        return clamp((2 * a - 1) * (sqrt(b) - b) + b, 0, 1);
    }
}

float HardLight(float a, float b) {
    if(a < 0.5) {
        return clamp(2 * a * b, 0, 1);
    }
    else {
        return clamp(1 - 2 * (1 - a) * (1- b), 0, 1);
    }
}

float LinearLight(float a, float b) {
    return clamp(b + 2 * a - 1, 0, 1);
}

float VividLight(float a, float b) {
   if(a < 0.5) {
       return clamp(1 - (1 - b) / (2 * a), 0, 1);
   }
   else {
       return clamp(b / (2 * (1 - a)), 0, 1);
   }
}

float PinLight(float a, float b) {
    if(b < 2 * a - 1) {
        return clamp(2 * a - 1, 0, 1);
    }
    else if(2 * a - 1 < b && b < 2 * a) {
        return clamp(b, 0, 1);
    } 
    else {
        return clamp(2 * a, 0, 1);
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
    return clamp(a + b - 2 * a * b, 0, 1);
}

float Subtract(float a, float b) {
    return clamp(b - a, 0, 1);
}

float Difference(float a, float b) {
    return clamp(abs(a - b), 0, 1);
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

    return FromHSL(h);
}

vec3 Saturation(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.g = h2.g;

    return FromHSL(h);
}

vec3 Color(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.r = h2.r;
    h.g = h2.g;

    return FromHSL(h);
}

vec3 Luminosity(vec3 a, vec3 b) {
    vec3 h = ToHSL(a);
    vec3 h2 = ToHSL(b);

    h.b = h2.b;

    return FromHSL(h);
}

void main() {
    vec4 a = clamp(texture(Foreground, UV), vec4(0), vec4(1));
    vec4 b = clamp(texture(Background, UV), vec4(0), vec4(1));

    vec4 final = vec4(0);

    float m = 1;
    if(hasMask == 1) {
        m = clamp(texture(Mask, UV).r, 0, 1);       
    }

    //determine base alpha
    //background
    if (alphaMode == 0) {
        final.a = b.a;
    }
    //foreground
    else if (alphaMode == 1) {
        final.a = a.a;
    }
    //minimum of the two
    else if (alphaMode == 2) {
        final.a = min(a.a, b.a);
    }
    //maximum of the two
    else if (alphaMode == 3) {
        final.a = max(a.a, b.a);
    }
    //average
    else if (alphaMode == 4) {
        final.a = (a.a + b.a) * 0.5;
    }
    //add - default
    else {
        final.a = clamp(a.a + b.a, 0, 1);
    }

    final.a *= alpha * m;

    if(blendMode == 0) {
        final.r = AddSub(a.r,b.r);
        final.g = AddSub(a.g,b.g);
        final.b = AddSub(a.b,b.b);
    }
    else if(blendMode == 1) {
        final.rgb = Copy(a.rgb, b.rgb, a.a);
    }
    else if(blendMode == 2) {
        final.r = Multiply(a.r,b.r);
        final.g = Multiply(a.g,b.g);
        final.b = Multiply(a.b,b.b);
    }
    else if(blendMode == 3) {
        final.r = Screen(a.r,b.r);
        final.g = Screen(a.g,b.g);
        final.b = Screen(a.b,b.b);
    }
    else if(blendMode == 4) {
        final.r = Overlay(a.r,b.r);
        final.g = Overlay(a.g,b.g);
        final.b = Overlay(a.b,b.b);
    }
    else if(blendMode == 5) {
        final.r = HardLight(a.r,b.r);
        final.g = HardLight(a.g,b.g);
        final.b = HardLight(a.b,b.b);
    }
    else if(blendMode == 6) {
        final.r = SoftLight(a.r,b.r);
        final.g = SoftLight(a.g,b.g);
        final.b = SoftLight(a.b,b.b);
    }
    else if(blendMode == 7) {
        final.r = ColorDodge(a.r,b.r);
        final.g = ColorDodge(a.g,b.g);
        final.b = ColorDodge(a.b,b.b);
    }
    else if(blendMode == 8) {
        final.r = LinearDodge(a.r,b.r);
        final.g = LinearDodge(a.g,b.g);
        final.b = LinearDodge(a.b,b.b);
    }
    else if(blendMode == 9) {
        final.r = ColorBurn(a.r,b.r);
        final.g = ColorBurn(a.g,b.g);
        final.b = ColorBurn(a.b,b.b);
    }
    else if(blendMode == 10) {
        final.r = LinearBurn(a.r,b.r);
        final.g = LinearBurn(a.g,b.g);
        final.b = LinearBurn(a.b,b.b);
    }
    else if(blendMode == 11) {
        final.r = VividLight(a.r,b.r);
        final.g = VividLight(a.g,b.g);
        final.b = VividLight(a.b,b.b);
    }
    else if(blendMode == 12) {
        final.r = Divide(a.r,b.r);
        final.g = Divide(a.g,b.g);
        final.b = Divide(a.b,b.b);
    }
    else if(blendMode == 13) {
        final.r = Subtract(a.r,b.r);
        final.g = Subtract(a.g,b.g);
        final.b = Subtract(a.b,b.b);
    }
    else if(blendMode == 14) {
        final.r = Difference(a.r,b.r);
        final.g = Difference(a.g,b.g);
        final.b = Difference(a.b,b.b);
    }
    else if(blendMode == 15) {
        final.r = Darken(a.r, b.r);
        final.g = Darken(a.g, b.g);
        final.b = Darken(a.b, b.b);
    }
    else if(blendMode == 16) {
        final.r = Lighten(a.r, b.r);
        final.g = Lighten(a.g, b.g);
        final.b = Lighten(a.b, b.b);
    }
    else if(blendMode == 17) {
        final.rgb = Hue(a.rgb, b.rgb);
    }
    else if(blendMode == 18) {
        final.rgb = Saturation(a.rgb, b.rgb);  
    }
    else if(blendMode == 19) {
        final.rgb = Color(a.rgb, b.rgb);
    }
    else if(blendMode == 20) {
        final.rgb = Luminosity(a.rgb, b.rgb);
    }
    else if(blendMode == 21) {
        final.r = LinearLight(a.r,b.r);
        final.g = LinearLight(a.g,b.g);
        final.b = LinearLight(a.b,b.b);
    }
    else if(blendMode == 22) {
        final.r = PinLight(a.r,b.r);
        final.g = PinLight(a.g,b.g);
        final.b = PinLight(a.b,b.b);
    }
    else if(blendMode == 23) {
        final.r = HardMix(a.r,b.r);
        final.g = HardMix(a.g,b.g);
        final.b = HardMix(a.b,b.b);
    }
    else if(blendMode == 24) {
        final.r = Exclusion(a.r,b.r);
        final.g = Exclusion(a.g,b.g);
        final.b = Exclusion(a.b,b.b);
    }

    FragColor = clamp(final, vec4(0), vec4(1));
}