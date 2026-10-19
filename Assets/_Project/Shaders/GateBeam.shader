// Soft additive light spilling out of the dream gate: a widening, fading shaft from the doorway
// whose floor face is a pool of light on the ground. Mesh UVs: x across a face, y along the
// shaft (0 at the doorway, 1 at its far end); vertex alpha sets each face's strength.
Shader "ReadOfMask/GateBeam"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 4)) = 0.5
        _Falloff ("Length Falloff", Range(0.5, 8)) = 2.2
        _EdgeSoftness ("Edge Softness", Range(0.1, 4)) = 1.2
        _Shimmer ("Shimmer", Range(0, 1)) = 0.25
        _ShimmerSpeed ("Shimmer Speed", Range(0, 4)) = 0.35
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "GateBeam"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _Falloff;
                half _EdgeSoftness;
                half _Shimmer;
                half _ShimmerSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float along = pow(saturate(1.0 - input.uv.y), _Falloff);
                float across = pow(saturate(sin(input.uv.x * PI)), _EdgeSoftness);
                // fade faces seen edge-on, so the shaft has no hard outline
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                float facing = saturate(abs(dot(normalize(input.normalWS), view)) * 1.6);
                float shimmer = 1.0 - _Shimmer * (0.5 + 0.5 * sin(input.uv.x * 23.0 + _Time.y * _ShimmerSpeed * 6.2831853)
                                                       * sin(input.uv.x * 9.0 - _Time.y * _ShimmerSpeed * 4.0));
                float light = along * across * facing * shimmer * input.color.a * _Intensity;
                return half4(_Color.rgb * light, 0);
            }
            ENDHLSL
        }
    }
}
