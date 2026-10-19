// Soft additive glow for the dream gate. With a Hole set, it's a halo around a door-shaped
// opening (spilling onto the wall around the doorway); with no hole, a soft round dot
// (used for the particles drifting into the gate - tinted by vertex colour).
Shader "ReadOfMask/GateGlow"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 8)) = 1
        _Hole ("Hole (fraction of the quad)", Vector) = (0, 0, 0, 0)
        _Roundness ("Roundness", Range(2, 16)) = 2
        _Falloff ("Falloff", Range(0.5, 8)) = 2.5
        _Pulse ("Pulse", Range(0, 1)) = 0.15
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1.7
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "GateGlow"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                float4 _Hole;
                half _Roundness;
                half _Falloff;
                half _Pulse;
                half _PulseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 q = abs((input.uv - 0.5) * 2.0);
                float n = _Roundness;
                float glow;

                if (_Hole.x > 0.0 && _Hole.y > 0.0)
                {
                    // 0 at the opening's edge, 1 at the quad's edge, measured outwards
                    float2 h = _Hole.xy;
                    float dHole = pow(pow(abs(q.x / h.x), n) + pow(abs(q.y / h.y), n), 1.0 / n);
                    float dQuad = pow(pow(q.x, n) + pow(q.y, n), 1.0 / n);
                    float span = max(1.0 / max(h.x, h.y) - 1.0, 0.001);
                    float outward = saturate((dHole - 1.0) / span);
                    glow = pow(saturate(1.0 - outward), _Falloff) * smoothstep(0.97, 1.0, dHole) * (1.0 - smoothstep(0.85, 1.0, dQuad));
                }
                else
                {
                    float d = pow(pow(q.x, n) + pow(q.y, n), 1.0 / n);
                    glow = pow(saturate(1.0 - d), _Falloff);
                }

                float pulse = 1.0 + _Pulse * sin(_Time.y * _PulseSpeed * 6.2831853);
                half3 color = _Color.rgb * input.color.rgb * input.color.a * glow * _Intensity * pulse;
                return half4(color, 0);
            }
            ENDHLSL
        }
    }
}
