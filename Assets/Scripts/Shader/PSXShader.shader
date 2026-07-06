Shader "Hidden/Shift25/PSXShader"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Source Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _Res; 

            // [Algorithm] Pseudo-random function for noise generation
            float SimpleNoise(float2 uv, float time)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233) * time)) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // [Step 1] Pixelation Logic
                if(_Res.x > 0 && _Res.y > 0)
                {
                    uv = floor(IN.uv * _Res.xy) / _Res.xy;
                }

                // [Step 2] Sample the camera texture
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                
                // [Step 3] Subtle Color Quantization (32 steps for cleaner look but retro feel)
                // This creates very soft color bands compared to the previous 16 steps
                col.rgb = floor(col.rgb * 32.0) / 32.0;

                // [Step 4] Procedural Grain / Noise
                // We use _Time.y to make the noise move every frame like an old TV
                float noise = SimpleNoise(uv, _Time.y);
                float grainIntensity = 0.04; // Adjust this for more/less noise
                col.rgb += (noise - 0.5) * grainIntensity;
                
                // [Step 5] Slight Gamma Lift (Prevents crushed blacks)
                col.rgb = pow(abs(col.rgb), 0.95);

                return col;
            }
            ENDHLSL
        }
    }
}