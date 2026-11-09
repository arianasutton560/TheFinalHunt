
Shader "Custom/VideoBlur"
{
    Properties
    {
        _MainTex ("Video Texture", 2D) = "white" {}
        _BlurSize ("Blur Strength", Range(0, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            ZWrite Off
            ZTest [unity_GUIZTestMode]
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float _BlurSize;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _BlurSize * 3;

                half4 color = 0;
                float total = 0;

                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        float2 sampleUV = input.uv +
                            float2(x, y) * offset;

                        sampleUV = clamp(
                            sampleUV,
                            offset * 2.0,
                            1.0 - offset * 2.0
                        );

                        color += SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            sampleUV
                        );

                        total += 1;
                    }
                }

                return (color / total) * input.color;
            }

            ENDHLSL
        }
    }
}
