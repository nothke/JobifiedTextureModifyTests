Shader "Unlit/Mandelbrot"
{
	Properties
	{
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;// TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			int Compute(float2 c, int threshold)
			{
				int iter = 0;
				float r = 0, i = 0, rsqr = 0, isqr = 0;

				const float MAX_MAG_SQUARED = 10;

				while ((iter < threshold) && (rsqr + isqr < MAX_MAG_SQUARED))
				{
					rsqr = r * r;
					isqr = i * i;
					i = 2 * i * r + c.y;
					r = rsqr - isqr + c.x;
					iter++;
				}

				return iter;
			}

			float Band(float center, float width, float t)
			{
				return saturate(1 - abs((-center + t) / width));
			}

			float4 _PositionBounds;

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				float2 position = _PositionBounds.xy;
				float2 bounds = _PositionBounds.zw;
				const int threshold = 100;

				float2 coord = position + bounds * i.uv;
				int p = Compute(coord, threshold);
				float v = saturate(p * 1.0 / threshold);

				fixed r = Band(0.33f, 0.33f, v) + Band(1, 0.33f, v);
				fixed g = Band(0.5f, 0.33f, v) + Band(1, 0.33f, v);
				fixed b = Band(0.66f, 0.33f, v) + Band(1, 0.33f, v);

				fixed4 col = fixed4(r, g, b, 1);

				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
