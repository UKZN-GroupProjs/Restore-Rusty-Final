Shader "Custom/RainbowPulsingGlow"
{
    Properties
    {
        _GlowStrength("Glow Strength", Range(0.0, 10.0)) = 5.0
        _PulseSpeed("Pulse Speed", Range(0.0, 10.0)) = 2.0
        _HueSpeed("Hue Shift Speed", Range(0.0, 5.0)) = 0.5
        _Alpha("Transparency", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        Blend SrcAlpha One      // additive blending, lets base material show
        ZWrite Off
        Lighting Off
        Cull Back

        Pass
        {
            Name "GLOW"
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
                float4 positionHCS : SV_POSITION;
            };

            float _GlowStrength;
            float _PulseSpeed;
            float _HueSpeed;
            float _Alpha;

            // HSV to RGB conversion
            float3 HSVtoRGB(float3 hsv)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
                return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Pulsing factor (0.5 to 1.0)
                float pulse = abs(sin(_Time.y * _PulseSpeed)) * 0.5 + 0.5;

                // Hue cycling over time
                float hue = frac(_Time.y * _HueSpeed);
                float3 rgb = HSVtoRGB(float3(hue, 1.0, 1.0));

                float3 glow = rgb * _GlowStrength * pulse;
                return float4(glow, _Alpha); // semi-transparent
            }
            ENDHLSL
        }
    }
    FallBack Off
}



