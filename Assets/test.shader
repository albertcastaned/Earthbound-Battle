// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/test"
{
    Properties {
 _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"
         colorMap ("Base (RGB)", 2D) = "white" {}
        _Amplitude("Amplitude",Float) = 1.0
        _Frequency("Frequency",Float) = 1.0
        _Scale("Scale",Float) = 1.0
        _Alpha("Alpha", Float) = 1
        _LineWidth("Line Width", Float) = 4
        _Hardness("Hardness", Float) = 0.9
        _DistortionType("Distortion Type", Float) = 1
        in_PatternRand("Pattern rand", Float) = 1
    }
 
    SubShader {
         Tags { "Queue"="Transparent" "RenderType"="Transparent" }
         Blend SrcAlpha OneMinusSrcAlpha
 
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma exclude_renderers gles
            #include "UnityCG.cginc"
            #pragma target 3.0
                
            sampler2D _MainTex;
            sampler2D colorMap;
            
 
  
             float _Amplitude;
             float _Frequency;
             float _Scale;
             float _Alpha;
             float in_PatternRand;
             
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
            
            float3 rgb2hsv(float3 c) {
              float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
              float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
              float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

              float d = q.x - min(q.w, q.y);
              float e = 1.0e-10;
              return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 hsv2rgb(float3 c) {
              c = float3(c.x, clamp(c.yz, 0.0, 1.0));
              float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
              float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
              return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            half4 frag(v2f i) : COLOR {
                float M_PI = 3.1415926535897932384626433832795;

                half2 uv = i.uv;
                float offset;
                float lineSize = _ScreenParams.y*0.004;
                float ps = (i.scr_pos.y * _ScreenParams.y / i.scr_pos.w);
                
                switch(_DistortionType)
                {
                case 1:
                offset = _Amplitude * sin((_Frequency * uv.y) + (_Time.y * _Scale));

                if(((int)(ps / floor(_LineWidth*lineSize)) % 2 == 0))
                {
                uv.x = uv.x + offset;
                }else{
                uv.x = uv.x - offset;
                }
                break;
                
                case 2:
                offset = _Amplitude * sin((_Frequency * uv.y) + (_Time.y * _Scale));

                
                
                uv.x = uv.x - offset;
                break;
                
                case 3:
                offset = _Amplitude * sin((_Frequency * uv.x) + (_Time.y * _Scale));
                uv.y = uv.y - offset;
                break;

                               
                case 4:
                offset = _Amplitude * sin((_Frequency * uv.x) + (_Time.y * _Scale));
                if(((int)(ps / floor(_LineWidth*lineSize)) % 2 == 0))
                {
                uv.y = uv.y + offset;
                }else{
                uv.y = uv.y - offset;
                }
                break;
                
                case 5:
                offset = _Amplitude * sin((_Frequency * uv.y) + (_Time.y * _Scale));
                if(((int)(ps / floor(_LineWidth*lineSize)) % 2 == 0))
                {
                uv.y = uv.y + offset;
                }else{
                uv.y = uv.y - offset;
                }
                break;

                }

                
                
                

                float4 colour = tex2D(_MainTex, uv);
                float aux = 32 + sin(_Time.z)*16;

    
    
                   // Random hue shift
                 float3 hsv = rgb2hsv(colour.rgb);
                //color.r =  color.r + sin(color.r + _Time.z) /16;
              //  color.b =  color.b + sin(color.b + _Time.z) /32;
              //  color.g =  color.g + sin(color.g + _Time.z) /32;
                    hsv.x = fmod(hsv.x + in_PatternRand + sin(in_PatternRand + uv.x*M_PI*2.0)/aux + sin(in_PatternRand +uv.y*M_PI*2.0)/aux, 1.0);
                    colour.rgb = hsv2rgb(hsv);

                
                

                colour.a =_Alpha;

            
                return colour;
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}