Shader "Custom/Unlit/SOS_CustomUnlit"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 textcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 textcoord : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			
			v2f vert (appdata IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.textcoord = TRANSFORM_TEX(IN.textcoord, _MainTex);
				return OUT;
			}
			
			fixed4 frag (v2f IN) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, IN.textcoord);
				UNITY_OPAQUE_ALPHA(col.a);
				return col * _Color;
			}
			ENDCG
		}
	}
}
