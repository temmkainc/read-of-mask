Shader "Custom/EyeReveal"
{
    Properties
    {
        _Progress ("Progress", Range(0, 1)) = 0
        _Softness ("Edge Softness", Range(0.001, 0.1)) = 0.02
        _AspectRatio ("Aspect Ratio", Float) = 1.777
        _Color ("Overlay Color", Color) = (0, 0, 0, 1)    // ← color added
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            float  _Progress;
            float  _Softness;
            float  _AspectRatio;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            float eyeSDF(float2 uv, float openAmount)
            {
                uv = uv * 2.0 - 1.0;
                uv.x *= _AspectRatio;

                float scale = lerp(0.001, 2.5, openAmount);
                uv.y /= scale;

                float r = 1.6;
                float d1 = length(uv - float2(0,  r - 1.0)) - r;
                float d2 = length(uv - float2(0, -(r - 1.0))) - r;
                return max(d1, d2);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float d = eyeSDF(i.uv, _Progress);

                float alpha = smoothstep(-_Softness, _Softness, d);
                alpha *= (1.0 - smoothstep(0.85, 1.0, _Progress));

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}