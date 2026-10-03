Shader "Custom/PowerBarGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // Strong blue base colour
        _GlowColour ("Glow Colour", Color) = (0, 0.35, 1, 1)

        // Faster pulse
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 4

        // Glow intensity
        _GlowStrength ("Glow Strength", Range(0, 2)) = 0.7
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _GlowColour;
            float _PulseSpeed;
            float _GlowStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Pulsing animation
                float pulse = (sin(_Time.y * _PulseSpeed) + 1) / 2;

                // Increase brightness during pulse
                float glow = 1 + pulse * _GlowStrength;

                fixed4 result = tex * _GlowColour * glow;

                result.a *= i.color.a;

                return result;
            }

            ENDCG
        }
    }
}
