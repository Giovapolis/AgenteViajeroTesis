Shader "Gio/Fade"{
	Properties{
		_MainTex("Base (RGB)",2D) = "white" {}
		_fade("Fade",Range(0,1)) = 1
	}
	SubShader{
		Pass{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform float _fade;

				float4 frag(v2f_img i) : COLOR{
					return tex2D(_MainTex, i.uv) * _fade;
				}
			ENDCG
		}//Pass
	}//SubShader
}//Shader