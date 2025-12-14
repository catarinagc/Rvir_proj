using UnityEngine;

/// <summary>
/// Snaps all trees to the terrain height to fix floating trees.
/// Can be run at Start or manually via the SnapTreesToTerrain method.
/// </summary>
public class TreeTerrainSnapper : MonoBehaviour
{
    [Header("Tree Settings")]
    [Tooltip("Name pattern to identify tree GameObjects (e.g., 'trees-new-try'). Leave empty to snap all objects with 'tree' in name.")]
    public string treeNamePattern = "trees-new-try";
    
    [Tooltip("Layer mask for terrain objects. Set this to the layer your terrain is on.")]
    public LayerMask terrainLayer = -1;
    
    [Tooltip("Maximum distance to search for terrain below trees.")]
    public float maxRaycastDistance = 100f;
    
    [Tooltip("Offset from terrain surface (useful if trees have pivot at bottom).")]
    public float terrainOffset = 0f;
    
    [Tooltip("If true, automatically snap trees when the scene starts.")]
    public bool snapOnStart = true;
    
    [Tooltip("If true, only snap trees that are currently floating (not touching terrain).")]
    public bool onlyFixFloating = true;
    
    [Tooltip("Distance threshold to consider a tree as floating.")]
    public float floatingThreshold = 0.5f;

    void Start()
    {
        if (snapOnStart)
        {
            SnapTreesToTerrain();
        }
    }

    /// <summary>
    /// Snaps all trees matching the name pattern to the terrain height.
    /// </summary>
    public void SnapTreesToTerrain()
    {
        int treesFixed = 0;
        int treesSkipped = 0;
        
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if this object matches our tree name pattern
            if (string.IsNullOrEmpty(treeNamePattern) || obj.name.Contains(treeNamePattern))
            {
                // Check if it's already on the ground (if onlyFixFloating is true)
                if (onlyFixFloating)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(obj.transform.position, Vector3.down, out hit, floatingThreshold, terrainLayer))
                    {
                        treesSkipped++;
                        continue; // Tree is already on the ground
                    }
                }
                
                // Raycast down to find terrain
                RaycastHit terrainHit;
                if (Physics.Raycast(obj.transform.position, Vector3.down, out terrainHit, maxRaycastDistance, terrainLayer))
                {
                    // Get the bottom of the tree (assuming pivot might be at center or top)
                    // We'll use the hit point and add the offset
                    Vector3 newPosition = obj.transform.position;
                    newPosition.y = terrainHit.point.y + terrainOffset;
                    obj.transform.position = newPosition;
                    
                    treesFixed++;
                    Debug.Log($"Snapped tree '{obj.name}' to terrain at Y: {newPosition.y}");
                }
                else
                {
                    Debug.LogWarning($"Could not find terrain below tree '{obj.name}' at position {obj.transform.position}");
                }
            }
        }
        
        Debug.Log($"Tree snapping complete! Fixed: {treesFixed}, Skipped: {treesSkipped}");
    }

    /// <summary>
    /// Alternative method using Terrain.SampleHeight for more accurate terrain height.
    /// This is more accurate but requires finding the Terrain component.
    /// </summary>
    public void SnapTreesToTerrainUsingTerrainComponent()
    {
        // Find all Terrain components in the scene
        Terrain[] terrains = FindObjectsOfType<Terrain>();
        
        if (terrains.Length == 0)
        {
            Debug.LogWarning("No Terrain components found in scene. Using raycast method instead.");
            SnapTreesToTerrain();
            return;
        }
        
        int treesFixed = 0;
        int treesSkipped = 0;
        
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if this object matches our tree name pattern
            if (string.IsNullOrEmpty(treeNamePattern) || obj.name.Contains(treeNamePattern))
            {
                // Find which terrain this tree is over
                Terrain targetTerrain = null;
                float minDistance = float.MaxValue;
                
                foreach (Terrain terrain in terrains)
                {
                    Vector3 terrainPos = terrain.transform.position;
                    Vector3 treePos = obj.transform.position;
                    
                    // Check if tree is within terrain bounds
                    TerrainData tData = terrain.terrainData;
                    if (treePos.x >= terrainPos.x && treePos.x <= terrainPos.x + tData.size.x &&
                        treePos.z >= terrainPos.z && treePos.z <= terrainPos.z + tData.size.z)
                    {
                        float distance = Vector3.Distance(treePos, terrainPos);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            targetTerrain = terrain;
                        }
                    }
                }
                
                if (targetTerrain != null)
                {
                    // Get terrain height at tree's X,Z position
                    float terrainHeight = targetTerrain.SampleHeight(obj.transform.position);
                    
                    // Check if it's already on the ground (if onlyFixFloating is true)
                    if (onlyFixFloating)
                    {
                        float currentHeight = obj.transform.position.y;
                        if (Mathf.Abs(currentHeight - terrainHeight) < floatingThreshold)
                        {
                            treesSkipped++;
                            continue; // Tree is already on the ground
                        }
                    }
                    
                    // Set tree Y position to terrain height + offset
                    Vector3 newPosition = obj.transform.position;
                    newPosition.y = terrainHeight + terrainOffset;
                    obj.transform.position = newPosition;
                    
                    treesFixed++;
                    Debug.Log($"Snapped tree '{obj.name}' to terrain at Y: {newPosition.y}");
                }
                else
                {
                    Debug.LogWarning($"Tree '{obj.name}' at {obj.transform.position} is not over any terrain. Using raycast method.");
                    // Fallback to raycast method
                    RaycastHit terrainHit;
                    if (Physics.Raycast(obj.transform.position, Vector3.down, out terrainHit, maxRaycastDistance, terrainLayer))
                    {
                        Vector3 newPosition = obj.transform.position;
                        newPosition.y = terrainHit.point.y + terrainOffset;
                        obj.transform.position = newPosition;
                        treesFixed++;
                    }
                }
            }
        }
        
        Debug.Log($"Tree snapping complete! Fixed: {treesFixed}, Skipped: {treesSkipped}");
    }
}
