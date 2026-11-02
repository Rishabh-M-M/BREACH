Shader "Custom/BossVortex"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _CoreColor ("Core Color", Color) = (0,0,0,1)
        _RimColor ("Rim Color", Color) = (0.4,0,0.6,1)
        _SwirlSpeed ("Swirl Speed", Float) = 2.0
        _SwirlTightness ("Swirl Tightness", Float) = 4.0
        _Progress ("Progress", Range(0,1)) = 0
        _EdgeSoftness ("Edge Softness", Range(0.01,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
            fixed4 _CoreColor;
            fixed4 _RimColor;
            float _SwirlSpeed;
            float _SwirlTightness;
            float _Progress;
            float _EdgeSoftness;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centred = i.uv - 0.5;
                float dist = length(centred) * 2.0; // 0 at centre, 1 at edge
                float angle = atan2(centred.y, centred.x);

                // Rotate the sample angle by distance and time to get a spiral.
                float swirl = angle + dist * _SwirlTightness - _Time.y * _SwirlSpeed;
                float2 swirlUV = float2(swirl / (2 * UNITY_PI) + 0.5, dist);
                fixed noise = tex2D(_MainTex, swirlUV).r;

                // Only visible out to a radius driven by _Progress - this is
                // what makes the vortex look like it's opening or closing.
                float radiusMask = 1.0 - smoothstep(_Progress - _EdgeSoftness, _Progress, dist);

                fixed4 col = lerp(_CoreColor, _RimColor, dist) * noise;
                col.a = radiusMask * noise;
                return col;
            }
            ENDCG
        }
    }
}