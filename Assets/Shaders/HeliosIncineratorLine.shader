Shader "ProjectBreach/Helios Incinerator Line"
{
    Properties
    {
        [HDR] _CoreColor ("Core Colour", Color) = (0.75, 1.0, 1.0, 1.0)
        [HDR] _GlowColor ("Glow Colour", Color) = (0.0, 0.8, 1.0, 1.0)
        [HDR] _EndColor ("Beam End Colour", Color) = (1.0, 0.45, 0.05, 1.0)
        _CoreWidth ("Core Width", Range(0.01, 1.0)) = 0.22
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.5)) = 0.12
        _GlowStrength ("Glow Strength", Range(0, 8)) = 2.5
        _PulseSpeed ("Pulse Speed", Range(0, 20)) = 5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.15
        _FlowSpeed ("Flow Speed", Range(-20, 20)) = 8
        _FlowScale ("Flow Scale", Range(0, 100)) = 24
        _NoiseStrength ("Flow Variation", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "HeliosLaser"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _CoreColor;
            fixed4 _GlowColor;
            fixed4 _EndColor;
            float _CoreWidth;
            float _EdgeSoftness;
            float _GlowStrength;
            float _PulseSpeed;
            float _PulseAmount;
            float _FlowSpeed;
            float _FlowScale;
            float _NoiseStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float across = abs(input.uv.y - 0.5) * 2.0;
                float core = 1.0 - smoothstep(_CoreWidth, _CoreWidth + _EdgeSoftness, across);
                float outerGlow = 1.0 - smoothstep(0.12, 1.0, across);

                float flow = sin((input.uv.x * _FlowScale - _Time.y * _FlowSpeed) * 6.2831853);
                float variation = 1.0 + flow * _NoiseStrength;
                float pulse = 1.0 - _PulseAmount
                    + _PulseAmount * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));

                // UV.x creates a persistent gradient along the beam. The
                // LineRenderer's animated start/end colours change this tint as it charges.
                float endBlend = saturate(input.uv.x);
                fixed3 beamTint = lerp(_GlowColor.rgb, _EndColor.rgb, endBlend);
                fixed3 coreTint = lerp(_CoreColor.rgb, _EndColor.rgb, endBlend);

                float3 rgb = (beamTint * outerGlow * _GlowStrength
                            + coreTint * core * 2.5);
                rgb *= max(0.0, variation) * pulse * input.color.rgb;
                return fixed4(rgb, saturate(max(core, outerGlow) * input.color.a));
            }
            ENDCG
        }
    }
    FallBack Off
}
