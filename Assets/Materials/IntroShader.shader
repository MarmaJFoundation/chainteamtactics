//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2020 //
/// Shader generate with Shadero 1.9.9                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/IntroShader"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
PixelUV_Size_1("PixelUV_Size_1", Range(1, 128)) = 128
RotationUV_Rotation_1("RotationUV_Rotation_1", Range(-360, 360)) = 90
RotationUV_Rotation_PosX_1("RotationUV_Rotation_PosX_1", Range(-1, 2)) = 0.5
RotationUV_Rotation_PosY_1("RotationUV_Rotation_PosY_1", Range(-1, 2)) =0.5
RotationUV_Rotation_Speed_1("RotationUV_Rotation_Speed_1", Range(-8, 8)) =0
_ColorGradients_Color1_1("_ColorGradients_Color1_1", COLOR) = (0.09803922,0.1176471,0.2352941,1)
_ColorGradients_Color2_1("_ColorGradients_Color2_1", COLOR) = (0.2156863,0.254902,0.3529412,1)
_ColorGradients_Color3_1("_ColorGradients_Color3_1", COLOR) = (0.2156863,0.254902,0.3529412,1)
_ColorGradients_Color4_1("_ColorGradients_Color4_1", COLOR) = (0.09803922,0.1176471,0.2352941,1)
_Generate_Starfield_PosX_1("_Generate_Starfield_PosX_1", Range(-1, 2)) = 0.5
_Generate_Starfield_PosY_1("_Generate_Starfield_PosY_1", Range(-1, 2)) = 0.582
_Generate_Starfield_Number_1("_Generate_Starfield_Number_1", Range(0, 20)) = 6.346
_Generate_Starfield_Size_1("_Generate_Starfield_Size_1", Range(0.01, 16)) = 10.664
_Generate_Starfield_Speed_1("_Generate_Starfield_Speed_1", Range(-100, 100)) = 1
_Generate_Circle_PosX_1("_Generate_Circle_PosX_1", Range(-1, 2)) = 0.5
_Generate_Circle_PosY_1("_Generate_Circle_PosY_1", Range(-1, 2)) = 0.5
_Generate_Circle_Size_1("_Generate_Circle_Size_1", Range(-1, 1)) = 0.3
_Generate_Circle_Dist_1("_Generate_Circle_Dist_1", Range(0, 1)) = 0.07
_MaskRGBA_Fade_1("_MaskRGBA_Fade_1", Range(0, 1)) = 1
_OperationBlend_Fade_1("_OperationBlend_Fade_1", Range(0, 1)) = 0.724
_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

// required for UI.Mask
[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
[HideInInspector]_Stencil("Stencil ID", Float) = 0
[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
[HideInInspector]_ColorMask("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off 

// required for UI.Mask
Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp]
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}

Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
float4 color    : COLOR;
};

sampler2D _MainTex;
float _SpriteFade;
float PixelUV_Size_1;
float RotationUV_Rotation_1;
float RotationUV_Rotation_PosX_1;
float RotationUV_Rotation_PosY_1;
float RotationUV_Rotation_Speed_1;
float4 _ColorGradients_Color1_1;
float4 _ColorGradients_Color2_1;
float4 _ColorGradients_Color3_1;
float4 _ColorGradients_Color4_1;
float _Generate_Starfield_PosX_1;
float _Generate_Starfield_PosY_1;
float _Generate_Starfield_Number_1;
float _Generate_Starfield_Size_1;
float _Generate_Starfield_Speed_1;
float _Generate_Circle_PosX_1;
float _Generate_Circle_PosY_1;
float _Generate_Circle_Size_1;
float _Generate_Circle_Dist_1;
float _MaskRGBA_Fade_1;
float _OperationBlend_Fade_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float2 RotationUV(float2 uv, float rot, float posx, float posy, float speed)
{
rot=rot+(_Time*speed*360);
uv = uv - float2(posx, posy);
float angle = rot * 0.01744444;
float sinX = sin(angle);
float cosX = cos(angle);
float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
uv = mul(uv, rotationMatrix) + float2(posx, posy);
return uv;
}
float4 Generate_Circle(float2 uv, float posX, float posY, float Size, float Smooth, float black)
{
float2 center = float2(posX, posY);
float dist = 1.0 - smoothstep(Size, Size + Smooth, length(center - uv));
float4 result = float4(1,1,1,dist);
if (black == 1) result = float4(dist, dist, dist, 1);
return result;
}
float4 OperationBlend(float4 origin, float4 overlay, float blend)
{
float4 o = origin; 
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) * (o.a+0.0000001);
o.a = saturate(o.a);
o = lerp(origin, o, blend);
return o;
}
float4 Color_Gradients(float4 txt, float2 uv, float4 col1, float4 col2, float4 col3, float4 col4)
{
float4 c1 = lerp(col1, col2, smoothstep(0., 0.33, uv.x));
c1 = lerp(c1, col3, smoothstep(0.33, 0.66, uv.x));
c1 = lerp(c1, col4, smoothstep(0.66, 1, uv.x));
c1.a = txt.a;
return c1;
}
float2 PixelUV(float2 uv, float x)
{
uv = floor(uv * x + 0.5) / x;
return uv;
}
float4 Generate_Starfield(float2 uv, float posx, float posy, float number, float size, float speed, float black)
{
float2 position = uv-float2(posx,posy);
float spx = speed * _Time;
float angle = atan2(position.y, position.x) / (size*3.14159265359);
angle -= floor(angle);
float rad = length(position);
float color = 0.0;
float a = angle * 360;
float aF = frac(a);
float aR = floor(a) + 1.;
float aR1 = frac(aR*frac(aR*.7331)*54.3);
float aR2 = frac(aR*frac(aR*.82345)*12.345);
float t = spx + aR1*100.;
float radDist = sqrt(aR2 + float(0));
float adist = radDist / rad;
float dist = (t + adist);
dist = abs(frac(dist) - 0.15);
color += max(0., .5 - dist*100. / adist)*(.5 - abs(aF - .5))*number / adist / radDist;
angle = frac(angle);
float4 result = color;
result.a = saturate(color + black);
return result;
}
float4 frag (v2f i) : COLOR
{
float2 PixelUV_1 = PixelUV(i.texcoord,PixelUV_Size_1);
float2 RotationUV_1 = RotationUV(PixelUV_1,RotationUV_Rotation_1,RotationUV_Rotation_PosX_1,RotationUV_Rotation_PosY_1,RotationUV_Rotation_Speed_1);
float4 _ColorGradients_1 = Color_Gradients(float4(0,0,0,1),RotationUV_1,_ColorGradients_Color1_1,_ColorGradients_Color2_1,_ColorGradients_Color3_1,_ColorGradients_Color4_1);
float4 _Generate_Starfield_1 = Generate_Starfield(PixelUV_1,_Generate_Starfield_PosX_1,_Generate_Starfield_PosY_1,_Generate_Starfield_Number_1,_Generate_Starfield_Size_1,_Generate_Starfield_Speed_1,0);
float4 _Generate_Circle_1 = Generate_Circle(PixelUV_1,_Generate_Circle_PosX_1,_Generate_Circle_PosY_1,_Generate_Circle_Size_1,_Generate_Circle_Dist_1,0);
float4 MaskRGBA_1=_Generate_Starfield_1;
MaskRGBA_1.a = lerp(_Generate_Circle_1.r * _Generate_Starfield_1.a, (1 - _Generate_Circle_1.r) * _Generate_Starfield_1.a,_MaskRGBA_Fade_1);
float4 OperationBlend_1 = OperationBlend(MaskRGBA_1, _ColorGradients_1, _OperationBlend_Fade_1); 
float4 FinalResult = OperationBlend_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
