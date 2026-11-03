Shader "Custom/BossConsumeDeform"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Vortex Consume Deformation)]
        _Progress ("Consume Progress", Range(0,1)) = 0
        _ModelHeight ("Model Height (local units, feet to head)", Float) = 2.0
        _WipeSoftness ("Wipe Softness", Range(0.01, 1)) = 0.25
        _TwistAmount ("Twist Amount (radians)", Float) = 6.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;

        float _Progress;
        float _ModelHeight;
        float _WipeSoftness;
        float _TwistAmount;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full v)
        {
            // Normalised height along the model, assuming local Y=0 at the feet.
            float heightNorm = saturate(v.vertex.y / max(_ModelHeight, 0.0001));

            // Vertices near the feet reach full "consumed" state before vertices
            // near the head do - a wave of consumption travelling up the model
            // as _Progress rises from 0 (fully formed) to 1 (fully sucked in).
            float consumed = saturate((_Progress - heightNorm) / max(_WipeSoftness, 0.0001));

            // Spiral twist around the local up-axis, stronger on consumed vertices.
            float angle = consumed * _TwistAmount;
            float s, c;
            sincos(angle, s, c);
            float2 xz = v.vertex.xz;
            v.vertex.xz = float2(xz.x * c - xz.y * s, xz.x * s + xz.y * c);

            // Pull consumed vertices in toward the vertical axis and down toward
            // the vortex plane (local Y = 0) - this reads as "sucked in" rather
            // than just shrinking in place.
            v.vertex.xz *= (1.0 - consumed);
            v.vertex.y *= (1.0 - consumed);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}