// Full-screen "moving between dreams" effect, drawn by DreamTransitionFeature.
// _DreamAmount: swirl + gaussian blur + zoom + chromatic split, _DreamWhite: white-out,
// _DreamSpin: rotation of the whole picture (radians). All set globally by DreamTransition.
Shader "Hidden/ReadOfMask/DreamTransition"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "DreamTransition"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _DreamAmount;
            float _DreamWhite;
            float _DreamSpin;

            #define TAPS 24

            // white-out floods from the middle outwards, fully white at 1
            float WhiteLead(float r)
            {
                float white = saturate(_DreamWhite);
                return saturate(white * 1.35 - r * 0.5 * (1.0 - white));
            }

            float2 Rotate(float2 v, float angle)
            {
                float s, c;
                sincos(angle, s, c);
                return float2(c * v.x - s * v.y, s * v.x + c * v.y);
            }

            // mirror out-of-screen samples back in, so the rotated picture never shows smeared edges
            float3 SampleMirrored(float2 uv)
            {
                uv = 1.0 - abs(1.0 - abs(uv));
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 toAspect = float2(aspect, 1.0);
                float2 c = (input.texcoord - 0.5) * toAspect;
                float r = length(c);
                float amount = saturate(_DreamAmount);

                float zoom = 1.0 + amount * 0.6;                       // pulled in towards the middle
                float twirl = amount * 2.6 * saturate(1.0 - r * 1.1);   // vortex: the middle turns further
                float blurRadius = amount * 0.035;                      // gaussian blur, in screen heights
                float smear = amount * 0.45;                            // motion blur along the spin
                float split = amount * 0.012;                           // split the channels, then drain the colour (stays black and white)

                float3 sum = 0;
                float weightSum = 0;

                [unroll]
                for (int i = 0; i < TAPS; i++)
                {
                    float t = (i + 0.5) / TAPS;
                    float s = t - 0.5;
                    float golden = i * 2.39996323;
                    float2 disk = float2(cos(golden), sin(golden)) * sqrt(t);
                    float weight = exp(-t * 2.2);                       // gaussian falloff over the disk

                    float2 p = Rotate(c, _DreamSpin + twirl + s * smear) / (zoom * (1.0 + s * amount * 0.12)) + disk * blurRadius;

                    float3 tap;
                    tap.r = SampleMirrored(p * (1.0 + split) / toAspect + 0.5).r;
                    tap.g = SampleMirrored(p / toAspect + 0.5).g;
                    tap.b = SampleMirrored(p * (1.0 - split) / toAspect + 0.5).b;

                    sum += tap * weight;
                    weightSum += weight;
                }

                float3 color = sum / weightSum;

                // heavenly: brighter, black-and-white only, with light pouring in from the middle
                float3 glow = float3(3.5, 3.5, 3.5);
                color *= 1.0 + amount * 1.2;
                color = lerp(color, dot(color, float3(0.299, 0.587, 0.114)).xxx, saturate(amount * 2.0));
                color += glow * amount * 0.55 * exp(-r * r * 5.0);

                // start the white flood here too, so it blooms; the WhiteOut pass finishes it
                color = lerp(color, glow, smoothstep(0.0, 1.0, WhiteLead(r)) * 0.6);

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Runs after post-processing, so the peak is pure white - no vignette, grain or tonemapping on it.
        Pass
        {
            Name "DreamWhiteOut"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _DreamWhite;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float r = length((input.texcoord - 0.5) * float2(aspect, 1.0));
                float3 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, input.texcoord, 0).rgb;

                float white = saturate(_DreamWhite);
                float lead = saturate(white * 1.35 - r * 0.5 * (1.0 - white));
                return half4(lerp(color, float3(1.0, 1.0, 1.0), smoothstep(0.0, 1.0, lead)), 1.0);
            }
            ENDHLSL
        }
    }
}
