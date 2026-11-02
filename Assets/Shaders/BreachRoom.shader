
Shader "Custom/BreachRoom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _BreachAmount ("Breach Amount", Range(0, 1)) = 1
        _BreachColour ("Breach Colour", Color) = (1, 0, 0, 1)
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3

        _CorruptionStrength ("Corruption Strength", Range(0, 2)) = 1

        // 0 = no breach, 1-4 = permanently fallen rooms.
        _BreachLevel ("Permanent Breach Level", Range(0, 4)) = 0
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

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _BreachAmount;
            float4 _BreachColour;
            float _PulseSpeed;
            float _CorruptionStrength;
            float _BreachLevel;

            // ==================================================
            // PROCEDURAL RANDOM FUNCTIONS
            // ==================================================

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }

            float noise21(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            // ==================================================
            // VERTEX SHADER
            // ==================================================

            v2f vert(appdata v)
            {
                v2f o;

                float level = saturate(_BreachLevel / 4.0);
                float time = _Time.y;

                float3 displacedVertex = v.vertex.xyz;

                float deformation =
                    _BreachAmount *
                    (0.003 + level * 0.018);

                // Multiple overlapping waves.
                float wave1 =
                    sin(v.vertex.x * 9.0 + time * 2.5);

                float wave2 =
                    cos(v.vertex.y * 13.0 - time * 3.0);

                float wave3 =
                    sin(
                        (v.vertex.x + v.vertex.y) * 17.0
                        + time * 4.0
                    );

                float combinedWave =
                    wave1 * 0.4 +
                    wave2 * 0.35 +
                    wave3 * 0.25;

                // Irregular animated vertex movement.
                float vertexNoise =
                    sin(
                        v.vertex.x * 21.0 +
                        v.vertex.y * 16.0 +
                        time * (3.0 + level * 2.0)
                    );

                float displacement =
                    (combinedWave + vertexNoise * 0.25)
                    * deformation;

                // Displace along the vertex normal.
                displacedVertex +=
                    v.normal * displacement;

                // Additional horizontal movement.
                displacedVertex.x +=
                    sin(v.vertex.y * 12.0 + time * 2.0)
                    * deformation * 0.2;

                // Additional vertical movement.
                displacedVertex.y +=
                    cos(v.vertex.x * 15.0 - time * 2.5)
                    * deformation * 0.2;

                float4 displacedPosition =
                    float4(displacedVertex, 1.0);

                o.vertex =
                    UnityObjectToClipPos(displacedPosition);

                o.uv =
                    TRANSFORM_TEX(v.uv, _MainTex);

                o.localPos = displacedVertex;

                o.worldPos =
                    mul(unity_ObjectToWorld, displacedPosition).xyz;

                o.worldNormal =
                    UnityObjectToWorldNormal(v.normal);

                return o;
            }

            // ==================================================
            // FRAGMENT SHADER
            // ==================================================

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;

                float level =
                    saturate(_BreachLevel / 4.0);

                float intensity = _BreachAmount;

                float corruptionLevel =
                    saturate(
                        (_CorruptionStrength * 0.25) +
                        (level * 0.75)
                    );

                // ------------------------------------------------
                // 1. HEAT DISTORTION
                // ------------------------------------------------

                float heatWave =
                    sin(i.uv.y * 35.0 + time * 3.0) *
                    cos(i.uv.x * 24.0 - time * 2.0);

                float heatStrength =
                    intensity *
                    (0.001 + level * 0.012);

                float2 distortedUV = i.uv;

                distortedUV.x +=
                    heatWave * heatStrength;

                distortedUV.y +=
                    sin(i.uv.x * 30.0 + time * 2.0)
                    * heatStrength * 0.4;

                fixed4 original =
                    tex2D(_MainTex, distortedUV);

                // ------------------------------------------------
                // 2. RED BREACH PULSE
                // ------------------------------------------------

                float pulse =
                    (sin(time * _PulseSpeed) + 1.0) * 0.5;

                float aggressivePulse =
                    (sin(time * (_PulseSpeed + level * 5.0)) + 1.0)
                    * 0.5;

                float3 darkBreach =
                    _BreachColour.rgb * 0.2;

                float3 brightBreach =
                    _BreachColour.rgb *
                    (0.8 + aggressivePulse * (0.4 + level));

                float3 finalRGB =
                    lerp(
                        original.rgb,
                        lerp(darkBreach, brightBreach, pulse),
                        intensity
                    );

                // ------------------------------------------------
                // 3. HORIZONTAL CORRUPTION BANDS
                // ------------------------------------------------

                float bandCount =
                    25.0 + level * 55.0;

                float section =
                    floor(i.uv.y * bandCount);

                float randomValue =
                    hash21(float2(section, 4.0));

                float animatedRandom =
                    hash21(
                        float2(
                            section,
                            floor(time * (5.0 + level * 8.0))
                        )
                    );

                float corruptionThreshold =
                    0.98 - corruptionLevel * 0.55;

                float corruption =
                    step(
                        corruptionThreshold,
                        animatedRandom
                    );

                float glitchOffset =
                    (randomValue - 0.5) *
                    (0.008 + level * 0.025) *
                    corruption;

                float2 glitchUV = distortedUV;
                glitchUV.x += glitchOffset;

                fixed3 glitched =
                    tex2D(_MainTex, glitchUV).rgb;

                finalRGB =
                    lerp(
                        finalRGB,
                        glitched,
                        corruption * intensity * 0.65
                    );

                // ------------------------------------------------
                // 4. DIGITAL SCANLINES
                // ------------------------------------------------

                float scanline =
                    sin(i.uv.y * (500.0 + level * 400.0));

                float scanlineStrength =
                    (0.04 + level * 0.12) * intensity;

                finalRGB *=
                    1.0 - abs(scanline) * scanlineStrength;

                // ------------------------------------------------
                // 5. PROCEDURAL ELECTRICAL CRACKS
                // ------------------------------------------------

                float2 crackUV =
                    i.uv * float2(12.0, 9.0);

                float2 cell =
                    floor(crackUV);

                float2 cellUV =
                    frac(crackUV) - 0.5;

                float crackRandom =
                    hash21(cell);

                float crackDirection =
                    (crackRandom - 0.5) * 1.5;

                float crackLine =
                    abs(
                        cellUV.y -
                        cellUV.x * crackDirection
                    );

                float crackBranches =
                    abs(
                        cellUV.x +
                        sin(cellUV.y * 18.0 + crackRandom * 8.0)
                        * 0.12
                    );

                float crack =
                    1.0 - smoothstep(
                        0.015 + level * 0.008,
                        0.055 + level * 0.012,
                        crackLine
                    );

                crack *=
                    1.0 - smoothstep(
                        0.04,
                        0.12,
                        crackBranches
                    );

                float crackMask =
                    step(
                        0.65 - level * 0.35,
                        crackRandom
                    );

                float crackFlicker =
                    0.65 +
                    0.35 * sin(
                        time * (7.0 + level * 6.0)
                        + crackRandom * 20.0
                    );

                crack *=
                    crackMask *
                    crackFlicker *
                    corruptionLevel *
                    intensity;

                finalRGB +=
                    _BreachColour.rgb *
                    crack *
                    (0.5 + level * 2.0);

                // ------------------------------------------------
                // 6. DIGITAL STATIC
                // ------------------------------------------------

                float2 staticUV =
                    i.uv * float2(250.0, 180.0);

                float staticNoise =
                    hash21(
                        floor(staticUV) +
                        floor(time * (10.0 + level * 15.0))
                    );

                float staticThreshold =
                    0.97 - level * 0.18;

                float staticAmount =
                    step(staticThreshold, staticNoise) *
                    corruptionLevel *
                    intensity;

                finalRGB +=
                    _BreachColour.rgb *
                    staticAmount *
                    0.8;

                // Occasional bright static flashes.
                float flashNoise =
                    hash21(float2(floor(time * 9.0), 17.0));

                float flash =
                    step(
                        0.96 - level * 0.12,
                        flashNoise
                    );

                finalRGB +=
                    _BreachColour.rgb *
                    flash *
                    level *
                    intensity *
                    0.25;

                // ------------------------------------------------
                // 7. RED CORRUPTION GLOW
                // ------------------------------------------------

                float edgeDistance =
                    min(
                        min(i.uv.x, 1.0 - i.uv.x),
                        min(i.uv.y, 1.0 - i.uv.y)
                    );

                float edgeGlow =
                    1.0 - smoothstep(
                        0.0,
                        0.35 + level * 0.15,
                        edgeDistance
                    );

                float glowStrength =
                    edgeGlow *
                    aggressivePulse *
                    intensity *
                    (0.15 + level * 0.65);

                finalRGB +=
                    _BreachColour.rgb *
                    glowStrength;

                // ------------------------------------------------
                // 8. FINAL COLOUR
                // ------------------------------------------------

                finalRGB = saturate(finalRGB);

                fixed4 finalColour;

                finalColour.rgb = finalRGB;
                finalColour.a = original.a;

                return finalColour;
            }

            ENDCG
        }
    }
}
