// Về Quê – Nước pixel (Built-in RP, WebGL 2).
// Port từ "2D Pixel Art Water" của jess::codes (MIT, github.com/jess-hammer/2d-pixel-water-shader), viết lại bằng CG:
//   1. Màu nước + bậc độ sâu: xa bờ tối dần theo bậc (bản đồ khoảng cách tới bờ _ShoreTex, bake từ Tilemap).
//   2. Caustic: lưới vân sáng (_CausticTex.R) + điểm sáng (.G), lấy mẫu theo pixel thế giới, bị noise làm méo -> uốn lượn.
//   3. Mờ ngẫu nhiên: noise rất thưa (_NoiseTex.G) làm caustic hiện / tắt theo từng mảng lớn.
//   4. Lấp lánh: 2 lớp noise (_NoiseTex.B) trôi ngược chiều, trộn overlay, ngưỡng -> chấm trắng, chỉ nằm trên vân caustic.
//   5. Bọt bờ: sóng sin theo khoảng cách tới bờ chạy dần vào bờ, lượng tử hóa vài bậc; viền bờ 1 px.
//   6. Sóng phao: tối đa 2 vòng elip pixel tỏa ra từ phao, mờ dần (biến global từ BobberRipple.cs; độ mạnh 0 = bỏ qua).
// Mọi phép tính lấy theo tâm pixel thế giới (PPU 16) -> khớp lưới pixel ở mọi bậc zoom.
// 9 lần đọc texture / pixel nước, không GrabPass, không render texture. Ngày đêm: WorldLight nhân màu đè lên như lớp khác.
Shader "VeQue/PixelWater"
{
    Properties
    {
        [PerRendererData] _MainTex ("Tile (chỉ dùng alpha, hoặc màu nếu bật Use Tile Color)", 2D) = "white" {}
        [Header(Textures)]
        _CausticTex ("Caustic (R lưới, G điểm sáng) – Point, Repeat", 2D) = "black" {}
        _NoiseTex   ("Noise (R méo, G mờ, B lấp lánh) – Bilinear, Repeat", 2D) = "gray" {}
        _ShoreTex   ("Shore distance (bake)", 2D) = "white" {}
        _ShoreRect  ("Shore rect (xy gốc, zw kích thước, đơn vị thế giới)", Vector) = (0, 0, 1, 1)
        _ShoreMaxDist ("Shore max dist (ô, khớp lúc bake)", Float) = 5
        _PPU ("Pixels Per Unit", Float) = 16

        [Header(Colour and depth)]
        [Toggle] _UseTileColor ("Dùng màu tile thay Water Color", Float) = 0
        _WaterColor ("Water Color", Color) = (0.306, 0.647, 0.784, 1)
        _DeepTint   ("Deep tint (nhân ở chỗ sâu nhất)", Color) = (0.72, 0.80, 0.90, 1)
        _DepthRange ("Depth range (ô)", Float) = 4.5
        _DepthSteps ("Depth steps", Float) = 3

        [Header(Caustic)]
        _CausticScale   ("Caustic scale (texel / pixel)", Float) = 0.8
        _CausticColor   ("Caustic color (a = độ phủ)", Color) = (0.62, 0.90, 0.95, 0.55)
        _HighlightColor ("Highlight color (a = độ phủ)", Color) = (1, 1, 1, 0.8)
        _MoveScale    ("Méo: scale", Float) = 0.012
        _MoveSpeed    ("Méo: speed", Float) = 0.05
        _MoveStrength ("Méo: độ lệch (texel)", Float) = 2
        _FadeScale ("Mờ mảng: scale", Float) = 0.004
        _FadeSpeed ("Mờ mảng: speed", Float) = 0.01
        _FadeLow   ("Mờ mảng: ngưỡng dưới", Range(0, 0.9)) = 0.12

        [Header(Sparkle)]
        _SpecScale     ("Lấp lánh: scale", Float) = 0.006
        _SpecSpeed     ("Lấp lánh: speed", Float) = 0.02
        _SpecThreshold ("Lấp lánh: ngưỡng", Range(0, 1)) = 0.16
        _SpecColor2    ("Lấp lánh: màu (a = độ phủ)", Color) = (1, 1, 1, 0.85)

        [Header(Foam)]
        _FoamColor ("Foam color (a = độ phủ)", Color) = (1, 1, 1, 0.9)
        _FoamRange ("Foam range (ô)", Float) = 1.4
        _FoamPeak  ("Foam peak", Range(0, 1)) = 0.55
        _FoamFreq  ("Foam frequency", Float) = 30
        _FoamSpeed ("Foam speed", Float) = 3
        _FoamNoiseScale ("Foam noise scale", Float) = 0.012
        _FoamNoiseAmount ("Foam noise amount", Float) = 9
        _FoamQuantize ("Foam bậc", Float) = 3.5
        _ShoreLinePx ("Viền bờ (pixel)", Float) = 1

        // Tâm / tuổi vòng sóng là biến global (_BobberRipplePos, _BobberRippleRings), KHÔNG khai báo ở đây
        // (khai báo thì giá trị material đè lên global).
        [Header(Bobber ripple)]
        _RippleColor ("Sóng phao: màu (a = độ phủ)", Color) = (1, 1, 1, 0.75)
        _RippleSpeed ("Sóng phao: tốc độ tỏa (pixel/giây)", Float) = 9
        _RippleStartRadius ("Sóng phao: bán kính lúc đầu (pixel)", Float) = 2
        _RippleMaxRadius ("Sóng phao: bán kính tối đa (pixel ngang)", Float) = 11
        _RippleSquash ("Sóng phao: dẹt (ngang : dọc)", Float) = 2
        _RippleWidth ("Sóng phao: độ dày vòng", Float) = 2
        _RippleSteps ("Sóng phao: bậc mờ", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CausticTex; float4 _CausticTex_TexelSize;
            sampler2D _NoiseTex;
            sampler2D _ShoreTex;
            float4 _ShoreRect;
            float _ShoreMaxDist;
            float _PPU;

            float _UseTileColor;
            fixed4 _WaterColor;
            fixed4 _DeepTint;
            float _DepthRange;
            float _DepthSteps;

            float _CausticScale;
            fixed4 _CausticColor;
            fixed4 _HighlightColor;
            float _MoveScale, _MoveSpeed, _MoveStrength;
            float _FadeScale, _FadeSpeed, _FadeLow;

            float _SpecScale, _SpecSpeed, _SpecThreshold;
            fixed4 _SpecColor2;

            fixed4 _FoamColor;
            float _FoamRange, _FoamPeak, _FoamFreq, _FoamSpeed;
            float _FoamNoiseScale, _FoamNoiseAmount, _FoamQuantize, _ShoreLinePx;

            float4 _BobberRipplePos;   // global: xy tâm vòng 1, zw tâm vòng 2 (thế giới)
            float4 _BobberRippleRings; // global: x tuổi 1, y độ mạnh 1, z tuổi 2, w độ mạnh 2
            fixed4 _RippleColor;
            float _RippleSpeed, _RippleStartRadius, _RippleMaxRadius, _RippleSquash, _RippleWidth, _RippleSteps;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 world : TEXCOORD1; // float: half trên GPU điện thoại làm rung pixel ở x = -60
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                o.color = v.color;
                return o;
            }

            fixed3 over(fixed3 dst, fixed3 src, float a) { return lerp(dst, src, saturate(a)); }

            // Một vòng sóng: elip dẹt quanh pixel tâm, bán kính tăng theo tuổi, mờ dần theo bậc.
            float ripple(float2 px, float2 center, float age, float strength)
            {
                float2 d = px - floor(center * _PPU);
                d.y *= _RippleSquash;
                float radius = _RippleStartRadius + age * _RippleSpeed;
                float life = saturate(1.0 - radius / _RippleMaxRadius);
                float band = step(abs(length(d) - radius), _RippleWidth * 0.5);
                float a = saturate(strength) * life * band;
                return ceil(a * _RippleSteps) / _RippleSteps;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tile = tex2D(_MainTex, i.uv) * i.color;
                if (tile.a < 0.01) discard;

                float t = _Time.y;
                float2 px = floor(i.world * _PPU);              // pixel thế giới (số nguyên)
                float2 snapped = (px + 0.5) / _PPU;             // tâm pixel, đơn vị thế giới

                // --- Khoảng cách tới bờ (ô) ---
                float2 shoreUV = (snapped - _ShoreRect.xy) / _ShoreRect.zw;
                float dist = tex2D(_ShoreTex, shoreUV).r * _ShoreMaxDist;

                // --- 1. Màu + bậc độ sâu ---
                fixed3 col = lerp(_WaterColor.rgb, tile.rgb, _UseTileColor);
                float depth = saturate(dist / _DepthRange);
                depth = floor(depth * _DepthSteps) / _DepthSteps;
                col *= lerp(fixed3(1, 1, 1), _DeepTint.rgb, depth);

                // --- 2. Caustic uốn lượn ---
                float2 moveUV = px * _MoveScale + frac(float2(1.0, 0.7) * _MoveSpeed * t);
                float mx = tex2D(_NoiseTex, moveUV).r;
                float my = tex2D(_NoiseTex, moveUV + float2(0.37, 0.61)).r;
                float2 causticUV = (px * _CausticScale + (float2(mx, my) - 0.5) * 2.0 * _MoveStrength) * _CausticTex_TexelSize.xy;
                fixed2 caustic = tex2D(_CausticTex, causticUV).rg;

                // --- 3. Mờ theo mảng ---
                float2 fadeUV = px * _FadeScale + frac(float2(1.0, -0.5) * _FadeSpeed * t);
                float fade = saturate((tex2D(_NoiseTex, fadeUV).g - _FadeLow) / (1.0 - _FadeLow));

                col = over(col, _CausticColor.rgb, caustic.r * _CausticColor.a * fade);
                col = over(col, _HighlightColor.rgb, caustic.g * _HighlightColor.a * fade);

                // --- 4. Lấp lánh (chỉ trên vân caustic) ---
                float specT = frac(_SpecSpeed * t);
                float s1 = tex2D(_NoiseTex, px * _SpecScale + float2(-specT, 0)).b;
                float s2 = tex2D(_NoiseTex, px * _SpecScale + float2(specT + 0.5, 0.3)).b;
                float ov = s1 < 0.5 ? 2.0 * s1 * s2 : 1.0 - 2.0 * (1.0 - s1) * (1.0 - s2);
                float spec = step(ov, _SpecThreshold) * step(0.5, caustic.r) * step(0.05, fade);
                col = over(col, _SpecColor2.rgb, spec * _SpecColor2.a);

                // --- 5. Bọt bờ ---
                float r = saturate(1.0 - dist / _FoamRange) * _FoamPeak;  // cao ở sát bờ
                float foamNoise = tex2D(_NoiseTex, px * _FoamNoiseScale).g;
                float phase = frac(_FoamSpeed * t / 6.2831853) * 6.2831853; // giữ nhỏ cho float trên mobile
                float wave = 0.1 + 0.3 * (sin(phase + foamNoise * _FoamNoiseAmount - r * _FoamFreq) * 0.5 + 0.5);
                float foam = clamp(r - wave, 0.0, 0.4);
                foam = ceil(foam * _FoamQuantize) / _FoamQuantize;
                float foamA = saturate(foam * _FoamColor.a);
                foamA = dist * _PPU < _ShoreLinePx ? _FoamColor.a : foamA;
                col = over(col, _FoamColor.rgb, foamA);

                // --- 6. Sóng phao (bỏ qua khi không câu) ---
                if (_BobberRippleRings.y + _BobberRippleRings.w > 0.0)
                {
                    float rp = max(ripple(px, _BobberRipplePos.xy, _BobberRippleRings.x, _BobberRippleRings.y),
                                   ripple(px, _BobberRipplePos.zw, _BobberRippleRings.z, _BobberRippleRings.w));
                    col = over(col, _RippleColor.rgb, rp * _RippleColor.a);
                }

                return fixed4(col, tile.a);
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
