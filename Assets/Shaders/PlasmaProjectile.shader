Shader "Custom/PlasmaProjectile"
{
    Properties
    {
        _Color ("Plasma Color (HDR)", Color) = (0, 0.7, 1, 1)
        _RimPower ("Edge Glow Sharpness", Range(0.5, 8)) = 2.5
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3
        _GlowStrength ("Glow Strength", Range(0, 8)) = 3
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseSpeed ("Noise Scroll Speed", Range(0, 5)) = 1
        _Wobble ("Vertex Wobble", Range(0, 0.2)) = 0.03
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            fixed4 _Color;
            float _RimPower;
            float _PulseSpeed;
            float _GlowStrength;
            float _NoiseSpeed;
            float _Wobble;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                // Vertex stage: wobble along normal for plasma instability
                float wave = sin(_Time.y * _PulseSpeed * 2.0 + v.vertex.x * 10.0 + v.vertex.y * 10.0);
                float3 displaced = v.vertex.xyz + v.normal * wave * _Wobble;
                o.worldPos = mul(unity_ObjectToWorld, float4(displaced, 1.0)).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.pos = UnityObjectToClipPos(displaced);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Fragment stage: fresnel rim glow
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float rim = 1.0 - saturate(dot(viewDir, normalize(i.worldNormal)));
                float fresnel = pow(rim, _RimPower);

                // Breathing pulse
                float pulse = 0.85 + 0.15 * sin(_Time.y * _PulseSpeed);

                // Scrolling noise turbulence
                float2 noiseUV = i.uv + float2(_Time.y * _NoiseSpeed, 0);
                float noise = tex2D(_NoiseTex, noiseUV).r;
                float turbulence = lerp(1.0, noise * 2.0, 0.35);

                float glow = (fresnel * 2.0 + 0.35) * pulse * turbulence * _GlowStrength;

                fixed3 col = _Color.rgb * glow;
                float alpha = saturate(fresnel + 0.35);
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
    FallBack Off
}