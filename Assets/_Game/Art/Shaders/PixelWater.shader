// Về Quê – Pixel Water (Built-in RP, WebGL-friendly)
// Dùng cho: Tilemap FishingWater (vùng câu); sau này dùng lại cho ao nhà.
// Tile nước gốc giữ nguyên UV (không méo) -> an toàn với Sprite Atlas. Ngày đêm: WorldLight nhân màu đè lên như các lớp khác.
// Không GrabPass, không screen texture, 2 lần sample noise / pixel.
Shader "VeQue/PixelWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite / Tile", 2D) = "white" {}
        _NoiseTex   ("Noise (tileable, Point, Repeat)", 2D) = "gray" {}

        _PPU        ("Pixels Per Unit", Float) = 16
        _NoiseScale ("Noise Scale", Float) = 0.15
        _Speed1     ("Speed lớp 1 (xy)", Vector) = (0.03, 0.02, 0, 0)
        _Speed2     ("Speed lớp 2 (xy)", Vector) = (-0.02, 0.025, 0, 0)
        _AnimFPS    ("Anim FPS (giật kiểu pixel)", Float) = 8

        _ShineColor ("Màu gợn sáng (a = độ phủ)", Color) = (0.85, 0.95, 1, 0.8)
        _ShineThreshold ("Ngưỡng gợn sáng", Range(0,1)) = 0.62
        _DarkColor  ("Nhân màu vùng tối", Color) = (0.75, 0.85, 0.95, 1)
        _DarkThreshold ("Ngưỡng vùng tối", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            float  _PPU;
            float  _NoiseScale;
            float4 _Speed1;
            float4 _Speed2;
            float  _AnimFPS;

            fixed4 _ShineColor;
            float  _ShineThreshold;
            fixed4 _DarkColor;
            float  _DarkThreshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 worldXY  : TEXCOORD1; // float: tránh sai số half trên mobile
                fixed4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos     = UnityObjectToClipPos(v.vertex);
                o.uv      = v.uv;
                o.worldXY = mul(unity_ObjectToWorld, v.vertex).xy;
                o.color   = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;
                if (baseCol.a < 0.01) discard;

                // 1) Snap toạ độ world về tâm "texel" 1/PPU -> gợn nước khớp lưới pixel
                float2 p = (floor(i.worldXY * _PPU) + 0.5) / _PPU;

                // 2) Lượng tử hoá thời gian -> chuyển động giật nhẹ kiểu sprite animation
                float t = floor(_Time.y * _AnimFPS) / _AnimFPS;

                // 3) frac() offset để _Time lớn (tab mở lâu) không làm mất độ chính xác
                float2 off1 = frac(_Speed1.xy * t);
                float2 off2 = frac(_Speed2.xy * t);

                // 4) Hai lớp noise trôi ngược chiều, nhân với nhau (ý tưởng của jess::codes)
                float n1 = tex2D(_NoiseTex, p * _NoiseScale + off1).r;
                float n2 = tex2D(_NoiseTex, p * _NoiseScale * 1.37 + off2).r;
                float n  = n1 * n2 * 2.0; // ~0..1

                // 5) Bậc màu cứng (step), không gradient -> giữ chất pixel
                float shine = step(_ShineThreshold, n);
                float dark  = step(n, _DarkThreshold);

                fixed3 col = baseCol.rgb;
                // Tối = nhân màu gốc, sáng = phủ màu gợn -> tự hợp với màu tile nước bất kỳ.
                col = lerp(col, col * _DarkColor.rgb, dark);
                col = lerp(col, _ShineColor.rgb, shine * _ShineColor.a);

                return fixed4(col, baseCol.a);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
