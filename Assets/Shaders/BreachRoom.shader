Shader "Custom/BreachRoom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _BreachAmount ("Breach Amount", Range(0, 1)) = 1

        _BreachColour ("Breach Colour", Color) = (1, 0, 0, 1)

        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3

        // 0 = no corruption
        // 1 = low corruption
        // 2 = high corruption
        _CorruptionStrength ("Corruption Strength", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Vertex input.
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Data passed from the vertex shader
            // to the fragment shader.
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _BreachAmount;
            float4 _BreachColour;
            float _PulseSpeed;
            float _CorruptionStrength;

            // Vertex shader.
            // Transforms the vertex into clip space
            // and passes the UV coordinates to the fragment shader.
            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // Fragment shader.
            // Calculates the breach colour, pulse,
            // and procedural screen corruption.
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the original screen texture.
                fixed4 original =
                    tex2D(_MainTex, i.uv);

                // Create a value between 0 and 1
                // that changes over time.
                float pulse =
                    (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;

                // Create darker and brighter versions
                // of the selected breach colour.
                float3 darkBreach =
                    _BreachColour.rgb * 0.25;

                float3 brightBreach =
                    _BreachColour.rgb;

                // Interpolate between the dark and bright
                // colours to create the pulsing effect.
                float3 breach =
                    lerp(
                        darkBreach,
                        brightBreach,
                        pulse
                    );

                // Blend the original screen with
                // the breach colour.
                float3 finalRGB =
                    lerp(
                        original.rgb,
                        breach,
                        _BreachAmount
                    );

                // Divide the screen into horizontal sections.
                float section =
                    floor(i.uv.y * 35.0);

                // Generate a pseudo-random value for each section.
                float randomValue =
                    frac(
                        sin(section * 12.9898) *
                        43758.5453
                    );

                // Generate an animated random value.
                // Changing the time value makes different
                // sections become corrupted over time.
                float animatedRandom =
                    frac(
                        sin(
                            section * 78.233 +
                            floor(_Time.y * 7.0)
                        ) *
                        43758.5453
                    );

                // Increase the number of corrupted sections
                // as corruption strength increases.
                float corruptionThreshold =
                    0.99 -
                    (_CorruptionStrength * 0.25);

                // Determine whether this section is corrupted.
                float corruption =
                    step(
                        corruptionThreshold,
                        animatedRandom
                    );

                // Scale corruption by the user-controlled strength.
                float corruptionAmount =
                    corruption *
                    _CorruptionStrength;

                // Calculate a horizontal UV offset.
                float glitchOffset =
                    (randomValue - 0.5) *
                    0.015 *
                    _CorruptionStrength;

                // Only apply the offset to corrupted sections.
                glitchOffset *= corruption;

                float2 glitchUV =
                    i.uv;

                glitchUV.x += glitchOffset;

                // Sample the screen texture using the corrupted UVs.
                fixed4 glitched =
                    tex2D(
                        _MainTex,
                        glitchUV
                    );

                // Blend the corrupted texture into the result.
                finalRGB =
                    lerp(
                        finalRGB,
                        glitched.rgb,
                        corruptionAmount * 0.35
                    );

                // Add a small amount of the breach colour
                // to corrupted sections.
                finalRGB =
                    lerp(
                        finalRGB,
                        finalRGB * 0.75 +
                        _BreachColour.rgb * 0.25,
                        corruptionAmount * 0.35
                    );

                // Preserve the original texture alpha.
                fixed4 finalColour;

                finalColour.rgb =
                    finalRGB;

                finalColour.a =
                    original.a;

                return finalColour;
            }

            ENDCG
        }
    }
}