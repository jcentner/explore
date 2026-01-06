// SH_DistantObject.shader
// Unlit shader for asteroids, spacecraft, and small moons
// Uses phase angle or per-pixel terminator based on toggle

Shader "Explorer/SH_DistantObject"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseMap ("Base Map (RGB)", 2D) = "white" {}
        
        [Toggle(_USE_PHASE_ANGLE)] _UsePhaseAngle ("Use Phase Angle (vs Per-Pixel)", Float) = 1
        _TerminatorSoftness ("Terminator Softness", Range(0, 0.5)) = 0.05
        
        [Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows ("Receive Shadows", Float) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature_local _USE_PHASE_ANGLE
            #pragma shader_feature_local _RECEIVE_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/_Project/Shaders/Include/DistantLighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _TerminatorSoftness;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);
                
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample base texture
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                
                // Calculate lighting
                float3 lighting;
                
                #ifdef _USE_PHASE_ANGLE
                    // Phase angle mode: single brightness for whole object
                    // Use object center (approximated by averaging - in practice, pass via material or compute)
                    // For now, use the fragment position as a reasonable approximation
                    float brightness = CalculatePhaseAngle(IN.positionWS, _WorldSpaceCameraPos);
                    
                    // Apply eclipse shadows (always enabled)
                    brightness *= CalculateShadow(IN.positionWS);
                    
                    float3 litColor = _SunColor.rgb * _SunIntensity;
                    float3 ambientColor = _SpaceAmbient.rgb;
                    lighting = lerp(ambientColor, litColor, brightness);
                #else
                    // Per-pixel terminator mode
                    float3 normalWS = normalize(IN.normalWS);
                    float illumination = CalculateSoftTerminator(IN.positionWS, normalWS, _TerminatorSoftness);
                    
                    // Apply eclipse shadows (always enabled)
                    illumination *= CalculateShadow(IN.positionWS);
                    
                    float3 litColor = _SunColor.rgb * _SunIntensity;
                    float3 ambientColor = _SpaceAmbient.rgb;
                    lighting = lerp(ambientColor, litColor, illumination);
                #endif
                
                // Final color
                half3 finalColor = albedo * lighting;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        // Shadow caster pass (so object can cast real-time shadows when close)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            float3 _LightDirection;
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return positionCS;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth only pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
