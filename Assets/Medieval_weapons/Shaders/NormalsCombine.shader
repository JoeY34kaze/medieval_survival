Shader "Custom/NormalsCombine" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_BumpMapDetail ("Normalmap Detail", 2D) = "bump" {}
	_Detail ("Detail", Range (0, 1)) = 0.5
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _BumpMapDetail;
sampler2D _SpecMap;
fixed4 _Color;
half _Shininess;
half _Detail;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float2 uv_BumpMapDetail;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = _Shininess *_SpecColor*2;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess *_SpecColor;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	o.Normal *= UnpackNormal(tex2D(_BumpMapDetail, IN.uv_BumpMapDetail))*_Detail;
}
ENDCG
}

FallBack "Specular"
}

