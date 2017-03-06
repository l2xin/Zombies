Shader "Hidden/ProGrids/PerspectiveGrid"
{
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Lighting Off
		ZWrite On
		ZTest LEqual
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			AlphaTest Off

			CGPROGRAM

			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.color = v.color;
				o.color = float4(1.0,1.0,1.0,1.0);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				//return i.color;
				return float4(1.0,1.0,1.0,1.0);
			}
			
			ENDCG	
		}
	} 
	FallBack "Diffuse"
}
