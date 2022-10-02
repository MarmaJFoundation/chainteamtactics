// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/UIUnitShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        [PerRendererData] _HairColor("Hair Color", Color) = (0,0,0,0)
        [PerRendererData] _SkinColor("Skin Color", Color) = (0,0,0,0)
        [PerRendererData] _RarityOneColor("RarityOne Color", Color) = (0,0,0,0)
        [PerRendererData] _RarityTwoColor("RarityTwo Color", Color) = (0,0,0,0)
        [PerRendererData] _RarityThreeColor("RarityThree Color", Color) = (0,0,0,0)
        [PerRendererData] _RarityFourColor("RarityFour Color", Color) = (0,0,0,0)
        _ColorMask("Color Mask", Float) = 15

        _UseOutline("Use Outline", Float) = 0
        [Toggle(IS_YELLOW)] _IsYellow("Is Yellow", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ IS_YELLOW

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                fixed4 _Color;
                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
                float4 _MainTex_ST;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                    OUT.color = v.color * _Color;
                    return OUT;
                }

                float4 _MainTex_TexelSize;
                float _Brightness_1;
                float _UseOutline;
                uniform fixed4 _OutlineColor;
                uniform fixed4 _SkinColor;
                uniform fixed4 _HairColor;
                uniform fixed4 _RarityOneColor;
                uniform fixed4 _RarityTwoColor;
                uniform fixed4 _RarityThreeColor;
                uniform fixed4 _RarityFourColor;
                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 c = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
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
                    if (c.r + c.g + c.b == 1)
                    {
                        c.rgb = _SkinColor;
                    }
                    if (c.r + c.g + c.b == 2)
                    {
                        c.rgb = _HairColor;
                    }
                    c.rgb *= IN.color.rgb;
                    if (_UseOutline == 1)
                    {
                        if (c.a == 0)
                        {
                            float totalAlpha = 0;

                            [unroll(16)]
                            for (int i = 1; i < 2; i++) {
                                fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, i * _MainTex_TexelSize.y));
                                fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0, i * _MainTex_TexelSize.y));
                                fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(i * _MainTex_TexelSize.x, 0));
                                fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(i * _MainTex_TexelSize.x, 0));

                                totalAlpha += pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;
                            }

                            if (totalAlpha > 0) {
                                c.rgba = _OutlineColor;
                            }
                        }
                    }
                    #ifdef UNITY_UI_CLIP_RECT
                    c.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif
                    clip(c.a - 0.001);

                    return c;
                }
            ENDCG
            }
        }
}
