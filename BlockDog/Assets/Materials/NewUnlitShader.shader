Shader "Custom/Trails"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LastFrameTex("LastFrameTexture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _LastFrameTex;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			float nrand(float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233)) + _Time.x) * 43758.5453);
			}


			float Epsilon = 1e-10;

			float3 RGBtoHCV(in float3 RGB)
			{
				// Based on work by Sam Hocevar and Emil Persson
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}
			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}

			float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}
			float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv + (float2(nrand(i.uv), nrand(i.uv * .276565456f)) *.003f) - (float2(nrand(i.uv * .765676567f), nrand(i.uv * .0155667654f)) *.003f));//tex2D(_MainTex, i.uv);
				i.uv.x -= .001f;
				fixed4 oldCol = tex2D(_LastFrameTex, i.uv + (float2(nrand(i.uv), nrand(i.uv * .276565456f)) *.0075f) - (float2(nrand(i.uv * .765676567f), nrand(i.uv * .0155667654f)) *.0075f));
				fixed4 realOldCol = tex2D(_LastFrameTex, i.uv);
				fixed3 hsv = RGBtoHSV(oldCol);
				fixed3 hsv2 = RGBtoHSV(realOldCol);
				if (hsv.b > .4f) {
					hsv.b = max(.4f, hsv.b - .2f);
				}
				if (hsv.b < .2f && hsv2.b < .2f) {
					hsv = hsv2;
				}
				if (nrand(i.uv) > .7f) {
					hsv.b -= .002f;
				}
				/*if (hsv.b > .2f) {
					hsv.b -= .002f;
				}*/
				//hsv.b -= .00195885f;
				oldCol = fixed4(HSVtoRGB(hsv), 1);


				float j = RGBtoHSV(col).b;
				float k = RGBtoHSV(oldCol).b;

				if (k > j) {
					//hsv = RGBtoHSV(oldCol);
					//hsv.b *= .25f;
					//oldCol = fi	xed4(HSVtoRGB(hsv), 1);
					col = oldCol;
				}
				return col;
			}
			ENDCG
		}
	}
}
