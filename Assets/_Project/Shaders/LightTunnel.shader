// Doodled "tunnel of light" for the dream gate, in the game's marker-on-white style: a double
// outline around a door-shaped spiral that winds into a big, glowing white middle and turns
// slowly while it rushes outwards. The marker lines get thinner with depth and wobble slightly,
// redrawn a few times a second.
// Each face can be closed (_ClosedFront / _ClosedBack, set by DreamGate): a closed face keeps the
// spiral moving, but slowly winding back inwards, grayed out and with no light - so the player
// can tell it won't let them in. The quad's front face looks towards -Z, the gate's entry side.
Shader "ReadOfMask/LightTunnel"
{
    Properties
    {
        [Header(Light)]
        [HDR] _CoreColor ("Light Color", Color) = (1, 1, 1, 1)
        _WallColor ("Paper Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 4)) = 1
        _WallLight ("Paper Light", Range(0, 4)) = 1.8
        _CoreSize ("Core Size", Range(0.02, 1)) = 0.4
        _CoreGlow ("Core Glow", Range(0, 12)) = 8
        _Pulse ("Pulse", Range(0, 1)) = 0.1
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 0.6

        [Header(Doodle)]
        _InkColor ("Marker Color", Color) = (0.42, 0.42, 0.42, 1)
        _LineWidth ("Line Width", Range(0.002, 0.06)) = 0.024
        _SpiralArms ("Spiral Arms", Range(1, 6)) = 2
        _SpiralRoundness ("Spiral Roundness (2 = round)", Range(2, 16)) = 2.4
        _RingDensity ("Spiral Tightness", Range(0.5, 8)) = 4.5
        _RingSpeed ("Rush Speed", Range(-5, 5)) = 1.1
        _Spin ("Spin Speed", Range(-2, 2)) = 0.2
        _Spokes ("Curved Spokes", Range(0, 12)) = 0
        _Twist ("Spoke Twist", Range(0, 2)) = 0.35
        _Outline ("Outline", Range(0, 1)) = 1
        _ClearMiddle ("White Middle Size", Range(0, 0.9)) = 0.4
        _Wobble ("Wobble", Range(0, 0.05)) = 0.008
        _BoilFps ("Redraws Per Second", Range(0, 24)) = 4
        _Roundness ("Corner Sharpness", Range(2, 48)) = 32

        [Header(Closed)]
        _ClosedGray ("Closed Paper Gray", Range(0, 1)) = 0.17
        _ClosedInk ("Closed Marker Gray", Range(0, 1)) = 0.07
        _ClosedSpeed ("Closed Drift Speed", Range(-3, 3)) = -0.35
        [HideInInspector] _ClosedFront ("Closed Front", Range(0, 1)) = 0
        [HideInInspector] _ClosedBack ("Closed Back", Range(0, 1)) = 0
    }

    SubShader
    {
        // drawn before other see-through things and writes depth, so the tunnel hides what's behind
        // it (wall writing, labels) instead of having them show through
        Tags { "Queue" = "Transparent-10" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One OneMinusSrcAlpha
        ZWrite On
        Cull Off

        Pass
        {
            Name "LightTunnel"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _CoreColor;
                half4 _WallColor;
                half _Intensity;
                half _WallLight;
                half _CoreSize;
                half _CoreGlow;
                half _Pulse;
                half _PulseSpeed;
                half4 _InkColor;
                half _LineWidth;
                half _SpiralArms;
                half _SpiralRoundness;
                half _RingDensity;
                half _RingSpeed;
                half _Spin;
                half _Spokes;
                half _Twist;
                half _Outline;
                half _ClearMiddle;
                half _Wobble;
                half _BoilFps;
                half _Roundness;
                half _ClosedGray;
                half _ClosedInk;
                half _ClosedSpeed;
                half _ClosedFront;
                half _ClosedBack;
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
                float fogFactor : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x),
                            lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
            }

            // marker line: full ink within half the width of the line, antialiased over a pixel
            float Ink(float distance, float width, float aa)
            {
                return 1.0 - smoothstep(width * 0.5 - aa, width * 0.5 + aa, distance);
            }

            float DoorDistance(float2 q, float n)
            {
                return min(pow(pow(abs(q.x), n) + pow(abs(q.y), n), 1.0 / n), 1.0);
            }

            half4 Frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float t = _Time.y;
                float closed = saturate(isFrontFace ? _ClosedFront : _ClosedBack);

                float2 q = (input.uv - 0.5) * 2.0;           // -1..1 across the doorway
                float d = DoorDistance(q, _Roundness);

                // drawn by hand: lines wander slightly, and get redrawn a few times a second
                float boil = _BoilFps > 0.0 ? floor(t * _BoilFps) : 0.0;
                float2 wobble = float2(ValueNoise(q * 1.7 + boil * 7.31), ValueNoise(q * 1.7 + boil * 3.17 + 17.0)) - 0.5;
                float2 qs = q + wobble * _Wobble;
                float ds = DoorDistance(qs, _Roundness);
                float aa = max(fwidth(ds), 1e-4);

                // the spiral has its own, rounder shape, so it reads as a spiral and not as nested
                // squares; it carries on into the doorway's square corners
                float n2 = _SpiralRoundness;
                float sp = pow(pow(abs(qs.x), n2) + pow(abs(qs.y), n2), 1.0 / n2);
                float spAA = max(fwidth(sp), 1e-4);

                // lines further into the tunnel are thinner, as if further away
                float width = _LineWidth * lerp(0.55, 1.0, saturate(sp));

                // --- spiral winding into the light: rushes out and turns when open,
                // winds slowly back in when closed
                float speed = lerp(_RingSpeed, _ClosedSpeed, closed);
                float spin = lerp(_Spin, -_Spin * 0.4, closed);
                float turn = atan2(qs.y, qs.x) / 6.2831853 + t * spin;   // 0..1 around the door
                float depth = 1.0 / max(sp, 0.02);
                float phase = depth * _RingDensity + turn * _SpiralArms - t * speed;
                float nearestDepth = (round(phase) - turn * _SpiralArms + t * speed) / _RingDensity;
                float spiral = Ink(abs(sp - 1.0 / max(nearestDepth, 0.02)), width, spAA);
                // turns closer together than the line itself merge into a blot: let them go
                // (measured on depth alone - the angle jumps where atan2 wraps)
                spiral *= saturate(1.0 - fwidth(depth * _RingDensity) * 2.5);

                // --- optional curved spokes, twisting with depth like a vortex
                float spokes = 0.0;
                if (_Spokes > 0.5)
                {
                    float count = floor(_Spokes);
                    float spokePhase = (turn - depth * _Twist * 0.1) * count;
                    float spokeDist = abs(spokePhase - round(spokePhase)) / count * 6.2831853 * ds;
                    spokes = Ink(spokeDist, width, max(fwidth(spokeDist), 1e-4));
                }

                // --- double outline along the doorway's edge
                float outline = (Ink(abs(d - 0.975), _LineWidth * 2.2, aa) + Ink(abs(ds - 0.92), _LineWidth * 0.9, aa)) * _Outline;

                // the drawing inside the outline gets lost in the big white light in the middle
                float lost = smoothstep(_ClearMiddle * 0.55, _ClearMiddle, sp);
                float inside = 1.0 - smoothstep(0.89, 0.91, d);
                float ink = saturate(saturate(spiral + spokes) * lost * inside + outline);

                // --- open: glowing white paper lit from the core, light marker over it
                float nearCore = saturate(1.0 - d);
                float core = exp(-pow(d / _CoreSize, 2.0));
                float pulse = 1.0 + _Pulse * sin(t * _PulseSpeed * 6.2831853);
                float3 paper = _WallColor.rgb * _WallLight * (0.9 + 0.35 * nearCore);
                float3 openColor = lerp(paper, _InkColor.rgb, ink) + _CoreColor.rgb * core * _CoreGlow * pulse * (1.0 - ink);
                openColor *= _Intensity;

                // --- closed: the same doodle, grayed out, no light
                float frost = ValueNoise(q * 3.0 + t * 0.04) * 0.6 + ValueNoise(q * 9.0 - t * 0.02) * 0.4;
                float closedPaper = _ClosedGray * (0.85 + 0.3 * frost);
                float closedLost = smoothstep(0.04, 0.3, sp);
                float closedInk = saturate(saturate(spiral + spokes) * closedLost * inside + outline);
                float3 closedColor = lerp(closedPaper.xxx, _ClosedInk.xxx, closedInk * 0.8);

                half3 color = lerp(openColor, closedColor, closed);

                // solid right to the corners: nothing behind the doorway shows through
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
