Shader "Custom/LaserBeam"
{
    Properties
    {
        _MainTex("Noise Texture (optional)", 2D) = "white" {}
        _Color("Base Color", Color) = (0.0, 0.6, 1.0, 1.0)
        _CoreColor("Core Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Intensity("Intensity", Float) = 3.0
        _BeamWidth("Beam Width (0..1)", Range(0.01, 1.0)) = 0.25
        _Softness("Edge Softness", Range(0.0, 1.0)) = 0.5
        _NoiseStrength("Noise Strength", Range(0.0, 2.0)) = 0.6
        _NoiseScale("Noise Scale", Float) = 3.0
        _NoiseSpeed("Noise Scroll Speed", Float) = 1.0
        _PulseSpeed("Pulse Speed", Float) = 2.0
        _PulseWidth("Pulse Width", Range(0.01,1.0)) = 0.15
        _GlowFalloff("Glow Falloff", Float) = 2.0
        _Alpha("Global Alpha", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        // Additive blend for glow-like effect
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float4 _CoreColor;
            float _Intensity;
            float _BeamWidth;
            float _Softness;
            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseSpeed;
            float _PulseSpeed;
            float _PulseWidth;
            float _GlowFalloff;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // uv.x along beam length, uv.y across width
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Simple 2D pseudo-random based on UV
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Fractional Brownian Motion - simple
            float fbm(float2 p)
            {
                float f = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 4; ++i)
                {
                    f += amp * hash21(p);
                    p *= 2.0;
                    amp *= 0.5;
                }
                return f;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV layout convention:
                // i.uv.x = 0..1 along the beam length (0 = origin, 1 = tip)
                // i.uv.y = 0..1 across the beam width (0 = bottom edge, 1 = top edge),
                // center line is at 0.5

                float2 uv = i.uv;

                // center across width
                float across = abs(uv.y - 0.5) / 0.5; // 0 at center, 1 at edge

                // Core falloff: sharper near center
                float core = pow(saturate(1.0 - across / max(0.0001, _BeamWidth)), 2.0);
                // soften edges
                float edge = smoothstep(_BeamWidth, _BeamWidth * (1.0 + _Softness), across);

                // Pulse traveling along beam (moving brighter band)
                float time = _Time.y;
                float pulsePos = frac(time * _PulseSpeed); // moves along uv.x
                float pulse = smoothstep(0.0, _PulseWidth, _PulseWidth - abs(frac(uv.x - pulsePos + 0.5) - 0.5));
                // alternative smoother pulse:
                pulse = pow(pulse, 1.5);

                // Noise: sample texture if provided (use uv scaled along length)
                float2 noiseUV = float2(uv.x * _NoiseScale, uv.y * _NoiseScale);
                noiseUV.x += time * _NoiseSpeed;
                float noise = tex2D(_MainTex, noiseUV).r; // assuming a grayscale noise texture
                // if no texture assigned, combine fbm fallback
                #if UNITY_NO_DXT5nm // not really detecting texture presence; fallback anyway
                // unreachable macro - kept for clarity
                #endif
                // combine sampled noise and procedural fallback
                float procNoise = fbm(noiseUV * 0.7);
                noise = lerp(procNoise, noise, 0.6);

                // core intensity shaped by pulse and noise
                float coreIntensity = core * (1.0 + pulse * 1.6) * (1.0 + _NoiseStrength * (noise - 0.5));

                // Glow: broader, softer band
                float glow = pow(saturate(1.0 - across / (_BeamWidth * 2.5)), _GlowFalloff);

                // Brightness falloff along the beam (optional: make beam fade toward tip)
                float lengthFalloff = smoothstep(1.0, 0.0, uv.x); // brighter near origin

                // Color composition: bright core + colored glow
                float3 coreCol = _CoreColor.rgb * coreIntensity * _Intensity;
                float3 colorCol = _Color.rgb * (glow * 0.8 + coreIntensity * 0.2) * _Intensity;

                float3 finalColor = coreCol + colorCol;
                float alpha = saturate((coreIntensity * 0.6 + glow * 0.5) * _Alpha * lengthFalloff);

                // small highlight along beam center for extra sharpness
                float centerLine = smoothstep(0.0, _BeamWidth * 0.4, 1.0 - across);
                finalColor += _CoreColor.rgb * centerLine * 0.6 * pulse;

                // Final tone mapping clamp
                finalColor = finalColor * (1.0); // leave as-is (additive blending will bloom)

                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
