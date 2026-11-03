Shader "HitReaction"
{
	Properties
	{
		_MainTex ("Albedo", 2D) = "white" {}
		_Shininess ("Shininess", Range(1, 256)) = 60

		// Hit ring
		_HitOrigin ("Hit Origin (World Offset From Root)", Vector) = (0, 1, 0, 0)
		_HitTime ("Hit Time", Float) = -100
		_RingSpeed ("Ring Speed", Float) = 35
		_RingWidth ("Ring Width", Range(0.01, 10)) = 2
		_RingColour ("Ring Colour", Color) = (1, 0, 0, 1)
		_RingMaxRadius ("Ring Max Radius", Float) = 15

		// Hit shake
		_ShakeAmplitude ("Shake Amplitude", Float) = 0.2
		_ShakeFrequency ("Shake Frequency", Float) = 45
		_ShakeDecay ("Shake Decay", Float) = 5
	}
	SubShader
	{
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _Shininess;

			uniform float4 _HitOrigin;
			uniform float _HitTime;
			uniform float _RingSpeed;
			uniform float _RingWidth;
			uniform float4 _RingColour;
			uniform float _RingMaxRadius;

			uniform float _ShakeAmplitude;
			uniform float _ShakeFrequency;
			uniform float _ShakeDecay;

			struct vertIn
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct vertOut
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float hitDist : TEXCOORD3;
				float hitTimeDelta : TEXCOORD4;
			};

			vertOut vert(vertIn v)
			{
				float t = max(0.0, _Time.y - _HitTime);

				float decay = exp(-_ShakeDecay * t);
				float shakeOffset = sin(t * _ShakeFrequency) * _ShakeAmplitude * decay;
				float3 shakenVertex = v.vertex.xyz + normalize(v.normal) * shakeOffset;

				vertOut o;

				float4 worldPos = mul(unity_ObjectToWorld, float4(shakenVertex, 1.0));
				float3 rootWorldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				float3 worldHitOrigin = rootWorldPos + _HitOrigin.xyz;

				o.worldPos = worldPos.xyz;
				o.hitDist = distance(worldPos.xyz, worldHitOrigin);
				o.hitTimeDelta = t;

				o.vertex = UnityObjectToClipPos(float4(shakenVertex, 1.0));
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			fixed4 frag(vertOut v) : SV_Target
			{
				fixed4 albedo = tex2D(_MainTex, v.uv);

				float3 normal = normalize(v.worldNormal);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
				float3 viewDir = normalize(_WorldSpaceCameraPos - v.worldPos);
				float3 halfDir = normalize(lightDir + viewDir);

				float ndotl = max(0, dot(normal, lightDir));
				float spec = pow(max(0, dot(normal, halfDir)), _Shininess);

				float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo.rgb;
				float3 diffuse = albedo.rgb * _LightColor0.rgb * ndotl;
				float3 specular = _LightColor0.rgb * spec;
				float3 litColor = ambient + diffuse + specular;

				float t = v.hitTimeDelta;
				float ringRadius = t * _RingSpeed;
				float distToRing = abs(v.hitDist - ringRadius);
				float ringMask = 1 - smoothstep(0, _RingWidth, distToRing);

				float lifeFade = saturate(1 - ringRadius / _RingMaxRadius);
				float active = step(ringRadius, _RingMaxRadius);

				float3 ringGlow = _RingColour.rgb * ringMask * lifeFade * active;

				return fixed4(litColor + ringGlow, albedo.a);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}