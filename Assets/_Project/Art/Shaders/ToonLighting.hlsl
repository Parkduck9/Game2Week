#ifndef GAME2WEEK_TOON_LIGHTING
#define GAME2WEEK_TOON_LIGHTING
#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif
void ToonLighting_float(float4 BaseColor, float3 Normal, float3 View, out float3 Out)
{
    float3 direction = normalize(float3(.4,.8,.3));
    float3 lightColor = 1;
    #ifndef SHADERGRAPH_PREVIEW
    Light main = GetMainLight();
    direction = main.direction;
    lightColor = main.color;
    #endif
    float diffuse = saturate(dot(normalize(Normal), direction));
    float band = diffuse < .25 ? .42 : (diffuse < .65 ? .72 : 1);
    float rim = pow(1 - saturate(dot(normalize(Normal), normalize(View))), 3) * .18;
    Out = BaseColor.rgb * band * lerp(float3(1,1,1),lightColor,.35) + rim;
}
void ToonLighting_half(half4 BaseColor, half3 Normal, half3 View, out half3 Out)
{
    float3 result; ToonLighting_float(BaseColor, Normal, View, result); Out = result;
}
#endif
