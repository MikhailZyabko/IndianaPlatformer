Shader "Hidden/PointLight2D"
{
	Properties
	{
		[PerRendererData]
        _CookieText ("", 2D) = "white" {}
		[PerRendererData]
		_LightColor ("", Color) = (1, 1, 1, 1)
		[PerRendererData]
		_Radius ("", Float) = 0
	}
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
        Blend SrcAlpha DstAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 uv       : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
			};

            sampler2D _CookieText;
            float4 _CookieText_ST;
			
            float _Radius;
            fixed4 _LightColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex * _Radius);
                OUT.uv = IN.uv;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				return tex2D(_CookieText, IN.uv) * _LightColor;
			}
		ENDCG
		}
	}
}