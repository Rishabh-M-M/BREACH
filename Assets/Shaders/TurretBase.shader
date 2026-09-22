Shader "ProjectBreach/TurretBase"
{
    // Built-in Render Pipeline (CGPROGRAM). Uses the base mesh texture (image_8).

    Properties
    {
        _MainTex ("Base Texture (image_8)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Activation ("Activation (0 = Off, 1 = On)", Range(0,1)) = 0
        _ShadowBrightness ("Deactivated Brightness", Range(0,1)) = 0.18

        _Shininess ("Shininess", Range(1,256)) = 40
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.25

        [Header(Glow Bands)]
        _GlowColor ("Glow Colour", Color) = (0,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0,8)) = 3
        _BandCenter ("Band Center (V)", Range(0,1)) = 0.5
        _BandSpacing ("Band Spacing", Range(0,0.5)) = 0.14
        _BandWidth ("Band Width", Range(0.001,0.2)) = 0.025
        _BandSoftness ("Band Edge Softness", Range(0.001,0.1)) = 0.015
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            float _Activation;
            float _ShadowBrightness;

            float _Shininess;
            float _SpecularStrength;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _BandCenter;
            float _BandSpacing;
            float _BandWidth;
            float _BandSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Soft horizontal band mask
            float Band(float v, float center, float width, float softness)
            {
                float halfW = width * 0.5;
                float lower = center - halfW;
                float upper = center + halfW;

                return smoothstep(lower - softness, lower, v)
                     - smoothstep(upper, upper + softness, v);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv) * _Color;

                float activation = saturate(_Activation);

                // Powered-down brightness
                float brightness = lerp(_ShadowBrightness, 1.0, activation);
                float3 baseColor = texCol.rgb * brightness;

                // Fixed Blinn–Phong lighting
                float3 N = normalize(i.worldNormal);
                float3 L = normalize(float3(0.4, 0.85, 0.35));

                float ndotl = max(0, dot(N, L));
                float3 diffuse = baseColor * (0.7 + 0.3 * ndotl);

                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 H = normalize(L + V);

                float spec = pow(max(0, dot(N, H)), _Shininess);
                float3 specular = spec * _SpecularStrength * brightness;

                float3 litColor = diffuse + specular;

                // Three horizontal cyan energy bands
                float bandMask = saturate(
                    Band(i.uv.y, _BandCenter - _BandSpacing, _BandWidth, _BandSoftness) +
                    Band(i.uv.y, _BandCenter,                 _BandWidth, _BandSoftness) +
                    Band(i.uv.y, _BandCenter + _BandSpacing, _BandWidth, _BandSoftness)
                );

                float3 glow =
                    _GlowColor.rgb *
                    bandMask *
                    _GlowIntensity *
                    activation;

                return fixed4(litColor + glow, texCol.a);
            }

            ENDCG
        }
    }

    Fallback "Diffuse"
}
