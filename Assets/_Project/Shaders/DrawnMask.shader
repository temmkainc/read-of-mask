// The school kids' masks: a drawn mask (black marker line on white, clear outside) on a card over
// the face, unlit like paper. The line "boils" - jumps a hair every few frames, like a hand-drawn
// animation redrawn each frame.
Shader "ReadOfMask/DrawnMask"
{
    Properties
    {
        _MainTex ("Drawing", 2D) = "white" {}
        _Color ("Paper", Color) = (1, 1, 1, 1)
        _Cutoff ("Cutoff", Range(0, 1)) = 0.5
        _Boil ("Line Boil", Range(0, 0.02)) = 0.004
        _BoilRate ("Boil Rate (per second)", Float) = 8
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            half _Cutoff;
            float _Boil;
            float _BoilRate;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float seed : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float Hash(float n)
        {
            return frac(sin(n * 12.9898) * 43758.5453);
        }

        Varyings Vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = TRANSFORM_TEX(input.uv, _MainTex);
            // each mask boils out of step with the others, from where it stands
            float3 origin = TransformObjectToWorld(float3(0, 0, 0));
            output.seed = floor(origin.x) * 7.13 + floor(origin.z) * 3.71;
            return output;
        }

        half4 Drawing(Varyings input)
        {
            // the line jumps to a new spot BoilRate times a second
            float frame = floor(_Time.y * _BoilRate) + input.seed;
            float2 jitter = float2(Hash(frame), Hash(frame + 17.31)) - 0.5;
            half4 drawing = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + jitter * _Boil);
            clip(drawing.a - _Cutoff);
            return drawing;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            half4 Frag(Varyings input) : SV_Target
            {
                half4 drawing = Drawing(input);
                half3 color = MixFog(drawing.rgb * _Color.rgb, ComputeFogFactor(input.positionCS.z));
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half Frag(Varyings input) : SV_Target
            {
                Drawing(input);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}
