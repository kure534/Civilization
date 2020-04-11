Shader "Unlit/Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MainColor("Color", Color) = (0,0,0,0)
		_MarkingColor("MarkColor", Color) = (1,0,0,0)
		_Speed("Speed", float) = 1
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainColor;
			float4 _MarkingColor;
			float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _MainColor;
				float4 mark = _MarkingColor * (sin(_Time.y * _Speed) + 1) / 2;
				col.r += mark.r;
				col.g += mark.g;
				col.b += mark.b;
				
                return col;
            }
            ENDCG
        }
    }
}
