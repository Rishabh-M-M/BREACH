Shader "Custom/Shield"
{
    Properties
    {
        _MainTex ("Hex Texture", 2D) = "white" {}

        _HealthyColor ("Healthy", Color) = (0,1,1,1)
        _MidColor ("Mid", Color) = (1,1,0,1)
        _CriticalColor ("Critical", Color) = (1,0,0,1)

        _Health ("Health", Range(0,1)) = 1
        _Opacity ("Opacity", Range(0,1)) = 0.14
        _Glow ("Glow", Range(0,5)) = 3.5
        _HitFlash ("Hit Flash", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _HealthyColor;
            fixed4 _MidColor;
            fixed4 _CriticalColor;

            float _Health;
            float _Opacity;
            float _Glow;
            float _HitFlash;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                float fresnel =
                    pow(1 - saturate(dot(normalize(i.worldNormal), viewDir)), 3);

                // Cyan -> Yellow -> Red
                float t = 1 - _Health;

                fixed3 col;

                if (t < 0.5)
                    col = lerp(_HealthyColor.rgb, _MidColor.rgb, t * 2);
                else
                    col = lerp(_MidColor.rgb, _CriticalColor.rgb, (t - 0.5) * 2);

                // Hex hologram
                fixed3 hex = tex.rgb * col;

                // Animated scanlines
                float scan =
                    sin(i.uv.y * 180 + _Time.y * 8) * 0.5 + 0.5;

                // Edge glow
                fixed3 edgeGlow = col * fresnel * (_Glow * 2.2);

                fixed3 finalColor =
                    hex * 1.4 +
                    edgeGlow +
                    col * scan * 0.18 +
                    _HitFlash;

                // Transparency
                float alpha = tex.a * (_Opacity + 0.08);
                alpha += fresnel * 0.22;

                return fixed4(finalColor, saturate(alpha));
            }

            ENDCG
        }
    }
}