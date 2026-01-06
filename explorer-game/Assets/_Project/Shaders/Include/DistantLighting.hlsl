// DistantLighting.hlsl
// Shared functions for solar system lighting calculations
// Used by SH_DistantPlanet and SH_DistantObject shaders

#ifndef DISTANT_LIGHTING_INCLUDED
#define DISTANT_LIGHTING_INCLUDED

// Global shader properties (set by SolarSystemLightingManager.cs)
float3 _SunPosition;
float4 _SunColor;
float _SunIntensity;
float4 _SpaceAmbient;

int _ShadowCasterCount;
float4 _ShadowCasterPositions[8];
float _ShadowCasterRadii[8];

// ============================================================================
// CORE LIGHTING FUNCTIONS
// ============================================================================

/// <summary>
/// Calculates basic sun illumination (0-1) based on surface normal.
/// Simple NdotL calculation - no softening.
/// </summary>
/// <param name="worldPos">World position of the fragment</param>
/// <param name="worldNormal">World normal of the surface</param>
/// <returns>Illumination value 0 (dark) to 1 (fully lit)</returns>
float CalculateSunIllumination(float3 worldPos, float3 worldNormal)
{
    float3 toSun = normalize(_SunPosition - worldPos);
    float NdotL = dot(worldNormal, toSun);
    return saturate(NdotL);
}

/// <summary>
/// Calculates soft terminator (day/night boundary) for planets.
/// Uses smoothstep for a gradual transition instead of hard edge.
/// </summary>
/// <param name="worldPos">World position of the fragment</param>
/// <param name="worldNormal">World normal of the surface</param>
/// <param name="softness">Softness of the terminator (0.0 = hard, 0.5 = very soft)</param>
/// <returns>Illumination value 0 (dark) to 1 (fully lit)</returns>
float CalculateSoftTerminator(float3 worldPos, float3 worldNormal, float softness)
{
    float3 toSun = normalize(_SunPosition - worldPos);
    float NdotL = dot(worldNormal, toSun);
    
    // Use smoothstep for soft transition centered at terminator
    // At softness=0.1: fully dark at NdotL=-0.1, fully lit at NdotL=0.1
    return smoothstep(-softness, softness, NdotL);
}

/// <summary>
/// Calculates phase angle brightness for small/distant objects.
/// Based on viewing angle relative to sun direction.
/// Good for asteroids and spacecraft that are too small for per-pixel terminator.
/// </summary>
/// <param name="objectPos">World position of the object center</param>
/// <param name="cameraPos">World position of the camera</param>
/// <returns>Brightness multiplier 0 (backlit) to 1 (frontlit)</returns>
float CalculatePhaseAngle(float3 objectPos, float3 cameraPos)
{
    float3 toSun = normalize(_SunPosition - objectPos);
    float3 toCamera = normalize(cameraPos - objectPos);
    
    // Phase angle: angle between sun direction and camera direction
    // cos(phase) = dot(toSun, toCamera)
    // At phase=0 (sun behind camera), object is fully lit
    // At phase=180 (sun in front of camera), object is backlit
    float cosPhase = dot(toSun, toCamera);
    
    // Remap from [-1, 1] to [0, 1] with some artistic adjustment
    // Keep some illumination even when backlit (rim light effect)
    float brightness = saturate(cosPhase * 0.5 + 0.5);
    
    // Add a small minimum to prevent complete darkness
    return max(brightness, 0.05);
}

/// <summary>
/// Calculates shadow from registered shadow casters (eclipses).
/// Uses simplified cylindrical shadow model.
/// </summary>
/// <param name="worldPos">World position of the fragment</param>
/// <returns>Shadow multiplier 0 (fully shadowed) to 1 (no shadow)</returns>
float CalculateShadow(float3 worldPos)
{
    float shadow = 1.0;
    
    float3 toSun = _SunPosition - worldPos;
    float distanceToSun = length(toSun);
    float3 sunDir = toSun / distanceToSun;
    
    for (int i = 0; i < _ShadowCasterCount && i < 8; i++)
    {
        float3 casterPos = _ShadowCasterPositions[i].xyz;
        float casterRadius = _ShadowCasterRadii[i];
        
        // Skip if caster has no radius
        if (casterRadius <= 0.0) continue;
        
        // Vector from fragment to caster
        float3 toCaster = casterPos - worldPos;
        
        // Project onto sun direction to check if caster is between us and sun
        float projectionOnSunDir = dot(toCaster, sunDir);
        
        // Skip if caster is behind us (not between us and sun)
        if (projectionOnSunDir < 0.0) continue;
        
        // Skip if caster is beyond the sun
        if (projectionOnSunDir > distanceToSun) continue;
        
        // Calculate perpendicular distance from the sun-ray
        float3 closestPointOnRay = worldPos + sunDir * projectionOnSunDir;
        float perpDistance = length(casterPos - closestPointOnRay);
        
        // If within caster radius, we're in shadow
        if (perpDistance < casterRadius)
        {
            // Soft edge for penumbra approximation
            float softEdge = saturate((casterRadius - perpDistance) / (casterRadius * 0.1));
            shadow = min(shadow, 1.0 - softEdge);
        }
    }
    
    return shadow;
}

/// <summary>
/// Combined distant lighting calculation with all features.
/// </summary>
/// <param name="worldPos">World position of the fragment</param>
/// <param name="worldNormal">World normal of the surface</param>
/// <param name="softness">Terminator softness (0.0 = hard, 0.5 = very soft)</param>
/// <returns>Final lighting color</returns>
float3 CalculateDistantLighting(float3 worldPos, float3 worldNormal, float softness)
{
    // Calculate terminator
    float illumination = CalculateSoftTerminator(worldPos, worldNormal, softness);
    
    // Apply shadow
    float shadow = CalculateShadow(worldPos);
    illumination *= shadow;
    
    // Blend between ambient and sun color based on illumination
    float3 litColor = _SunColor.rgb * _SunIntensity;
    float3 ambientColor = _SpaceAmbient.rgb;
    
    return lerp(ambientColor, litColor, illumination);
}

/// <summary>
/// Distant lighting with phase angle mode (for small objects).
/// </summary>
/// <param name="objectPos">World position of the object center</param>
/// <param name="cameraPos">World position of the camera</param>
/// <returns>Final lighting color based on phase angle</returns>
float3 CalculateDistantLightingPhaseAngle(float3 objectPos, float3 cameraPos)
{
    // Calculate phase angle brightness
    float brightness = CalculatePhaseAngle(objectPos, cameraPos);
    
    // Apply shadow
    float shadow = CalculateShadow(objectPos);
    brightness *= shadow;
    
    // Blend between ambient and sun color
    float3 litColor = _SunColor.rgb * _SunIntensity;
    float3 ambientColor = _SpaceAmbient.rgb;
    
    return lerp(ambientColor, litColor, brightness);
}

#endif // DISTANT_LIGHTING_INCLUDED
