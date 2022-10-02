Shader "Shadero Customs/UnitShader"
{
	Properties
	{
		[Toggle(IS_YELLOW)] _IsYellow("Is Yellow", Float) = 0
		[PerRendererData] _HasRarity("Has Rarity", Float) = 0
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_NewTex_1("NewTex_1(RGB)", 2D) = "white" { }
		_MaskRGBA_Fade_1("_MaskRGBA_Fade_1", Range(0, 1)) = 0
		_SpriteFade("SpriteFade", Range(0, 1)) = 1.0
		[PerRendererData] _OutlineColor("Outline Color", Color) = (0,0,0,0)
		[PerRendererData] _HairColor("Hair Color", Color) = (0,0,0,0)
		[PerRendererData] _SkinColor("Skin Color", Color) = (0,0,0,0)
		[PerRendererData] _RarityOneColor("RarityOne Color", Color) = (0,0,0,0)
		[PerRendererData] _RarityTwoColor("RarityTwo Color", Color) = (0,0,0,0)
		[PerRendererData] _RarityThreeColor("RarityThree Color", Color) = (0,0,0,0)
		[PerRendererData] _RarityFourColor("RarityFour Color", Color) = (0,0,0,0)
	}

	SubShader
	{

		Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_local _ IS_YELLOW
			#include "UnityCG.cginc"

			struct appdata_t 
			{
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
			sampler2D _NewTex_1;
			float _MaskRGBA_Fade_1;
			float _HasRarity;
			uniform fixed4 _OutlineColor;
			uniform fixed4 _SkinColor;
			uniform fixed4 _HairColor;
			uniform fixed4 _RarityOneColor;
			uniform fixed4 _RarityTwoColor;
			uniform fixed4 _RarityThreeColor;
			uniform fixed4 _RarityFourColor;
			float4 _MainTex_TexelSize;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				return OUT;
			}


			float4 frag(v2f i) : COLOR
			{
				float4 _MainTex_1 = tex2D(_MainTex, i.texcoord);
				float4 NewTex_1 = tex2D(_NewTex_1, i.texcoord);
				float4 MaskRGBA_1 = _MainTex_1;
				MaskRGBA_1.a = lerp(NewTex_1.r * _MainTex_1.a, (1 - NewTex_1.r) * _MainTex_1.a,_MaskRGBA_Fade_1);
				float4 c = MaskRGBA_1;
#if IS_YELLOW
				if (round(c.r * 255) == 130 && round(c.g * 255) == 10 && round(c.b * 255) == 100)
				{
					c.rgb = float3(15.0 / 255.0, 55.0 / 255.0, 155.0 / 255.0);
				}
				else if (round(c.r * 255) == 180 && round(c.g * 255) == 35 && round(c.b * 255) == 110)
				{
					c.rgb = float3(20.0 / 255.0, 105.0 / 255.0, 195.0 / 255.0);
				}
				else if (round(c.r * 255) == 75 && round(c.g * 255) == 20 && round(c.b * 255) == 60)
				{
					c.rgb = float3(15.0 / 255.0, 15.0 / 255.0, 105.0 / 255.0);
				}
#endif
				if (_HasRarity)
				{
					if (round(c.r * 255) == 85 && round(c.g * 255) == 100 && round(c.b * 255) == 125)
					{
						c.rgb = _RarityOneColor;
					}
					else if (round(c.r * 255) == 120 && round(c.g * 255) == 145 && round(c.b * 255) == 165)
					{
						c.rgb = _RarityTwoColor;
					}
					else if (round(c.r * 255) == 165 && round(c.g * 255) == 190 && round(c.b * 255) == 205)
					{
						c.rgb = _RarityThreeColor;
					}
					else if (round(c.r * 255) == 200 && round(c.g * 255) == 225 && round(c.b * 255) == 235)
					{
						c.rgb = _RarityFourColor;
					}
				}
				if (c.r + c.g + c.b == 1)
				{
					c.rgb = _SkinColor;
				}
				if (c.r + c.g + c.b == 2)
				{
					c.rgb = _HairColor;
				}
				c.rgb *= i.color.rgb;
				c.a = c.a * _SpriteFade * i.color.a;
				if (c.a == 0 && _OutlineColor.a > 0)
				{
					float totalAlpha = 0;

					[unroll(16)]
					for (int k = 1; k < 2; k++) {
						fixed4 pixelUp = tex2D(_MainTex, i.texcoord + fixed2(0, k * _MainTex_TexelSize.y));
						fixed4 pixelDown = tex2D(_MainTex, i.texcoord - fixed2(0, k * _MainTex_TexelSize.y));
						fixed4 pixelRight = tex2D(_MainTex, i.texcoord + fixed2(k * _MainTex_TexelSize.x, 0));
						fixed4 pixelLeft = tex2D(_MainTex, i.texcoord - fixed2(k * _MainTex_TexelSize.x, 0));

						totalAlpha += pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;
					}

					if (totalAlpha > 0) {
						c.rgba = _OutlineColor;
					}
				}
				clip(c.a - 0.001);
				return c;
			}

			ENDCG
		}
	}
	Fallback "Sprites/Default"
}
