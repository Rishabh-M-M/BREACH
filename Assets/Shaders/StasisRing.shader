Shader "Custom/StasisRing"
{
    Properties
    {
        _MainTex ("Ring Texture", 2D) = "white" {}
        _OffColor ("Unpowered Colour", Color) = (0.38, 0.40, 0.43, 1)
        _OnColor ("Powered Hologram Colour", Color) = (0.05, 0.85, 1.0, 1)
        _Powered ("Powered Blend", Range(0,1)) = 0
        _OffOpacity ("Unpowered Opacity", Range(0,1)) = 1
        _OnOpacity ("Powered Opacity", Range(0,1)) = 0.42
        _Glow ("Hologram Glow", Range(0,5)) = 2.2
        _ScanSpeed ("Scan Speed", Range(0,10)) = 2.5
        _ScanStrength ("Scanline Strength", Range(0,1)) = 0.18
        _FresnelPower ("Edge Fresnel", Range(0.5,8)) = 3
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

            fixed4 _OffColor;
            fixed4 _OnColor;
            float _Powered;
            float _OffOpacity;
            float _OnOpacity;
            float _Glow;
            float _ScanSpeed;
            float _ScanStrength;
            float _FresnelPower;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float powered = saturate(_Powered);
                fixed4 tex = tex2D(_MainTex, i.uv);

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(
                    1.0 - saturate(dot(normalize(i.worldNormal), viewDir)),
                    _FresnelPower
                );

                // Subtle moving scan bands, visible only while powered.
                float scan = 0.5 + 0.5 * sin((i.uv.y * 150.0) - (_Time.y * _ScanSpeed * 8.0));
                float scanGlow = scan * _ScanStrength * powered;

                fixed3 offColour = _OffColor.rgb * tex.rgb;
                fixed3 hologramColour = _OnColor.rgb * tex.rgb;
                hologramColour += _OnColor.rgb * (fresnel * _Glow + scanGlow);

                fixed3 finalColour = lerp(offColour, hologramColour, powered);
                float alpha = lerp(_OffOpacity, _OnOpacity, powered);

                // Texture alpha is retained in both states.
                alpha *= tex.a;

                return fixed4(finalColour, saturate(alpha));
            }
            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}
