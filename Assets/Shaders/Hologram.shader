Shader "Unlit/Hologram"
{
   Properties
    {
        //User exposed params
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 1, 1, 1)   // cyan hologram
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 4
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _ScanSpeed ("Scanline Speed", Range(0, 10)) = 5
        _ScanDensity ("Scanline Density", Range(1, 200)) = 80
        _WobbleStrength ("Wobble Strength", Range(0, 0.2)) = 0.05
        _WobbleSpeed ("Wobble Speed", Range(0, 10)) = 3
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" } //Draw after opaque objects
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off //Unlit
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc" // Unity’s helper functions (like UnityObjectToClipPos)

            sampler2D _MainTex;
            float4 _Color;
            float _FresnelPower;// controls edge glow strength
            float _Transparency;
            float _ScanSpeed;
            float _ScanDensity;
            float _WobbleStrength;
            float _WobbleSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;

                // vertex wobble
                v.vertex.y += sin(_Time.y * _WobbleSpeed + v.vertex.x * 2.0) * _WobbleStrength;
                v.vertex.x += cos(_Time.y * (_WobbleSpeed * 0.5) + v.vertex.z * 2.0) * _WobbleStrength;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // base texture
                fixed4 tex = tex2D(_MainTex, i.uv);

                // fresnel
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - saturate(dot(i.normalDir, viewDir)), _FresnelPower);

                // scanlines
                float scan = sin(i.uv.y * _ScanDensity + _Time.y * _ScanSpeed) * 0.5 + 0.5;

                // final color
                fixed4 col = tex * _Color;
                col.rgb += fresnel;       // add fresnel glow
                col.rgb *= scan;          // multiply by scanline brightness
                col.a = _Transparency;    // transparency

                return col;
            }
            ENDCG
        }
    }
}

