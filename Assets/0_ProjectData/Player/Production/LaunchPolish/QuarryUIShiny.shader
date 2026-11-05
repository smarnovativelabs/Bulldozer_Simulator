Shader "Quarry/UI Shiny Atlas Safe"
{
 Properties {
  [PerRendererData] _MainTex ("Sprite",2D)="white" {}
  _Color ("Tint",Color)=(1,1,1,1)
  _ParamTex ("Plugin parameters",2D)="white" {}
  _StencilComp ("Stencil Comparison",Float)=8
  _Stencil ("Stencil ID",Float)=0
  _StencilOp ("Stencil Operation",Float)=0
  _StencilWriteMask ("Stencil Write Mask",Float)=255
  _StencilReadMask ("Stencil Read Mask",Float)=255
  _ColorMask ("Color Mask",Float)=15
  [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Alpha Clip",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   sampler2D _MainTex,_ParamTex;fixed4 _Color,_TextureSampleAdd;float4 _ClipRect;
   struct Input {float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float2 effect:TEXCOORD1;};
   struct Output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;float2 effect:TEXCOORD1;float2 local:TEXCOORD2;};
   Output vert(Input v){Output o;o.local=v.vertex.xy;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color*_Color;o.uv=v.uv;o.effect=v.effect;return o;}
   fixed4 frag(Output i):SV_Target {
    fixed4 color=(tex2D(_MainTex,i.uv)+_TextureSampleAdd)*i.color;
    fixed4 settings=tex2D(_ParamTex,float2(.25,i.effect.y));
    fixed gloss=tex2D(_ParamTex,float2(.75,i.effect.y)).x;
    half position=settings.x*2-.5;
    half shine=smoothstep(0,max(.01,settings.z),1-saturate(abs(i.effect.x-position)/max(.001,settings.y)));
    color.rgb+=color.a*shine*.5*settings.w*lerp(half3(1,1,1),color.rgb*7,gloss);
    #ifdef UNITY_UI_CLIP_RECT
    color.a*=UnityGet2DClipping(i.local,_ClipRect);
    #endif
    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a-.001);
    #endif
    return color;
   }
   ENDCG
  }
 }
}
