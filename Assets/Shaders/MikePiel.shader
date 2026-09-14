Shader "MonsterInc/MikePiel"
{
    Properties
    {
        _Color ("Color", Color) = (0.50, 0.78, 0.15, 1)
        _PoreScale ("Pore Scale", Float) = 48
        _PoreAmount ("Pore Amount", Float) = 0.04
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        float _PoreScale;
        float _PoreAmount;

        struct Input
        {
            float3 worldPos;
        };

        float Hash3(float3 p)
        {
            p = frac(p * 0.3183099 + 0.1);
            p *= 17.0;
            return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
        }

        float IsoNoise(float3 x)
        {
            float3 i = floor(x);
            float3 f = frac(x);
            f = f * f * (3.0 - 2.0 * f);
            return lerp(
                lerp(
                    lerp(Hash3(i), Hash3(i + float3(1, 0, 0)), f.x),
                    lerp(Hash3(i + float3(0, 1, 0)), Hash3(i + float3(1, 1, 0)), f.x),
                    f.y),
                lerp(
                    lerp(Hash3(i + float3(0, 0, 1)), Hash3(i + float3(1, 0, 1)), f.x),
                    lerp(Hash3(i + float3(0, 1, 1)), Hash3(i + float3(1, 1, 1)), f.x),
                    f.y),
                f.z);
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 obj = mul(unity_WorldToObject, float4(IN.worldPos, 1)).xyz;
            float n = IsoNoise(obj * _PoreScale);
            float pores = saturate((n - 0.55) * 1.6);
            o.Albedo = _Color.rgb * (0.92 + 0.08 * (1.0 - _PoreAmount * pores));
            o.Alpha = 1.0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
