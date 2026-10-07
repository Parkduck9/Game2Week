Shader "Game2Week/그라디언트 하늘"
{
    Properties { _Top("위 색",Color)=(.12,.2,.35,1) _Bottom("아래 색",Color)=(.45,.58,.68,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Background" "RenderType"="Background" }
        Pass
        {
            Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Top,_Bottom;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; }; struct Varyings { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            Varyings Vert(Attributes v){ Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.direction=v.positionOS.xyz;return o; }
            half4 Frag(Varyings i):SV_Target{return lerp(_Bottom,_Top,saturate(normalize(i.direction).y*.5+.5));}
            ENDHLSL
        }
    }
}
