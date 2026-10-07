Shader "Game2Week/화면 외곽선"
{
    Properties { _Width("두께",Float)=1 _Strength("농도",Range(0,1))=.35 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off Cull Off ZTest Always
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Width, _Strength;
            CBUFFER_END
            half4 Frag(Varyings i):SV_Target
            {
                float2 uv=i.texcoord, pixel=_Width/_ScaledScreenParams.xy;
                float center=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float a=LinearEyeDepth(SampleSceneDepth(uv+float2(pixel.x,0)),_ZBufferParams);
                float b=LinearEyeDepth(SampleSceneDepth(uv+float2(0,pixel.y)),_ZBufferParams);
                float edge=step(.018,max(abs(a-center),abs(b-center))/max(.1,center));
                half4 color=SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);
                color.rgb*=1-edge*_Strength; return color;
            }
            ENDHLSL
        }
    }
}
