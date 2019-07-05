Shader "Custom/tile"
{
	Properties{
	 _MainTex("Texture", 2D) = "white" {}
     _Color ("Color", Color) = (1,1,1,1)
	}

		SubShader{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			Cull off
			Lighting Off
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
				fixed4 _Color;

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
					colour.r = colour.r * _Color.r;
					colour.g = colour.g * _Color.g;
					colour.b = colour.b * _Color.b;

					return colour;
				}

			ENDCG
			}
	}
		FallBack "Diffuse"
}