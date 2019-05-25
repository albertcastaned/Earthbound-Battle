Shader "Custom/tile"
{
	Properties{
	 _MainTex("Color (RGB) Alpha (A)", 2D) = "white" {}

	}

		SubShader{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			Cull off
			Blend One OneMinusSrcAlpha
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers gles
				#include "UnityCG.cginc"
				#pragma target 3.0

				sampler2D _MainTex;


				 uniform float _LineWidth;
				 uniform float _Hardness;
				 uniform float _DistortionType;

				struct v2f {
					half4 pos : SV_POSITION;
					half2 uv : TEXCOORD0;
					float4 scr_pos : TEXCOORD1;
				};

				fixed4 _MainTex_ST;

				v2f vert(appdata_base v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.scr_pos = ComputeScreenPos(o.pos);
					return o;
				}


				half4 frag(v2f i) : COLOR {

					half2 uv = i.uv;

					float4 colour = tex2D(_MainTex, uv);
					return colour;
				}

			ENDCG
			}
	}
		FallBack "Diffuse"
}