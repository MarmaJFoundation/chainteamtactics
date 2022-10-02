// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _OutlineColor("OutlineColor", Color) = (0,0,0,1)
        _Brightness_1("_Brightness_1", Range(0, 2)) = 1
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(HAS_OUTLINE)] _UseOutline("Use Outline", Float) = 0
        [Toggle(HAS_GRAYSCALE)] _UseGrayscale("Use Grayscale", Float) = 0
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
                #pragma multi_compile_local _ HAS_OUTLINE
                #pragma multi_compile_local _ HAS_GRAYSCALE

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
                fixed4 _OutlineColor;

                float4 grayscale(float4 txt)
                {
                    float3 gs = dot(txt.rgb, float3(0.3, 0.59, 0.11));
                    return lerp(txt, float4(gs, txt.a), 1);
                }
                float4 ColorHSV(float4 RGBA, float HueShift, float Sat, float Val)
                {

                    float4 RESULT = float4(RGBA);
                    float a1 = Val * Sat;
                    float a2 = HueShift * 3.14159265 / 180;
                    float VSU = a1 * cos(a2);
                    float VSW = a1 * sin(a2);

                    RESULT.x = (.299 * Val + .701 * VSU + .168 * VSW) * RGBA.x
                        + (.587 * Val - .587 * VSU + .330 * VSW) * RGBA.y
                        + (.114 * Val - .114 * VSU - .497 * VSW) * RGBA.z;

                    RESULT.y = (.299 * Val - .299 * VSU - .328 * VSW) * RGBA.x
                        + (.587 * Val + .413 * VSU + .035 * VSW) * RGBA.y
                        + (.114 * Val - .114 * VSU + .292 * VSW) * RGBA.z;

                    RESULT.z = (.299 * Val - .3 * VSU + 1.25 * VSW) * RGBA.x
                        + (.587 * Val - .588 * VSU - 1.05 * VSW) * RGBA.y
                        + (.114 * Val + .886 * VSU - .203 * VSW) * RGBA.z;

                    return RESULT;
                }
                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                    color = ColorHSV(color, 1, 1, _Brightness_1);
                    #ifdef HAS_GRAYSCALE
                    color = grayscale(color);
                    #endif
                    #ifdef HAS_OUTLINE
                    if (color.a == 0)
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
                            color.rgba = _OutlineColor;
                        }
                    }
                    #endif
                    #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif
                    clip(color.a - 0.001);

                    return color;
                }
            ENDCG
            }
        }
}
