// Ngày đêm (Đợt 2 Phần 13): một quad phủ khung nhìn camera, hòa màu kiểu NHÂN (Blend DstColor Zero) lên mọi thứ vẽ trước nó
// (sortingOrder < quad): đất, cây, nhân vật, nước. Icon / dấu phía trên (sortingOrder cao hơn) và UI không bị ảnh hưởng.
// Hệ số nhân = màu trời × (1 − độ tối × (1 − ánh sáng)); trong vòng sáng còn ấm lên theo màu đèn.
//   - Đêm ở nông trại: độ tối 0, màu trời tối nhẹ (≥ 70% độ sáng), đèn dầu / đom đóm là vòng sáng.
//   - Soi đèn (Phần 16): độ tối ~0,9 + vòng sáng đèn pin.
// Vòng sáng tính theo tọa độ thế giới làm tròn về lưới pixel (PPU 16) và chia 4 bậc → rìa có bậc như pixel art.
// Nhân màu cho kết quả như nhau ở Linear và Gamma ((a·b)^γ = a^γ·b^γ), nên không cần đổi không gian màu.
Shader "VeQue/WorldLight"
{
    Properties
    {
        _Tint ("Sky tint (multiply)", Color) = (1,1,1,1)
        _Darkness ("Darkness", Range(0, 1)) = 0
        _LightCount ("Light count", Float) = 0
        _PixelsPerUnit ("Pixels per unit", Float) = 16
        _LightSteps ("Light steps", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend DstColor Zero

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            #define MAX_LIGHTS 8

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 world : TEXCOORD0;
            };

            fixed4 _Tint;
            float _Darkness;
            float _LightCount;
            float _PixelsPerUnit;
            float _LightSteps;
            // xy = tâm (thế giới), z = bán kính (đơn vị thế giới), w = độ mềm rìa (đơn vị thế giới).
            float4 _Lights[MAX_LIGHTS];
            // rgb = màu đèn, a = cường độ (0–1).
            float4 _LightColors[MAX_LIGHTS];

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Tâm pixel thế giới.
                float2 p = (floor(IN.world * _PixelsPerUnit) + 0.5) / _PixelsPerUnit;
                float light = 0;
                float3 warm = float3(0, 0, 0);
                for (int i = 0; i < MAX_LIGHTS; i++)
                {
                    if (i < _LightCount)
                    {
                        float4 l = _Lights[i];
                        float d = distance(p, l.xy);
                        float a = saturate((l.z - d) / max(l.w, 0.0001)) * _LightColors[i].a;
                        a = ceil(a * _LightSteps) / _LightSteps;
                        light = max(light, a);
                        warm = max(warm, _LightColors[i].rgb * a);
                    }
                }
                float3 m = _Tint.rgb * (1 - _Darkness * (1 - light));
                // Trong vòng sáng: kéo về màu đèn (không vượt 1 vì là phép nhân).
                m = lerp(m, max(m, warm), light);
                return fixed4(m, 1);
            }
        ENDCG
        }
    }
}
