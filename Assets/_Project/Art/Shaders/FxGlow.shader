Shader "Game2Week/효과 잔광"
{
    Properties { [HDR] _BaseColor("색",Color)=(1,1,1,1) _UseVertexColor("정점 색 사용",Float)=1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor; float _UseVertexColor;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION;float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION;float4 color:COLOR; };
            Varyings Vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;return o;}
            half4 Frag(Varyings i):SV_Target{return lerp(float4(1,1,1,1),i.color,_UseVertexColor)*_BaseColor;}
            ENDHLSL
        }
    }
}
