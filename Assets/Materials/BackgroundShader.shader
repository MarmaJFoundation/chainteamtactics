//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.3                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/BackgroundShader"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
//[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
ColorLimit("ColorLimit", Range(1, 128)) = 15
PixelXYUV_SizeX_1("PixelXYUV_SizeX_1", Range(1, 128)) = 128
PixelXYUV_SizeY_1("PixelXYUV_SizeY_1", Range(1, 128)) = 128
PinchUV_Size_1("PinchUV_Size_1", Range(0, 0.5)) = 0.1
AnimatedOffsetUV_X_1("AnimatedOffsetUV_X_1", Range(-1, 1)) = 0.5
AnimatedOffsetUV_Y_1("AnimatedOffsetUV_Y_1", Range(-1, 1)) = 0
AnimatedOffsetUV_ZoomX_1("AnimatedOffsetUV_ZoomX_1", Range(1, 10)) = 1
AnimatedOffsetUV_ZoomY_1("AnimatedOffsetUV_ZoomY_1", Range(1, 10)) = 1
AnimatedOffsetUV_Speed_1("AnimatedOffsetUV_Speed_1", Range(-1, 1)) = 0.05
DistortionUV_WaveX_1("DistortionUV_WaveX_1", Range(0, 128)) = 50
DistortionUV_WaveY_1("DistortionUV_WaveY_1", Range(0, 128)) = 50
DistortionUV_DistanceX_1("DistortionUV_DistanceX_1", Range(0, 1)) = 0.3
DistortionUV_DistanceY_1("DistortionUV_DistanceY_1", Range(0, 1)) = 0.3
DistortionUV_Speed_1("DistortionUV_Speed_1", Range(-2, 2)) = 0.1
RotationUV_Rotation_1("RotationUV_Rotation_1", Range(-360, 360)) = -90
RotationUV_Rotation_PosX_1("RotationUV_Rotation_PosX_1", Range(-1, 2)) = 0.5
RotationUV_Rotation_PosY_1("RotationUV_Rotation_PosY_1", Range(-1, 2)) = 0.5
RotationUV_Rotation_Speed_1("RotationUV_Rotation_Speed_1", Range(-8, 8)) =0
_ColorGradients_Color1_1("_ColorGradients_Color1_1", COLOR) = (0.6467604,0.8113208,0.7600126,1)
_ColorGradients_Color2_1("_ColorGradients_Color2_1", COLOR) = (0.1862762,0.1862762,0.245283,1)
_ColorGradients_Color3_1("_ColorGradients_Color3_1", COLOR) = (0,1,0,1)
_ColorGradients_Color4_1("_ColorGradients_Color4_1", COLOR) = (0.1176471,0.1215686,0.1176471,1)


}

SubShader
{

Tags {"Queue" = "Background" "IgnoreProjector" = "true" "RenderType" = "Opaque" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
ZWrite Off 
Blend Off 
Cull Off
Lighting Off
Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
//#pragma multi_compile _ PIXELSNAP_ON
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
float PixelXYUV_SizeX_1;
float PixelXYUV_SizeY_1;
float PinchUV_Size_1;
float AnimatedOffsetUV_X_1;
float AnimatedOffsetUV_Y_1;
float AnimatedOffsetUV_ZoomX_1;
float AnimatedOffsetUV_ZoomY_1;
float AnimatedOffsetUV_Speed_1;
float DistortionUV_WaveX_1;
float DistortionUV_WaveY_1;
float DistortionUV_DistanceX_1;
float DistortionUV_DistanceY_1;
float DistortionUV_Speed_1;
float RotationUV_Rotation_1;
float RotationUV_Rotation_PosX_1;
float RotationUV_Rotation_PosY_1;
float RotationUV_Rotation_Speed_1;
float ColorLimit;
float4 _ColorGradients_Color1_1;
float4 _ColorGradients_Color2_1;
float4 _ColorGradients_Color3_1;
float4 _ColorGradients_Color4_1;

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
float time = _Time;
uv += float2(offsetx * time, offsety * time);
uv = fmod(uv * float2(zoomx, zoomy), 1);
return uv;
}
float2 DistortionUV(float2 p, float WaveX, float WaveY, float DistanceX, float DistanceY, float Speed)
{
Speed *= _Time * 100;
p.x = p.x + sin(p.y * WaveX + Speed) * DistanceX * 0.05;
p.y = p.y + cos(p.x * WaveY + Speed) * DistanceY * 0.05;
return p;
}
float2 RotationUV(float2 uv, float rot, float posx, float posy, float speed)
{
rot = rot + (_Time * speed * 360);
uv = uv - float2(posx, posy);
float angle = rot * 0.01744444;
float sinX = sin(angle);
float cosX = cos(angle);
float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
uv = mul(uv, rotationMatrix) + float2(posx, posy);
return uv;
}
float2 PixelXYUV(float2 uv, float x, float y)
{
float2 pos = float2(x * 1.5, y * 1.5);
uv = floor(uv * pos+0.5) / pos;
return uv;
}
float4 Color_Gradients(float4 txt, float2 uv, float4 col1, float4 col2, float4 col3, float4 col4)
{
float4 c1 = lerp(col1, col2, smoothstep(0., 0.33, uv.x));
c1 = lerp(c1, col3, smoothstep(0.33, 0.66, uv.x));
c1 = lerp(c1, col4, smoothstep(0.66, 1, uv.x));
c1.a = txt.a;
return c1;
}
float2 PinchUV(float2 uv, float size)
{
float2 m = float2(0.5, 0.5);
float2 d = uv - m;
float r = sqrt(dot(d, d));
float power = (2.0 * 3.141592 / (2.0 * sqrt(dot(m, m)))) * (-size+0.001);
float bind = 0.5;
uv = m + normalize(d) * atan(r * -power * 10.0) * bind / atan(-power * bind * 10.0);
return uv;
}
float4 frag (v2f i) : COLOR
{
float2 PixelXYUV_1 = PixelXYUV(i.texcoord,PixelXYUV_SizeX_1,PixelXYUV_SizeY_1);
float2 PinchUV_1 = PinchUV(PixelXYUV_1,PinchUV_Size_1);
float2 AnimatedOffsetUV_1 = AnimatedOffsetUV(PinchUV_1,AnimatedOffsetUV_X_1,AnimatedOffsetUV_Y_1,AnimatedOffsetUV_ZoomX_1,AnimatedOffsetUV_ZoomY_1,AnimatedOffsetUV_Speed_1);
float2 DistortionUV_1 = DistortionUV(AnimatedOffsetUV_1,DistortionUV_WaveX_1,DistortionUV_WaveY_1,DistortionUV_DistanceX_1,DistortionUV_DistanceY_1,DistortionUV_Speed_1);
float2 RotationUV_1 = RotationUV(DistortionUV_1,RotationUV_Rotation_1,RotationUV_Rotation_PosX_1,RotationUV_Rotation_PosY_1,RotationUV_Rotation_Speed_1);
float4 _ColorGradients_1 = Color_Gradients(float4(0,0,0,1),RotationUV_1,_ColorGradients_Color1_1,_ColorGradients_Color2_1,_ColorGradients_Color3_1,_ColorGradients_Color4_1);
float4 FinalResult = _ColorGradients_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * i.color.a;
FinalResult.rgb *= FinalResult.a;
FinalResult.rgb = round(FinalResult.rgb * ColorLimit) / ColorLimit;
FinalResult.a = saturate(FinalResult.a);
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
