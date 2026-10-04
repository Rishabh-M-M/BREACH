Shader "Custom/CameraGlitch"
{
    Properties
    {
        _GlitchStrength ("Glitch Strength", Range(0, 1)) = 0.8
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.5
        _LineDensity ("Line Density", Range(50, 500)) = 250
        _StaticStrength ("Static Strength", Range(0, 1)) = 0.25
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

            float _GlitchStrength;
            float _ScanlineStrength;
            float _LineDensity;
            float _StaticStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // -----------------------------------------
            // RANDOM
            // -----------------------------------------

            float random(float2 p)
            {
                return frac(
                    sin(
                        dot(
                            p,
                            float2(12.9898, 78.233)
                        )
                    ) * 43758.5453
                );
            }

            // -----------------------------------------
            // VERTEX
            // -----------------------------------------

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex =
                    UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;

                return o;
            }

            // -----------------------------------------
            // FRAGMENT
            // -----------------------------------------

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // =====================================
                // SCANLINES
                // =====================================

                float scanline =
                    sin(
                        uv.y * _LineDensity
                    );

                scanline =
                    (scanline + 1.0) * 0.5;

                // Only create a subtle line
                float scan =
                    scanline
                    * _ScanlineStrength;

                // =====================================
                // HORIZONTAL GLITCH BANDS
                // =====================================

                float bandRandom =
                    random(
                        float2(
                            floor(uv.y * 45.0),
                            floor(_Time.y * 25.0)
                        )
                    );

                float glitchBand =
                    step(0.82, bandRandom);

                // =====================================
                // MOVING GLITCH LINES
                // =====================================

                float movingLines =
                    random(
                        float2(
                            floor(uv.y * 180.0),
                            floor(_Time.y * 45.0)
                        )
                    );

                float glitchLine =
                    step(
                        0.94,
                        movingLines
                    );

                // =====================================
                // STATIC
                // =====================================

                float noise =
                    random(
                        floor(
                            uv * 500.0
                            + _Time.y * 70.0
                        )
                    );

                // =====================================
                // CYAN SCANLINE
                // =====================================

                float cyanLine =
                    step(
                        0.94,
                        random(
                            float2(
                                floor(uv.y * 300.0),
                                floor(_Time.y * 35.0)
                            )
                        )
                    );

                // =====================================
                // COMBINE
                // =====================================

                float alpha = 0.0;

                // Normal scanlines
                alpha +=
                    scan * 0.18;

                // Horizontal glitch bands
                alpha +=
                    glitchBand
                    * _GlitchStrength
                    * 0.35;

                // Small bright glitch lines
                alpha +=
                    glitchLine
                    * _GlitchStrength
                    * 0.65;

                // Static
                alpha +=
                    step(
                        0.93,
                        noise
                    )
                    * _StaticStrength;

                alpha = saturate(alpha);

                // =====================================
                // COLOUR
                // =====================================

                float3 cyan =
                    float3(
                        0.05,
                        0.65,
                        1.0
                    );

                float3 white =
                    float3(
                        0.75,
                        0.9,
                        1.0
                    );

                float3 finalColour =
                    lerp(
                        cyan,
                        white,
                        glitchLine
                    );

                // Some glitch bands become darker
                if (glitchBand > 0.5)
                {
                    finalColour *= 0.35;
                }

                return fixed4(
                    finalColour,
                    alpha
                );
            }

            ENDCG
        }
    }
}
