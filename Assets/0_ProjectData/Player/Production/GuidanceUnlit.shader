Shader "Bulldozer/GuidanceUnlit"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {float4 positionOS:POSITION;half4 color:COLOR;};
            struct Varyings {float4 positionCS:SV_POSITION;half4 color:COLOR;};
            Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.color=v.color;return o;}
            half4 frag(Varyings i):SV_Target{return i.color;}
            ENDHLSL
        }
    }
}
