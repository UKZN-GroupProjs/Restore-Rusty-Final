Shader "Custom/GlowingWindow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (0.3, 0.7, 1.0, 1.0)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3.0
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.3
        _Transparency ("Transparency", Range(0, 1)) = 0.7
        _EdgeGlow ("Edge Glow Thickness", Range(0, 1)) = 0.2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _GlowColor;
            float _GlowIntensity;
            float _FresnelPower;
            float _PulseSpeed;
            float _PulseAmount;
            float _Transparency;
            float _EdgeGlow;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample base texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate Fresnel effect (glow at edges)
                float fresnel = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.worldNormal)));
                fresnel = pow(fresnel, _FresnelPower);
                
                // Add pulsing animation
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float glowMod = 1.0 + (pulse * _PulseAmount);
                
                // Edge detection for extra glow
                float edgeDist = min(min(i.uv.x, 1.0 - i.uv.x), min(i.uv.y, 1.0 - i.uv.y));
                float edgeGlow = smoothstep(0, _EdgeGlow, edgeDist);
                edgeGlow = 1.0 - edgeGlow;
                
                // Combine glow effects
                float totalGlow = (fresnel + edgeGlow * 0.5) * glowMod * _GlowIntensity;
                
                // Apply glow color
                float3 glowCol = _GlowColor.rgb * totalGlow;
                
                // Mix base color with glow
                col.rgb = lerp(col.rgb, glowCol, 0.5);
                col.rgb += glowCol * 0.5;
                
                // Apply transparency
                col.a = _Transparency + (fresnel * 0.3);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}