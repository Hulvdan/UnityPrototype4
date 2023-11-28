Shader "Unlit/TileGrid"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Width ("Width", float) = .05
        _Strength ("Strength", float) = .2
    }
    SubShader {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Width;
            float _Strength;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a == 0)
                    discard;

                // fixed3 a = i.vertex * UNITY_MATRIX_VP;
                // fixed3 a = mul(i.vertex, vp);
                // fixed3 a = UnityWorldToObjectDir(i.vertex);

                // fixed4 col_alt = fixed4(col.r, col.g, col.b, 1);
                // fixed4 col_alt = fixed4(i.uv.z, i.uv.a, 0, 1);
                fixed4 pos = i.worldPos % 1;
                if (pos.x < 0) pos.x++;
                if (pos.y < 0) pos.y++;
                fixed4 col_alt = fixed4(col.x, col.y, 0, 1);

                if (
                    pos.x < _Width
                    || pos.x >= 1 - _Width
                    || pos.y < _Width
                    || pos.y >= 1 - _Width
                ) {
                    col_alt *= 1 - _Strength;
                }

                UNITY_APPLY_FOG(i.fogCoord, colll);
                return col_alt;
            }
            ENDCG



        }
    }
}
