Shader "WonderDoor/Sky_WindowPanel"
{
    Properties
    {
        _SkyTint ("Sky Tint", Color) = (0.35, 0.65, 1.0, 1)
        _HorizonColor ("Horizon Color", Color) = (0.72, 0.88, 1.0, 1)
        _ZenithColor ("Zenith Color", Color) = (0.15, 0.45, 0.95, 1)
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Range(0.2, 4)) = 1.4
        _CloudDensity ("Cloud Density", Range(0, 2)) = 1.1
        _CloudScale ("Cloud Scale", Range(0.2, 8)) = 2.2
        _CloudSoftness ("Cloud Softness", Range(0.05, 1)) = 0.35
        _HorizonLift ("Hide Ground (Horizon Lift)", Range(-0.5, 0.8)) = 0.28
        _UpBias ("Look Up Bias", Range(0, 0.8)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry-10"
        }
        Cull Off
        ZWrite On

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _SkyTint;
                half4 _HorizonColor;
                half4 _ZenithColor;
                half4 _CloudColor;
                half _Exposure;
                half _CloudDensity;
                half _CloudScale;
                half _CloudSoftness;
                half _HorizonLift;
                half _UpBias;
            CBUFFER_END

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
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    v += a * noise(p);
                    p = p * 2.02 + float2(17.1, 9.7);
                    a *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 viewDir = normalize(i.positionWS - _WorldSpaceCameraPos);
                // Bias upward so window never shows "earth", only bright sky.
                viewDir = normalize(viewDir + float3(0, _UpBias, 0));

                float y = saturate((viewDir.y + _HorizonLift) / (1.0 + _HorizonLift));
                half3 sky = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(y, 0.85));
                sky *= _SkyTint.rgb;

                float2 cloudUv = viewDir.xz / max(0.15, abs(viewDir.y) + 0.35);
                cloudUv *= _CloudScale;
                float clouds = fbm(cloudUv + float2(_Time.y * 0.01, _Time.y * 0.006));
                clouds = smoothstep(0.45 - _CloudSoftness, 0.45 + _CloudSoftness, clouds * _CloudDensity);
                clouds *= smoothstep(0.0, 0.2, y);

                sky = lerp(sky, _CloudColor.rgb, clouds * 0.9);
                sky *= _Exposure;
                return half4(sky, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
