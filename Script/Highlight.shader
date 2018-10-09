// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Outline"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OutLineSpreadX("Outline Spread", Range(0,0.012)) = 0.007
		_OutLineSpreadY("Outline Spread", Range(0,0.012)) = 0.007
		_Color("Outline Color", Color) = (1.0,1.0,1.0,1.0)
	}

	SubShader
	{


		Tags{ "RenderType" = "Opaque" }
		Lighting Off

		CGPROGRAM

#pragma surface surf Lambert alpha
//#pragma surface surf NoLighting

		/*fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}
*/
		struct Input
		{
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};

		struct v2f 
		{
			float4 pos : POSITION;
			float4 color : COLOR;
		};

		sampler2D _MainTex;
		float _OutLineSpreadX;
		float _OutLineSpreadY;
		float4 _Color;


		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 mainColor = (tex2D(_MainTex, IN.uv_MainTex + float2(_OutLineSpreadX,_OutLineSpreadY)) + tex2D(_MainTex, IN.uv_MainTex - float2(_OutLineSpreadX,_OutLineSpreadY))) * fixed4(0,0,0,1)/* _Color.rgba*/;
			fixed4 addcolor = tex2D(_MainTex, IN.uv_MainTex) * IN.color;

			if (addcolor.a > 0.95) 
			{
				mainColor = addcolor;
			}

			o.Albedo =  mainColor.rgb;
			o.Alpha = mainColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
