// Sprite cây đung đưa theo gió, giữ đúng pixel: ngọn lệch tối đa _SwayPixels pixel, gốc đứng yên.
// Độ lệch làm tròn theo pixel (1 / _PixelsPerUnit) để ảnh không nhòe. Gió thổi theo đợt và lan theo trục X.
// Cần toạ độ object (pivot Bottom: y = độ cao so với gốc) nên tắt gộp draw call (DisableBatching).
// Dựa trên Sprites/Default của Built-in (UnitySprites.cginc).
Shader "VeQue/CropSway"
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

        _SwayPixels ("Sway (pixels at top)", Float) = 1
        _PixelsPerUnit ("Pixels Per Unit", Float) = 16
        _SwayHeight ("Full sway height (world units)", Float) = 1
        _Speed ("Sway speed (cycles/s)", Float) = 0.8
        _GustSpeed ("Gust speed (cycles/s)", Float) = 0.12
        _GustThreshold ("Gust threshold (-1 always, near 1 rare)", Range(-1, 0.95)) = 0.3
        _PhasePerUnit ("Phase per unit X", Float) = 0.6
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
            "DisableBatching" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SwayVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _SwayPixels;
            float _PixelsPerUnit;
            float _SwayHeight;
            float _Speed;
            float _GustSpeed;
            float _GustThreshold;
            float _PhasePerUnit;

            v2f SwayVert(appdata_t IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                // Gốc sprite (pivot) trong world: lệch pha theo vị trí để gió lan qua cánh đồng.
                float3 origin = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                float phase = origin.x * _PhasePerUnit + origin.y * 0.3;

                // Đợt gió: chỉ lắc khi sóng chậm vượt ngưỡng, lan từ trái sang phải.
                float gust = saturate((sin((_Time.y * _GustSpeed) * 6.2831853 - origin.x * 0.25) - _GustThreshold)
                    / (1.0 - _GustThreshold));
                float wave = sin(_Time.y * _Speed * 6.2831853 + phase) * gust;

                float height = saturate(IN.vertex.y / max(_SwayHeight, 0.001));
                float pixels = round(wave * _SwayPixels * height);
                IN.vertex.x += pixels / _PixelsPerUnit;
                return SpriteVert(IN);
            }
        ENDCG
        }
    }

    Fallback "Sprites/Default"
}
