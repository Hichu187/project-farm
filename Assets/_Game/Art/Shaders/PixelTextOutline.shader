// Chữ TMP dạng bitmap (font pixel, atlas Raster lọc Point) có viền pixel.
// Dựa trên TextMeshPro/Mobile/Bitmap. Mỗi điểm lấy thêm 8 điểm xung quanh trong atlas cách _OutlineTexels texel
// (= 1 pixel thiết kế của font); chỗ chữ trống mà có chữ bên cạnh thì tô _OutlineColor → viền vuông, sắc nét.
// _Padding: TMP đọc thuộc tính này (ShaderUtilities.GetPadding) để nới khung từng chữ, viền không bị cắt.
// Atlas cần khoảng cách giữa các chữ > _OutlineTexels + 1 (PixelFontBuilder đặt 4).
Shader "VeQue/TMP Pixel Outline" {

Properties {
	_MainTex		    ("Font Atlas", 2D) = "white" {}
	_Color		        ("Text Color", Color) = (1,1,1,1)
	_DiffusePower	    ("Diffuse Power", Range(1.0,4.0)) = 1.0

	_OutlineColor       ("Outline Color", Color) = (0.094,0.078,0.145,1)
	_OutlineTexels      ("Outline Width (texel atlas)", Float) = 2
	_OutlineCorners     ("Outline Corners (0 = 4 hướng, 1 = 8 hướng)", Range(0,1)) = 1
	_Padding            ("Padding (texel atlas)", Float) = 2

	_VertexOffsetX      ("Vertex OffsetX", float) = 0
	_VertexOffsetY      ("Vertex OffsetY", float) = 0
	_MaskSoftnessX      ("Mask SoftnessX", float) = 0
	_MaskSoftnessY      ("Mask SoftnessY", float) = 0

	_ClipRect           ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)

	_StencilComp        ("Stencil Comparison", Float) = 8
	_Stencil            ("Stencil ID", Float) = 0
	_StencilOp          ("Stencil Operation", Float) = 0
	_StencilWriteMask   ("Stencil Write Mask", Float) = 255
	_StencilReadMask    ("Stencil Read Mask", Float) = 255

	_CullMode           ("Cull Mode", Float) = 0
	_ColorMask          ("Color Mask", Float) = 15
}

SubShader {

	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

	Stencil
	{
		Ref[_Stencil]
		Comp[_StencilComp]
		Pass[_StencilOp]
		ReadMask[_StencilReadMask]
		WriteMask[_StencilWriteMask]
	}

	Lighting Off
	Cull [_CullMode]
	ZTest [unity_GUIZTestMode]
	ZWrite Off
	Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask[_ColorMask]

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#pragma multi_compile __ UNITY_UI_ALPHACLIP

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"

		struct appdata_t
		{
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord0 : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
		};

		struct v2f
		{
			float4 vertex		: POSITION;
			fixed4 color		: COLOR;
			float2 texcoord0	: TEXCOORD0;
			float4 mask			: TEXCOORD2;
		};

		sampler2D 	_MainTex;
		float4		_MainTex_TexelSize;
		fixed4		_Color;
		float		_DiffusePower;
		fixed4		_OutlineColor;
		float		_OutlineTexels;
		float		_OutlineCorners;

		uniform float		_VertexOffsetX;
		uniform float		_VertexOffsetY;
		uniform float4		_ClipRect;
		uniform float		_MaskSoftnessX;
		uniform float		_MaskSoftnessY;
		uniform float		_UIMaskSoftnessX;
		uniform float		_UIMaskSoftnessY;
		uniform int _UIVertexColorAlwaysGammaSpace;

		v2f vert (appdata_t v)
		{
			v2f OUT;
			float4 vert = v.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;

			vert.xy += (vert.w * 0.5) / _ScreenParams.xy;
			if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
			{
				v.color.rgb = UIGammaToLinear(v.color.rgb);
			}
			OUT.vertex = UnityPixelSnap(UnityObjectToClipPos(vert));
			OUT.color = v.color;
			OUT.color *= _Color;
			OUT.color.rgb *= _DiffusePower;
			OUT.texcoord0 = v.texcoord0;

			float2 pixelSize = OUT.vertex.w;

			const float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
			const half2 maskSoftness = half2(max(_UIMaskSoftnessX, _MaskSoftnessX), max(_UIMaskSoftnessY, _MaskSoftnessY));
			OUT.mask = float4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * maskSoftness + pixelSize.xy));

			return OUT;
		}

		fixed4 frag (v2f IN) : COLOR
		{
			float2 uv = IN.texcoord0;
			float2 d = _MainTex_TexelSize.xy * _OutlineTexels;

			fixed face = tex2D(_MainTex, uv).a;

			// 4 hướng thẳng.
			fixed ring = tex2D(_MainTex, uv + float2( d.x, 0)).a;
			ring = max(ring, tex2D(_MainTex, uv + float2(-d.x, 0)).a);
			ring = max(ring, tex2D(_MainTex, uv + float2(0,  d.y)).a);
			ring = max(ring, tex2D(_MainTex, uv + float2(0, -d.y)).a);

			// 4 hướng chéo (tắt bằng _OutlineCorners = 0 cho góc viền bo).
			fixed corners = tex2D(_MainTex, uv + d).a;
			corners = max(corners, tex2D(_MainTex, uv - d).a);
			corners = max(corners, tex2D(_MainTex, uv + float2(d.x, -d.y)).a);
			corners = max(corners, tex2D(_MainTex, uv + float2(-d.x, d.y)).a);
			ring = max(ring, corners * _OutlineCorners);

			fixed outline = ring * _OutlineColor.a;
			fixed4 color;
			color.rgb = lerp(_OutlineColor.rgb, IN.color.rgb, face);
			color.a = max(face, outline) * IN.color.a;

			#if UNITY_UI_CLIP_RECT
				half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
				color *= m.x * m.y;
			#endif

			#if UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
			#endif

			return color;
		}
		ENDCG
	}
}

Fallback "TextMeshPro/Mobile/Bitmap"
}
