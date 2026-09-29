// Full-screen ring distortion, run by a Full Screen Pass renderer feature on the 2D Renderer.
// Its values are globals set by ShockwaveEffect; with strength 0 the screen passes through untouched.
Shader "Hidden/Asteroids/Shockwave"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "Shockwave"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            // Viewport-space centre; the distances are fractions of the screen height.
            float2 _ShockwaveCenter;
            float _ShockwaveRadius;
            float _ShockwaveThickness;
            float _ShockwaveStrength;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                if (_ShockwaveStrength > 0.0)
                {
                    // Measured in screen-height units so the ring stays round on wide screens.
                    float2 aspect = float2(_ScreenParams.x / _ScreenParams.y, 1.0);
                    float2 offset = (uv - _ShockwaveCenter) * aspect;
                    float dist = length(offset);

                    // 1 on the ring, fading smoothly to 0 at the band's edges.
                    float band = saturate(1.0 - abs(dist - _ShockwaveRadius) / _ShockwaveThickness);
                    band = band * band * (3.0 - 2.0 * band);

                    // Sampling from nearer the centre makes the image bulge outward along the ring.
                    float2 direction = offset / max(dist, 1e-4);
                    uv -= direction * band * _ShockwaveStrength / aspect;
                }

                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
            }
            ENDHLSL
        }
    }
}
