Shader "Hidden/Shift25/BrokenWorldShader"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Source", 2D) = "white" {}
        _RedAmount("Red Intensity", Range(0, 1)) = 0.0 
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

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            // [Modern URP API] Binding the camera texture
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float _RedAmount;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // [Algorithm] Procedural full-screen triangle generation for efficiency
                OUT.pos = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            // [Algorithm] High-Visibility 1-bit Red Shading Logic
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the original scene frame
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, IN.uv);
                
                // Calculate scene luminance (Standard perception weights)
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // Color Palette Definitions
                half3 black = half3(0.02, 0.0, 0.0); // Deep void black
                half3 red   = half3(0.85, 0.05, 0.05); // Menacing blood red
                half3 white = half3(1.0, 1.0, 1.0); // Pure stark white

                // [Algorithm: Color Keying] Detect Pure Green (R < 0.2, G > 0.8, B < 0.2)
                // This is used to isolate our Interaction Outlines from the rest of the world.
                bool isOutline = (col.g > 0.7 && col.r < 0.4 && col.b < 0.4);
                
                half3 finalBrokenColor;

                if (isOutline)
                {
                    // [Juice] Add a procedural flicker effect to the outline for extra visibility
                    float flicker = sin(_Time.y * 20.0) * 0.5 + 0.5;
                    finalBrokenColor = lerp(white, red, flicker * 0.5);
                }
                else 
                {
                    // standard 1-bit Red mapping based on luminance
                    if (lum > 0.75) finalBrokenColor = white;
                    else if (lum > 0.25) finalBrokenColor = red;
                    else finalBrokenColor = black;
                }

                // [Algorithm] Linear interpolation between the PSX world and the Broken world
                // This allows the gradual 'bleeding' effect from 70% pressure.
                return half4(lerp(col.rgb, finalBrokenColor, _RedAmount), 1.0);
            }
            ENDHLSL
        }
    }
}