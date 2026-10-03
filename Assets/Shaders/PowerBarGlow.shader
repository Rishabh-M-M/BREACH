Shader "Custom/PowerBarGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // Main blue colour
        _GlowColour ("Glow Colour", Color) = (0, 0.35, 1, 1)

        // Pulse
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 4
        _GlowStrength ("Glow Strength", Range(0, 2)) = 0.7

        // Liquid movement
        _FlowSpeed ("Flow Speed", Range(0, 5)) = 1.5
        _FlowStrength ("Flow Strength", Range(0, 1)) = 0.25
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

            float _FlowSpeed;
            float _FlowStrength;

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

                // --------------------------------
                // PULSING ENERGY
                // --------------------------------

                float pulse = (sin(_Time.y * _PulseSpeed) + 1) / 2;

                float glow = 1 + pulse * _GlowStrength;


                // --------------------------------
                // LIQUID FLOW
                // --------------------------------

                // Moving wave across the bar
                float wave = sin(
                    (i.uv.x * 12)
                    - (_Time.y * _FlowSpeed * 4)
                );

                // Convert wave into a soft highlight
                float liquid = (wave + 1) / 2;

                liquid *= _FlowStrength;


                // --------------------------------
                // FINAL COLOUR
                // --------------------------------

                fixed4 result = tex * _GlowColour;

                // Pulse brightness
                result.rgb *= glow;

                // Moving liquid highlight
                result.rgb += liquid;

                // Preserve transparency
                result.a *= i.color.a;

                return result;
            }

            ENDCG
        }
    }
}