
Shader "Breach/MainMenuGradient"
{
    Properties
    {
        _Blue ("Deep Space Blue", Color) = (0.005, 0.015, 0.09, 1)
        _Purple ("Nebula Purple", Color) = (0.20, 0.025, 0.38, 1)
        _Pink ("Nebula Pink", Color) = (0.45, 0.025, 0.22, 1)

        _Speed ("Nebula Movement Speed", Float) = 0.025
        _StarDensity ("Star Density", Float) = 180.0
        _NebulaStrength ("Nebula Strength", Float) = 0.8

        _ShootingStarRate ("Shooting Star Rate", Range(0.1, 5)) = 1
        _ShootingStarDuration ("Shooting Star Duration", Range(0.2, 2)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Opaque"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            fixed4 _Blue;
            fixed4 _Purple;
            fixed4 _Pink;

            float _Speed;
            float _StarDensity;
            float _NebulaStrength;
            float _ShootingStarRate;
            float _ShootingStarDuration;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(lerp(a, b, f.x),
                            lerp(c, d, f.x), f.y);
            }

            float segmentDistance(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;

                float h = saturate(dot(pa, ba) / dot(ba, ba));

                return length(pa - ba * h);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;

                // Deep space base
                float3 colour = _Blue.rgb;

                // Nebula clouds
                float2 cloudUV = uv * float2(3.0, 2.0);
                cloudUV += float2(time, time * 0.35);

                float n1 = noise(cloudUV * 2.0);
                float n2 = noise(cloudUV * 4.0 + 12.5);
                float n3 = noise(cloudUV * 7.0 - 4.0);

                float clouds = n1 * 0.55 + n2 * 0.30 + n3 * 0.15;
                clouds = smoothstep(0.38, 0.78, clouds);

                // Blue nebula
                float blueRegion = 1.0 - smoothstep(0.05, 0.58, uv.x);
                float blueCloud = clouds * blueRegion;

                // Purple and pink nebula
                float pinkRegion = smoothstep(0.35, 0.95, uv.x);
                float purpleCloud = clouds * pinkRegion;

                colour += float3(0.015, 0.12, 0.45) *
                          blueCloud * _NebulaStrength;

                colour += _Purple.rgb *
                          purpleCloud * _NebulaStrength;

                colour += _Pink.rgb *
                          purpleCloud * clouds * 0.55 *
                          _NebulaStrength;

                // Central cosmic haze
                float centreGlow = exp(-pow((uv.x - 0.5) * 3.0, 2.0));
                colour += float3(0.035, 0.012, 0.09) * centreGlow;

                // Static star field
                float2 starUV = uv * _StarDensity;
                float2 cell = floor(starUV);
                float2 local = frac(starUV) - 0.5;

                float starRandom = hash21(cell);
                float starSize = lerp(0.012, 0.045, starRandom);

                float star = 1.0 - smoothstep(
                    starSize * 0.25,
                    starSize,
                    length(local)
                );

                star *= step(0.965, starRandom);

                float twinkle = 0.65 +
                    0.35 * sin(_Time.y * 2.0 + starRandom * 40.0);

                colour += star * twinkle * float3(0.65, 0.78, 1.0);

                // Brighter stars
                float brightStar = 1.0 - smoothstep(
                    0.002, 0.012, length(local)
                );

                brightStar *= step(0.994, starRandom);
                colour += brightStar * float3(0.65, 0.75, 1.0);

                // =====================================
                // SHOOTING STARS
                // =====================================

                // Rate controls how quickly random intervals pass.
                // At 1: approximately 1-3 seconds between stars.
                // Higher values make them appear more frequently.
                float scaledTime = _Time.y * _ShootingStarRate;

                float cycle = floor(scaledTime / 2.0);
                float randomDelay = hash21(float2(cycle, 17.3));

                float spawnTime = cycle * 2.0 + randomDelay;
                float elapsed = scaledTime - spawnTime;

                // Random starting position
                float randomX = hash21(float2(cycle, 4.7));
                float randomY = hash21(float2(cycle, 9.2));

                float2 startPosition = float2(
                    lerp(0.35, 1.15, randomX),
                    lerp(0.85, 1.2, randomY)
                );

                // Top-right towards bottom-left
                float2 direction = normalize(float2(-1.0, -1.0));

                float duration = max(_ShootingStarDuration, 0.01);
                float progress = elapsed / (duration * _ShootingStarRate);

                float active = step(0.0, elapsed) *
                    (1.0 - step(duration * _ShootingStarRate, elapsed));

                float2 headPosition =
                    startPosition + direction * progress * 1.8;

                float2 tailPosition =
                    headPosition - direction * 0.13;

                float trailDistance =
                    segmentDistance(uv, headPosition, tailPosition);

                float trailGlow = exp(-trailDistance * 180.0);
                float trailCore = exp(-trailDistance * 900.0);

                float fade = sin(saturate(progress) * 3.14159);

                float shootingStar =
                    (trailGlow * 0.35 + trailCore) *
                    fade * active;

                colour += shootingStar *
                    float3(0.55, 0.75, 1.0) * 1.8;

                // Bright glowing head
                float headGlow =
                    exp(-length(uv - headPosition) * 65.0);

                colour += headGlow * fade * active *
                    float3(0.75, 0.85, 1.0) * 0.8;

                return fixed4(saturate(colour), 1);
            }
            ENDCG
        }
    }
}
