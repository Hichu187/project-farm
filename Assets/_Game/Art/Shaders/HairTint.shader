// Phủ màu tóc nhân vật Sunnyside: lấy độ sáng của tóc, chuẩn hóa theo từng kiểu tóc (_MinLum.._MaxLum),
// rồi tô theo _HairColor (tối → màu chọn → sáng nhẹ). Pixel tối hơn _OutlineLum (viền) giữ nguyên.
// _Amount = 0 giữ màu gốc. Tính trong không gian gamma để khớp số đo trên ảnh gốc (project dùng Linear).
// Dùng được cho SpriteRenderer (thông số qua MaterialPropertyBlock) và UI Image (material riêng).
// Dựa trên Sprites/Default của Built-in (UnitySprites.cginc).
Shader "VeQue/HairTint"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        _HairColor ("Hair color", Color) = (1,1,1,1)
        _Amount ("Amount (0 = original)", Range(0, 1)) = 0
        _MinLum ("Darkest hair luminance", Range(0, 1)) = 0.15
        _MaxLum ("Brightest hair luminance", Range(0, 1)) = 0.5
        _OutlineLum ("Outline luminance (kept)", Range(0, 1)) = 0.12
        _Shadow ("Shadow strength", Range(0, 1)) = 0.5
        _Highlight ("Highlight strength", Range(0, 1)) = 0.25
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment HairFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _HairColor;
            float _Amount;
            float _MinLum;
            float _MaxLum;
            float _OutlineLum;
            float _Shadow;
            float _Highlight;

            fixed4 HairFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord);
                float3 rgb = c.rgb;
                float3 hair = _HairColor.rgb;
            #ifndef UNITY_COLORSPACE_GAMMA
                rgb = LinearToGammaSpace(rgb);
                hair = LinearToGammaSpace(hair);
            #endif

                float lum = dot(rgb, float3(0.299, 0.587, 0.114));
                float t = saturate((lum - _MinLum) / max(_MaxLum - _MinLum, 0.001));
                float3 shade = hair * lerp(1.0 - _Shadow, 1.0, t);
                shade = lerp(shade, float3(1, 1, 1), _Highlight * t * t);

                float keep = step(lum, _OutlineLum);
                float3 tinted = lerp(shade, rgb, keep);
                rgb = lerp(rgb, tinted, _Amount);

            #ifndef UNITY_COLORSPACE_GAMMA
                rgb = GammaToLinearSpace(rgb);
            #endif
                c.rgb = rgb;
                c *= IN.color;
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }

    Fallback "Sprites/Default"
}
