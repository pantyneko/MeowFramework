Shader "Panty/TextGPU"
{
	Properties
	{
		[PerRendererData] _MainTex("Tex",2D) = "white"
	}
		SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			struct a2v
			{
				float4 v : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				float4 v : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			v2f vert(a2v i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				v2f o;
				o.uv = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
				o.v = UnityObjectToClipPos(i.v);
				return o;
			}
			fixed4 frag(v2f i) : SV_TARGET
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}