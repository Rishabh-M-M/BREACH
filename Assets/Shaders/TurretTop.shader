Shader "ProjectBreach/TurretTop"
{
    // Built-in Render Pipeline (CGPROGRAM). Uses the top mesh texture (image_10).
    //
    // v2: Uses a fixed, self-contained fake light direction instead of the scene's
    // real light, so the "activated" look always shows the true texture colours
    // regardless of how the scene is lit. Deactivated now dims toward a shadow
    // (same hue, lower brightness) instead of desaturating to grayscale.

    Properties
    {
        _MainTex ("Top Texture (image_10)", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _Activation ("Activation (0 = Off, 1 = On)", Range(0, 1)) = 0
        _ShadowBrightness ("Deactivated Brightness", Range(0, 1)) = 0.18

        _Shininess ("Shininess", Range(1, 256)) = 40
        _SpecularStrength ("Specular Strength", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos    : TEXCOORD2;
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

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv) * _Color;

                float activation = saturate(_Activation);

                // Same hue at all times — only brightness changes between shadowed (off) and full (on)
                float brightness = lerp(_ShadowBrightness, 1.0, activation);
                float3 baseColor = texCol.rgb * brightness;

                // Fixed fake light direction: independent of scene lighting setup, so the model
                // always reads with clear form/depth instead of relying on _LightColor0.
                float3 normal = normalize(i.worldNormal);
                float3 fakeLightDir = normalize(float3(0.4, 0.85, 0.35));
                float ndotl = max(0, dot(normal, fakeLightDir));

                // 70%-100% range: keeps some shading depth without ever crushing to black
                float3 shaded = baseColor * (0.7 + 0.3 * ndotl);

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(fakeLightDir + viewDir);
                float spec = pow(max(0, dot(normal, halfDir)), _Shininess);
                float3 specular = spec * _SpecularStrength * brightness;

                return fixed4(shaded + specular, texCol.a);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
