using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

/// <summary>
/// Optimizes the menu scene for better performance
/// Disables expensive features that aren't needed for a menu
/// </summary>
public class MenuSceneOptimizer : MonoBehaviour
{
    [Header("Optimization Settings")]
    [SerializeField] private bool disableShadows = true;
    [SerializeField] private bool reduceLightQuality = true;
    [SerializeField] private bool optimizeRendering = true;
    
    void Start()
    {
        OptimizeScene();
    }
    
    private void OptimizeScene()
    {
        // Disable shadows on all lights for menu scene
        if (disableShadows)
        {
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.shadows = LightShadows.None;
                }
            }
        }
        
        // Reduce light quality settings
        if (reduceLightQuality)
        {
            // Note: Shadow resolution is now controlled by URP Asset settings
            // Individual light shadow resolution API has changed in newer Unity versions
            // Since we're disabling shadows above, this section is mainly for future extensibility
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light != null && light.type == LightType.Directional)
                {
                    // Reduce light intensity slightly for menu (optional optimization)
                    // light.intensity = Mathf.Min(light.intensity, 1.0f);
                }
            }
        }
        
        // Optimize rendering pipeline
        if (optimizeRendering)
        {
            // Get URP asset and optimize settings
            var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                // These settings are typically set in the URP asset itself,
                // but we can log a message for the developer
                Debug.Log("Menu Scene Optimizer: Consider reducing URP quality settings in the asset for menu scenes");
            }
        }
        
        // Disable unnecessary components on XR Origin if present
        OptimizeXROrigin();
    }
    
    private void OptimizeXROrigin()
    {
        // Find XR Origin and disable expensive features
        GameObject xrOrigin = GameObject.Find("XR Origin Hands (XR Rig)");
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        }
        
        if (xrOrigin != null)
        {
            // Disable hand tracking if not needed for menu
            // This can significantly improve performance
            try
            {
                // Try to find XR Hand Visualizers (namespace may vary)
                var handVisualizers = xrOrigin.GetComponentsInChildren<MonoBehaviour>()
                    .Where(c => c.GetType().Name.Contains("HandVisualizer") || 
                                c.GetType().Name.Contains("XRHandVisualizer"))
                    .ToArray();
                
                foreach (var visualizer in handVisualizers)
                {
                    if (visualizer != null)
                    {
                        visualizer.enabled = false;
                    }
                }
                
                if (handVisualizers.Length > 0)
                {
                    Debug.Log($"Menu Scene Optimizer: Disabled {handVisualizers.Length} hand visualizer(s) for better performance");
                }
            }
            catch (System.Exception e)
            {
                // Silently fail if hand visualizers aren't available
                Debug.Log($"Menu Scene Optimizer: Could not disable hand visualizers: {e.Message}");
            }
        }
    }
    
    void OnDestroy()
    {
        // Re-enable features if needed when leaving menu
        // This is optional - usually you want to keep optimizations
    }
}
