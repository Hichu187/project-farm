// Cá đột biến lấp lánh (Đợt 2 Phần 18, thiết kế Docs/thiet-ke-shiny-ca.md mục 2, khớp script shiny_8kieu.py).
// Một shader cho mọi kiểu; mỗi kiểu là một material (bảng màu + cờ dạng float, không keyword riêng).
// Dựa trên UI/Default (giữ stencil, _ClipRect cho RectMask2D); thêm _Flip / _RendererColor để dùng được cho SpriteRenderer.
// Tọa độ pixel nguyên của sprite (gốc trên-trái như ảnh mô phỏng) tính từ khung sprite trong texture:
//   UI: uv1 = (uMin, vMin, uMax, vMax) do ShinyIconMesh ghi; SpriteRenderer: _SpriteRect qua MaterialPropertyBlock (_UseSpriteRect = 1).
// Thời gian: global _ShinyTime (ShinyClock, Time.unscaledTime). 16 khung, mỗi khung 0,095 giây như ảnh GIF.
// Tính màu trong không gian gamma để khớp số trong thiết kế (project dùng Linear).
Shader "VeQue/ShinyIcon"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ToneDark ("Tone dark", Color) = (0.72,0.44,0.10,1)
        _ToneMid ("Tone mid", Color) = (1,0.75,0.18,1)
        _ToneLight ("Tone light", Color) = (1,0.91,0.54,1)
        _Sweep0 ("Sweep 0", Color) = (1,1,1,1)
        _Sweep1 ("Sweep 1", Color) = (1,1,1,1)
        _Sweep2 ("Sweep 2", Color) = (1,1,1,1)
        _Sweep3 ("Sweep 3", Color) = (1,1,1,1)
        _TrailColor ("Trail color", Color) = (0.31,0.34,0.44,1)
        [Toggle] _Trail ("Trail (Bạc)", Float) = 0
        [Toggle] _Aurora ("Aurora", Float) = 0
        [Toggle] _AuroraDark ("Aurora dark (Tinh dạ)", Float) = 0
        [Toggle] _Stars ("Stars", Float) = 0
        _MixK ("Aurora dark mix", Range(0,1)) = 0.7

        [HideInInspector] _SpriteRect ("Sprite rect (UV)", Vector) = (0,0,1,1)
        [HideInInspector] _UseSpriteRect ("Use _SpriteRect", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 rect : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 rect : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            fixed4 _RendererColor;
            float4 _Flip;
            float4 _SpriteRect;
            float _UseSpriteRect;

            fixed4 _ToneDark, _ToneMid, _ToneLight;
            fixed4 _Sweep0, _Sweep1, _Sweep2, _Sweep3;
            fixed4 _TrailColor;
            float _Trail, _Aurora, _AuroraDark, _Stars, _MixK;
            float _ShinyTime;

            static const float3 Navy = float3(24.0, 20.0, 37.0) / 255.0;
            static const float TwoPi = 6.28318530718;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                v.vertex.xy *= _Flip.xy;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color * _RendererColor;
                OUT.rect = lerp(v.rect, _SpriteRect, _UseSpriteRect);
                return OUT;
            }

            float3 ToGamma(float3 c)
            {
            #ifdef UNITY_COLORSPACE_GAMMA
                return c;
            #else
                return LinearToGammaSpace(c);
            #endif
            }

            float3 FromGamma(float3 c)
            {
            #ifdef UNITY_COLORSPACE_GAMMA
                return c;
            #else
                return GammaToLinearSpace(c);
            #endif
            }

            float3 HsvToRgb(float h, float s, float v)
            {
                float3 k = saturate(abs(frac(h + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
                return v * lerp(float3(1, 1, 1), k, s);
            }

            float3 Pick3(float lvl, float3 a, float3 b, float3 c)
            {
                return lvl < 0.5 ? a : (lvl < 1.5 ? b : c);
            }

            // Màu shiny (gamma) của 1 pixel; keep = 1 thì giữ màu gốc.
            float3 Shiny(float3 g, float alpha, float x, float y, out float keep)
            {
                keep = 0;
                if (all(abs(g - Navy) < 2.0 / 255.0))
                {
                    keep = 1;
                    return g;
                }
                float L = dot(g, float3(0.299, 0.587, 0.114));
                if (L < 0.16)
                {
                    keep = 1;
                    return g;
                }
                // Vòng trắng mắt: PNG đánh dấu alpha 254.
                if (abs(alpha * 255.0 - 254.0) < 0.5)
                    return float3(1, 1, 1);

                float lvl = L < 0.42 ? 0 : (L < 0.72 ? 1 : 2);
                float frame = fmod(floor(_ShinyTime / 0.095), 16.0);
                float t = frame / 16.0;

                float3 tone = Pick3(lvl, ToGamma(_ToneDark.rgb), ToGamma(_ToneMid.rgb), ToGamma(_ToneLight.rgb));
                float3 c = tone;
                if (_Aurora > 0.5 || _AuroraDark > 0.5)
                {
                    float w = sin(x * 0.35 + t * TwoPi) * 3.0;
                    float k = (y + w) / 30.0 + t;
                    float hue = 0.45 + 0.5 * (0.5 + 0.5 * sin(k * TwoPi));
                    hue = floor(hue * 10.0 + 0.5) / 10.0;
                    if (_AuroraDark > 0.5)
                    {
                        float3 aurora = HsvToRgb(frac(hue), Pick3(lvl, 0.75, 0.65, 0.5).x, Pick3(lvl, 0.42, 0.55, 0.72).x);
                        c = lerp(tone, aurora, _MixK);
                    }
                    else
                    {
                        c = HsvToRgb(frac(hue), Pick3(lvl, 0.5, 0.38, 0.18).x, Pick3(lvl, 0.78, 0.97, 1.0).x);
                    }
                }

                float d = (x + y) - (t * 96.0 - 16.0);
                if (d >= 0 && d < 4)
                {
                    float i = floor(d);
                    float3 s = i < 0.5 ? _Sweep0.rgb : (i < 1.5 ? _Sweep1.rgb : (i < 2.5 ? _Sweep2.rgb : _Sweep3.rgb));
                    s = ToGamma(s);
                    c = lvl > 0.5 ? s : lerp(s, c, 0.4);
                }
                else if (_Trail > 0.5 && d >= 4 && d < 6 && lvl > 0.5)
                {
                    c = ToGamma(_TrailColor.rgb);
                }

                if (_Stars > 0.5 && lvl < 1.5
                    && fmod(x * 7.0 + y * 13.0, 23.0) < 0.5
                    && fmod(floor((x + y + frame) / 3.0), 2.0) < 0.5)
                {
                    c = float3(1, 1, 1);
                }
                return c;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 tex = tex2D(_MainTex, IN.texcoord);
                float4 color = tex + _TextureSampleAdd;

                if (tex.a > 0.002)
                {
                    float2 size = _MainTex_TexelSize.zw;
                    float2 minPx = IN.rect.xy * size;
                    float heightPx = floor((IN.rect.w - IN.rect.y) * size.y + 0.5);
                    float2 p = floor(IN.texcoord * size - minPx);
                    float x = p.x;
                    float y = heightPx - 1.0 - p.y;

                    float keep;
                    float3 shiny = Shiny(ToGamma(tex.rgb), tex.a, x, y, keep);
                    if (keep < 0.5)
                        color.rgb = FromGamma(shiny);
                    // Pixel trắng mắt (alpha 254) hiện đặc.
                    color.a = tex.a > 0.99 ? 1.0 : tex.a;
                }

                color *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
