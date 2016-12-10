//Am copying from this talk https://youtu.be/3penhrrKCYg?t=36m39s

Shader "Custom/Unlit/SOS_MaskedSprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Colour", Color) = (1,1,1,1)

		_DissolveTexture("Mask (set with SpriteMask script)", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vertexFunction
			#pragma fragment fragmentFunction
			
			#include "UnityCG.cginc"

			//Vertices
			//Normal
			//Colour
			//uv

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _DissolveTexture;

			//Build our object!
			v2f vertexFunction (appdata IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
				return OUT;
			}

			//Colour it in! Hooray!
			fixed4 fragmentFunction (v2f IN) : SV_Target
			{
				// sample the texture
				float4 textureColour = tex2D(_MainTex, IN.uv);
				return textureColour * _Colour;
			}
			ENDCG
		}
	}
}
