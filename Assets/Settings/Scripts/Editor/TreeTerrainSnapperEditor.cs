using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to snap trees to terrain height from the Unity menu.
/// </summary>
public class TreeTerrainSnapperEditor : EditorWindow
{
    private string treeNamePattern = "trees-new-try";
    private LayerMask terrainLayer = -1;
    private float maxRaycastDistance = 100f;
    private float terrainOffset = 0f;
    private bool onlyFixFloating = true;
    private float floatingThreshold = 0.5f;
    private bool useTerrainComponent = true;

    [MenuItem("Tools/Snap Trees to Terrain")]
    public static void ShowWindow()
    {
        GetWindow<TreeTerrainSnapperEditor>("Snap Trees to Terrain");
    }

    void OnGUI()
    {
        GUILayout.Label("Tree Terrain Snapper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        treeNamePattern = EditorGUILayout.TextField("Tree Name Pattern", treeNamePattern);
        
        // Use MaskField for LayerMask selection
        string[] layerNames = new string[32];
        int[] layerValues = new int[32];
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(layerName))
                layerName = "Layer " + i;
            layerNames[i] = layerName;
            layerValues[i] = 1 << i;
        }
        int selectedLayerMask = EditorGUILayout.MaskField("Terrain Layer", terrainLayer.value, layerNames);
        terrainLayer = selectedLayerMask;
        
        maxRaycastDistance = EditorGUILayout.FloatField("Max Raycast Distance", maxRaycastDistance);
        terrainOffset = EditorGUILayout.FloatField("Terrain Offset", terrainOffset);
        onlyFixFloating = EditorGUILayout.Toggle("Only Fix Floating Trees", onlyFixFloating);
        floatingThreshold = EditorGUILayout.FloatField("Floating Threshold", floatingThreshold);
        useTerrainComponent = EditorGUILayout.Toggle("Use Terrain Component (More Accurate)", useTerrainComponent);

        EditorGUILayout.Space();

        if (GUILayout.Button("Snap All Trees to Terrain"))
        {
            if (useTerrainComponent)
            {
                SnapTreesUsingTerrainComponent();
            }
            else
            {
                SnapTreesUsingRaycast();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will modify the positions of all trees matching the name pattern in the scene. Make sure to save your scene after running this.", MessageType.Info);
    }

    private void SnapTreesUsingRaycast()
    {
        int treesFixed = 0;
        int treesSkipped = 0;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (string.IsNullOrEmpty(treeNamePattern) || obj.name.Contains(treeNamePattern))
            {
                if (onlyFixFloating)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(obj.transform.position, Vector3.down, out hit, floatingThreshold, terrainLayer))
                    {
                        treesSkipped++;
                        continue;
                    }
                }

                RaycastHit terrainHit;
                if (Physics.Raycast(obj.transform.position, Vector3.down, out terrainHit, maxRaycastDistance, terrainLayer))
                {
                    Undo.RecordObject(obj.transform, "Snap Tree to Terrain");
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
        EditorUtility.DisplayDialog("Tree Snapping Complete", 
            $"Fixed: {treesFixed} trees\nSkipped: {treesSkipped} trees", "OK");
    }

    private void SnapTreesUsingTerrainComponent()
    {
        Terrain[] terrains = FindObjectsOfType<Terrain>();

        if (terrains.Length == 0)
        {
            EditorUtility.DisplayDialog("No Terrain Found", 
                "No Terrain components found in scene. Using raycast method instead.", "OK");
            SnapTreesUsingRaycast();
            return;
        }

        int treesFixed = 0;
        int treesSkipped = 0;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (string.IsNullOrEmpty(treeNamePattern) || obj.name.Contains(treeNamePattern))
            {
                Terrain targetTerrain = null;
                float minDistance = float.MaxValue;

                foreach (Terrain terrain in terrains)
                {
                    Vector3 terrainPos = terrain.transform.position;
                    Vector3 treePos = obj.transform.position;

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
                    float terrainHeight = targetTerrain.SampleHeight(obj.transform.position);

                    if (onlyFixFloating)
                    {
                        float currentHeight = obj.transform.position.y;
                        if (Mathf.Abs(currentHeight - terrainHeight) < floatingThreshold)
                        {
                            treesSkipped++;
                            continue;
                        }
                    }

                    Undo.RecordObject(obj.transform, "Snap Tree to Terrain");
                    Vector3 newPosition = obj.transform.position;
                    newPosition.y = terrainHeight + terrainOffset;
                    obj.transform.position = newPosition;

                    treesFixed++;
                    Debug.Log($"Snapped tree '{obj.name}' to terrain at Y: {newPosition.y}");
                }
                else
                {
                    Debug.LogWarning($"Tree '{obj.name}' at {obj.transform.position} is not over any terrain. Using raycast method.");
                    RaycastHit terrainHit;
                    if (Physics.Raycast(obj.transform.position, Vector3.down, out terrainHit, maxRaycastDistance, terrainLayer))
                    {
                        Undo.RecordObject(obj.transform, "Snap Tree to Terrain");
                        Vector3 newPosition = obj.transform.position;
                        newPosition.y = terrainHit.point.y + terrainOffset;
                        obj.transform.position = newPosition;
                        treesFixed++;
                    }
                }
            }
        }

        Debug.Log($"Tree snapping complete! Fixed: {treesFixed}, Skipped: {treesSkipped}");
        EditorUtility.DisplayDialog("Tree Snapping Complete", 
            $"Fixed: {treesFixed} trees\nSkipped: {treesSkipped} trees", "OK");
    }
}
