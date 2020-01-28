#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 Brightness;

struct AppData {
    vec3 Normal;
    vec2 UV;
    vec3 WorldPos;
    mat3 TBN;
    vec3 ObjectPos;
    vec4 ClipPos;
    vec3 T;
    vec3 B;
    vec3 N;
};

in AppData data;

uniform vec3 cameraPosition;

//Texture Data
uniform sampler2D albedo;
uniform sampler2D metallicMap;
uniform sampler2D roughnessMap;
uniform sampler2D occlusionMap;
uniform sampler2D normalMap;
uniform sampler2D heightMap;
uniform sampler2D brdfLUT;
uniform sampler2D irradianceMap;
uniform sampler2D prefilterMap;
uniform sampler2D thicknessMap;
uniform sampler2D emissionMap;

//Light Data
uniform vec3 lightPosition = vec3(0,1,1);
uniform vec3 lightColor = vec3(1,1,1);
uniform float lightPower = 1;

//POM Data
uniform float heightScale = 0.2;
uniform float occlusionClipBias = -1.0;
uniform int displace = 0;

//Index of refraction
uniform float refraction = 0.04;

//PI / INV PI constants
const float INV_PI = 0.31830988618;
const float PI = 3.14159265359;

uniform float near = 0.03;
uniform float far = 1000;

//SSS Related based on Dice Presentation: https://colinbarrebrisebois.com/2011/03/07/gdc-2011-approximating-translucency-for-a-fast-cheap-and-convincing-subsurface-scattering-look/
//this is the sigma distortion variable from the above slides
uniform float SSS_Distortion = 0.5;
//this is the extra ambient from the above slides
uniform float SSS_Ambient = 0;
//this is the intensity of the SSS
uniform float SSS_Power = 1;

struct Shading {
    vec3 diffuse;
    float metallic;
    float roughness;
    vec3 dielectric;
    vec3 normal;
    vec3 view;
    vec3 sss;
    vec2 uv;
    float attenuation;
};

vec3 getPOMOffset(AppData o, vec2 texCoords, vec3 viewDir)
{
    vec3 N = normalize(o.Normal);
    vec3 E = normalize(viewDir);

    vec2 texDx = dFdx(texCoords);
    vec2 texDy = dFdy(texCoords);

    vec3 sigmaX = dFdx(o.WorldPos);
    vec3 sigmaY = dFdy(o.WorldPos);

    vec3 r1 = cross(sigmaY, N);
    vec3 r2 = cross(N, sigmaX);

    float det = dot(sigmaX, r1);

    vec2 projVScr = (1.0 / det) * vec2(dot(r1, E), dot(r2,E));
    vec2 projTex = texDx * projVScr.x + texDy * projVScr.y;
    float projZ = dot(N,E) / heightScale;

    return vec3(projTex, projZ);
}

vec2 parallaxWorldMapping(AppData o, vec3 viewDir, sampler2D map) 
{
    vec2 texCoords = o.UV;

    const float maxSamples = 64;
    const float minSamples = 8;

    vec3 N = normalize(o.Normal);
    vec3 E = normalize(viewDir);

    vec3 projTex = getPOMOffset(o, texCoords, viewDir);

    float numLayers = mix(maxSamples, minSamples, abs(dot(E,N)));
    float layerDepth = 1.0 / numLayers;
    // depth of current layer
    float currentLayerDepth = 1.0;
    // the amount to shift the texture coordinates per layer (from vector P)
    vec2 deltaTexCoords = projTex.xy * heightScale / (projTex.z * numLayers);
  
    // get initial values
    vec2  currentTexCoords     = texCoords;

    float currentDepthMapValue = texture(map, currentTexCoords).r;
      
    while(currentLayerDepth > currentDepthMapValue)
    {
        // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
        // get depthmap value at current texture coordinates
        currentDepthMapValue = texture(map, currentTexCoords).r;  
        // get depth of next layer
        currentLayerDepth -= layerDepth;  
    }
    
    // get texture coordinates before collision (reverse operations)
    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;

    // get depth after and before collision for linear interpolation
    float afterDepth  = currentDepthMapValue - currentLayerDepth;
    float beforeDepth = texture(map, prevTexCoords).r - currentLayerDepth - layerDepth;
 
    // interpolation of texture coordinates
    float weight = afterDepth / (afterDepth - beforeDepth);
    vec2 finalTexCoords = mix(currentTexCoords, prevTexCoords, weight);

    return finalTexCoords;
}

vec3 unpackNormal(AppData o, vec2 uv, sampler2D map) 
{
    vec3 norm = texture(map, uv).rgb;
    
    if(length(norm) == 0) {
        return o.Normal;
    }
    
    norm = norm * 2.0 - 1.0;
    //this is using unnormalized T,B,N because we use mikktspace tangents
    norm = normalize(norm.x * o.T + norm.y * o.B + norm.z * o.N);
    return norm;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0) 
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

vec3 fresnelSchlickRoughness(float cosTheta, vec3 F0, float roughness)
{
    return F0 + (max(vec3(1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
}

float distrubtionGGX(vec3 N, vec3 H, float roughness) 
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N,H), 0.0);
    float NdotH2 = NdotH * NdotH;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    return a2 / denom;
}

float geometrySchlickGGX(float NdotV, float roughness) 
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    return num / denom;
}

float geometrySmith(vec3 N, vec3 V, vec3 L, float roughness) 
{
    float NdotV = max(dot(N,V), 0.0);
    float NdotL = max(dot(N,L), 0.0);
    float ggx2 = geometrySchlickGGX(NdotV, roughness);
    float ggx1 = geometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

vec2 toRadialCoords(vec3 coords)
{
  vec3 normalizedCoords = normalize(coords);
	float latitude = acos(normalizedCoords.y);
	float longitude = atan(normalizedCoords.x, normalizedCoords.z);
	vec2 sphereCoords = vec2(longitude, latitude) * vec2(0.5 * INV_PI, INV_PI);
	return vec2(0.5, 1.0) - sphereCoords;
}

vec3 lighting(vec3 lo, vec3 pos, vec3 color, vec3 wPos, Shading shading) {
    vec3 F0 = shading.dielectric;
    vec3 N = shading.normal;
    vec3 V = shading.view;
    vec3 ldiffuse = shading.diffuse;
    float roughness = shading.roughness;
    float metallic = shading.metallic;
    float atten = shading.attenuation;

    vec3 Lo = lo;

    vec3 L = normalize(pos - wPos);
    vec3 H = normalize(V + L);
    vec3 F = fresnelSchlick(max(dot(H,V), 0.0), F0);

    float NdotL = max(dot(N,L), 0.0);
    float NdotV = max(dot(N,V), 0.0);

    vec3 radiance = color * atten;

    float NDF = distrubtionGGX(N,H,roughness);
    float G = geometrySmith(N,V,L,roughness);

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * NdotV * NdotL + 0.001;
    vec3 specular = numerator / denominator;

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    Lo += (kD * ldiffuse / PI + specular) * radiance * NdotL;

    return Lo;
}

float subsurfaceScattering(float atten, float thickness, vec3 L, vec3 N)
{
    vec3 vLTLight = normalize(L + N * SSS_Distortion);

    //the formula for the fLTDot provided in the slides produces weird artifacts
    //since it uses the View Dir and thus based on the viewing angle
    //weird light shadowing artifacts can occur as if the SSS is cutting off
    //To get proper world based SSS with the same formula
    //the view dir is simply replaced with the incoming normal
    //this produces excellent results from any angle
    //with no artifacts

    //we also removed the ltScale from the formula
    //and simply rely on the atten from the light
    float fLTDot = pow(max(0, dot(N,-vLTLight)), SSS_Power);
    float fLT = atten * (fLTDot + SSS_Ambient) * thickness;
    return fLT;
}

float lengthSqr(vec3 v) {
    return v.x * v.x + v.y * v.y + v.z * v.z;
}

void main() 
{
    AppData o = data;
    Shading shading;

    vec3 V = shading.view = normalize(cameraPosition - o.WorldPos);
    vec2 uv = o.UV;

    if(displace == 0)
    {
        uv = parallaxWorldMapping(o, cameraPosition - o.WorldPos, heightMap);

        if(occlusionClipBias > -1) {
            if(uv.x < 0 - occlusionClipBias || uv.x > 1 + occlusionClipBias || uv.y < 0 - occlusionClipBias || uv.y > 1 + occlusionClipBias)
                discard;
        }
    }

    shading.uv = uv;

    vec3 N = shading.normal = unpackNormal(o, uv, normalMap);
    vec3 R = reflect(V, N).xyz;

    float NdotV = max(dot(N,V), 0.001);

    vec4 color = texture(albedo, uv);
    vec3 ldiffuse = shading.diffuse = pow(color.rgb, vec3(2.2));

    float roughness = shading.roughness = texture(roughnessMap, uv).r;
    float metallic = shading.metallic = texture(metallicMap, uv).r;
    float ao = texture(occlusionMap, uv).r;

    vec3 F0 = vec3(refraction);
    F0 = shading.dielectric = mix(F0, ldiffuse.rgb, metallic);

    vec3 final = vec3(0);

    ////Lighting for Point Light
    float dist = length(lightPosition - o.WorldPos);
    float attenuation = 1.0 / (dist * dist) * lightPower;
    shading.attenuation = attenuation;
    vec3 L = normalize(lightPosition - o.WorldPos);
    vec3 Lo = vec3(0.0);
    Lo = lighting(Lo, lightPosition, lightColor, o.WorldPos, shading);
    ////

    ///SSS for point light
    float sss = subsurfaceScattering(attenuation, texture(thicknessMap, uv).r, L, N);
    final += ldiffuse * lightColor * sss;
    ///

    vec3 F = fresnelSchlickRoughness(NdotV, F0, roughness);
	vec3 iKs = F;
    vec3 iKd = vec3(1.0) - iKs;
    iKd *= 1.0 - metallic;
	
	//normal for irradiance map must be reversed
    vec3 irradiance = texture(irradianceMap, toRadialCoords(-N)).rgb;
    vec3 diffuseIBL = irradiance * ldiffuse;

    const float MAX_REFLECT_LOD = 4.0;
    vec3 prefilteredColor = textureLod(prefilterMap, toRadialCoords(R), roughness * MAX_REFLECT_LOD).rgb;
    vec2 envBRDF = texture(brdfLUT, vec2(NdotV, roughness)).rg;
    vec3 specularIBL = prefilteredColor * (F * envBRDF.x + envBRDF.y);

    vec3 ambient = (iKd * diffuseIBL + specularIBL) * ao;
    //add ambient + light + emission
    final += (ambient + Lo) + texture(emissionMap, uv).rgb;

    //draw to secondary color attachment for bloom
    float bright = lengthSqr(final);
    Brightness = vec4(0);
    if(bright > 3) {
        Brightness = vec4(clamp(final, vec3(0), vec3(1)), 1.0);
    }

    //HDR
    //final = final / (final + vec3(1.0)); 

    //GAMMA
    //final = pow(final, vec3(1.0/2.2));

    FragColor = vec4(final, color.a);
    
}