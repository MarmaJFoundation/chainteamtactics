//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2020 //
/// Shader generate with Shadero 1.9.9                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
RotationUV_Rotation_1("RotationUV_Rotation_1", Range(-360, 360)) = 90
RotationUV_Rotation_PosX_1("RotationUV_Rotation_PosX_1", Range(-1, 2)) = 0.5
RotationUV_Rotation_PosY_1("RotationUV_Rotation_PosY_1", Range(-1, 2)) =0.5
RotationUV_Rotation_Speed_1("RotationUV_Rotation_Speed_1", Range(-8, 8)) =0
_ColorGradients_Color1_1("_ColorGradients_Color1_1", COLOR) = (0.08627451,0.4117647,0.764706,1)
_ColorGradients_Color2_1("_ColorGradients_Color2_1", COLOR) = (0.0627451,0.2156863,0.6117647,1)
_ColorGradients_Color3_1("_ColorGradients_Color3_1", COLOR) = (0.0627451,0.2156863,0.6117647,1)
_ColorGradients_Color4_1("_ColorGradients_Color4_1", COLOR) = (0.08627451,0.4117647,0.764706,1)
_Generate_Circle_PosX_1("_Generate_Circle_PosX_1", Range(-1, 2)) = 0.5
_Generate_Circle_PosY_1("_Generate_Circle_PosY_1", Range(-1, 2)) = 0.5
_Generate_Circle_Size_1("_Generate_Circle_Size_1", Range(-1, 1)) = 0.1
_Generate_Circle_Dist_1("_Generate_Circle_Dist_1", Range(0, 1)) = 0.5
_TintRGBA_Color_1("_TintRGBA_Color_1", COLOR) = (0.254902,0.8431373,0.8431373,1)
_Add_Fade_1("_Add_Fade_1", Range(0, 4)) = 0.3
AnimatedOffsetUV_X_1("AnimatedOffsetUV_X_1", Range(-1, 1)) = 0.1
AnimatedOffsetUV_Y_1("AnimatedOffsetUV_Y_1", Range(-1, 1)) = 0.1
AnimatedOffsetUV_ZoomX_1("AnimatedOffsetUV_ZoomX_1", Range(1, 10)) = 2
AnimatedOffsetUV_ZoomY_1("AnimatedOffsetUV_ZoomY_1", Range(1, 10)) = 1
AnimatedOffsetUV_Speed_1("AnimatedOffsetUV_Speed_1", Range(-1, 1)) = 0.1
OffsetUV_X_1("OffsetUV_X_1", Range(-1, 1)) = 0
OffsetUV_Y_1("OffsetUV_Y_1", Range(-1, 1)) = 0
OffsetUV_ZoomX_1("OffsetUV_ZoomX_1", Range(0.1, 10)) = 10
OffsetUV_ZoomY_1("OffsetUV_ZoomY_1", Range(0.1, 10)) = 10
_NewTex_1("NewTex_1(RGB)", 2D) = "white" { }
_ColorGradients_Color1_2("_ColorGradients_Color1_2", COLOR) = (0.2588235,0.8431373,0.8392158,1)
_ColorGradients_Color2_2("_ColorGradients_Color2_2", COLOR) = (0.07843138,0.627451,0.8078432,1)
_ColorGradients_Color3_2("_ColorGradients_Color3_2", COLOR) = (0.07843138,0.627451,0.8078432,1)
_ColorGradients_Color4_2("_ColorGradients_Color4_2", COLOR) = (0.2588235,0.8431373,0.8392158,1)
_Add_Fade_2("_Add_Fade_2", Range(0, 4)) = 0.1
_ColorHSV_Hue_1("_ColorHSV_Hue_1", Range(0, 360)) = 180
_ColorHSV_Saturation_1("_ColorHSV_Saturation_1", Range(0, 2)) = 1
_ColorHSV_Brightness_1("_ColorHSV_Brightness_1", Range(0, 2)) = 1
_TurnC64_Fade_1("_TurnC64_Fade_1", Range(0, 1)) = 1
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
float RotationUV_Rotation_1;
float RotationUV_Rotation_PosX_1;
float RotationUV_Rotation_PosY_1;
float RotationUV_Rotation_Speed_1;
float4 _ColorGradients_Color1_1;
float4 _ColorGradients_Color2_1;
float4 _ColorGradients_Color3_1;
float4 _ColorGradients_Color4_1;
float _Generate_Circle_PosX_1;
float _Generate_Circle_PosY_1;
float _Generate_Circle_Size_1;
float _Generate_Circle_Dist_1;
float4 _TintRGBA_Color_1;
float _Add_Fade_1;
float AnimatedOffsetUV_X_1;
float AnimatedOffsetUV_Y_1;
float AnimatedOffsetUV_ZoomX_1;
float AnimatedOffsetUV_ZoomY_1;
float AnimatedOffsetUV_Speed_1;
float OffsetUV_X_1;
float OffsetUV_Y_1;
float OffsetUV_ZoomX_1;
float OffsetUV_ZoomY_1;
sampler2D _NewTex_1;
float4 _ColorGradients_Color1_2;
float4 _ColorGradients_Color2_2;
float4 _ColorGradients_Color3_2;
float4 _ColorGradients_Color4_2;
float _Add_Fade_2;
float _ColorHSV_Hue_1;
float _ColorHSV_Saturation_1;
float _ColorHSV_Brightness_1;
float _TurnC64_Fade_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float2 AnimatedOffsetUV(float2 uv, float offsetx, float offsety, float zoomx, float zoomy, float speed)
{
speed *=_Time*25;
uv += float2(offsetx*speed, offsety*speed);
uv = fmod(uv * float2(zoomx, zoomy), 1);
return uv;
}
float2 OffsetUV(float2 uv, float offsetx, float offsety, float zoomx, float zoomy)
{
uv += float2(offsetx, offsety);
uv = fmod(uv * float2(zoomx, zoomy), 1);
return uv;
}

float2 OffsetUVClamp(float2 uv, float offsetx, float offsety, float zoomx, float zoomy)
{
uv += float2(offsetx, offsety);
uv = fmod(clamp(uv * float2(zoomx, zoomy), 0.0001, 0.9999), 1);
return uv;
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
float4 TintRGBA(float4 txt, float4 color)
{
float3 tint = dot(txt.rgb, float3(.222, .707, .071));
tint.rgb *= color.rgb;
txt.rgb = lerp(txt.rgb,tint.rgb,color.a);
return txt;
}
float4 ColorHSV(float4 RGBA, float HueShift, float Sat, float Val)
{

float4 RESULT = float4(RGBA);
float a1 = Val*Sat;
float a2 = HueShift*3.14159265 / 180;
float VSU = a1*cos(a2);
float VSW = a1*sin(a2);

RESULT.x = (.299*Val + .701*VSU + .168*VSW)*RGBA.x
+ (.587*Val - .587*VSU + .330*VSW)*RGBA.y
+ (.114*Val - .114*VSU - .497*VSW)*RGBA.z;

RESULT.y = (.299*Val - .299*VSU - .328*VSW)*RGBA.x
+ (.587*Val + .413*VSU + .035*VSW)*RGBA.y
+ (.114*Val - .114*VSU + .292*VSW)*RGBA.z;

RESULT.z = (.299*Val - .3*VSU + 1.25*VSW)*RGBA.x
+ (.587*Val - .588*VSU - 1.05*VSW)*RGBA.y
+ (.114*Val + .886*VSU - .203*VSW)*RGBA.z;

return RESULT;
}
float4 Generate_Circle(float2 uv, float posX, float posY, float Size, float Smooth, float black)
{
float2 center = float2(posX, posY);
float dist = 1.0 - smoothstep(Size, Size + Smooth, length(center - uv));
float4 result = float4(1,1,1,dist);
if (black == 1) result = float4(dist, dist, dist, 1);
return result;
}
float4 Color_Gradients(float4 txt, float2 uv, float4 col1, float4 col2, float4 col3, float4 col4)
{
float4 c1 = lerp(col1, col2, smoothstep(0., 0.33, uv.x));
c1 = lerp(c1, col3, smoothstep(0.33, 0.66, uv.x));
c1 = lerp(c1, col4, smoothstep(0.66, 1, uv.x));
c1.a = txt.a;
return c1;
}
float4 TurnC64(float4 txt, float value)
{
float3 a = float3(0, 0, 0);
#define Turn(n) a = lerp(n, a, step (length (a-txt.rgb), length (n-txt.rgb)));	
Turn(float3(0, 0, 0));
Turn(float3(1, 1, 1));
Turn(float3(116, 67, 53) / 256);
Turn(float3(124, 172, 186) / 256);
Turn(float3(123, 72, 144) / 256);
Turn(float3(100, 151, 79) / 256);
Turn(float3(64, 50, 133) / 256);
Turn(float3(191, 205, 122) / 256);
Turn(float3(123, 91, 47) / 256);
Turn(float3(79, 69, 0) / 256);
Turn(float3(163, 114, 101) / 256);
Turn(float3(80, 80, 80) / 256);
Turn(float3(120, 120, 120) / 256);
Turn(float3(164, 215, 142) / 256);
Turn(float3(120, 106, 189) / 256);
Turn(float3(159, 159, 150) / 256);
a = lerp(txt.rgb,a,value);
return float4(a, txt.a);
}
float4 frag (v2f i) : COLOR
{
float2 RotationUV_1 = RotationUV(i.texcoord,RotationUV_Rotation_1,RotationUV_Rotation_PosX_1,RotationUV_Rotation_PosY_1,RotationUV_Rotation_Speed_1);
float4 _ColorGradients_1 = Color_Gradients(float4(0,0,0,1),RotationUV_1,_ColorGradients_Color1_1,_ColorGradients_Color2_1,_ColorGradients_Color3_1,_ColorGradients_Color4_1);
float4 _Generate_Circle_1 = Generate_Circle(i.texcoord,_Generate_Circle_PosX_1,_Generate_Circle_PosY_1,_Generate_Circle_Size_1,_Generate_Circle_Dist_1,0);
float4 TintRGBA_1 = TintRGBA(_Generate_Circle_1,_TintRGBA_Color_1);
_ColorGradients_1 = lerp(_ColorGradients_1,_ColorGradients_1*_ColorGradients_1.a + TintRGBA_1*TintRGBA_1.a,_Add_Fade_1);
float2 AnimatedOffsetUV_1 = AnimatedOffsetUV(i.texcoord,AnimatedOffsetUV_X_1,AnimatedOffsetUV_Y_1,AnimatedOffsetUV_ZoomX_1,AnimatedOffsetUV_ZoomY_1,AnimatedOffsetUV_Speed_1);
float2 OffsetUV_1 = OffsetUV(AnimatedOffsetUV_1,OffsetUV_X_1,OffsetUV_Y_1,OffsetUV_ZoomX_1,OffsetUV_ZoomY_1);
float4 NewTex_1 = tex2D(_NewTex_1,OffsetUV_1);
float4 _ColorGradients_2 = Color_Gradients(NewTex_1,RotationUV_1,_ColorGradients_Color1_2,_ColorGradients_Color2_2,_ColorGradients_Color3_2,_ColorGradients_Color4_2);
_ColorGradients_1 = lerp(_ColorGradients_1,_ColorGradients_1*_ColorGradients_1.a + _ColorGradients_2*_ColorGradients_2.a,_Add_Fade_2);
float4 _ColorHSV_1 = ColorHSV(_ColorGradients_1,_ColorHSV_Hue_1,_ColorHSV_Saturation_1,_ColorHSV_Brightness_1);
float4 TurnC64_1 = TurnC64(_ColorHSV_1,_TurnC64_Fade_1);
float4 FinalResult = TurnC64_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
