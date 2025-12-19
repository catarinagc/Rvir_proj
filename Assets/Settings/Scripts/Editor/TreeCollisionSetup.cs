using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to automatically set up trees with colliders and TreeCollision components.
/// </summary>
public class TreeCollisionSetup : EditorWindow
{
    private string treeNamePattern = "trees-new-try";
    private GameObject gameManager;
    private float colliderRadius = 1f;
    private float colliderHeight = 5f;
    private bool useCapsuleCollider = true;

    [MenuItem("Tools/Setup Tree Collisions")]
    public static void ShowWindow()
    {
        GetWindow<TreeCollisionSetup>("Tree Collision Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Tree Collision Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        treeNamePattern = EditorGUILayout.TextField("Tree Name Pattern", treeNamePattern);
        gameManager = (GameObject)EditorGUILayout.ObjectField("Game Manager", gameManager, typeof(GameObject), true);
        useCapsuleCollider = EditorGUILayout.Toggle("Use Capsule Collider", useCapsuleCollider);
        
        if (useCapsuleCollider)
        {
            colliderRadius = EditorGUILayout.FloatField("Collider Radius", colliderRadius);
            colliderHeight = EditorGUILayout.FloatField("Collider Height", colliderHeight);
        }
        else
        {
            colliderRadius = EditorGUILayout.FloatField("Collider Size", colliderRadius);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup All Trees"))
        {
            SetupTrees();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will add Colliders and TreeCollision components to all trees matching the name pattern. Make sure to save your scene after running this.", MessageType.Info);
    }

    private void SetupTrees()
    {
        if (gameManager == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign the Game Manager GameObject.", "OK");
            return;
        }

        int treesSetup = 0;
        int treesSkipped = 0;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (string.IsNullOrEmpty(treeNamePattern) || obj.name.Contains(treeNamePattern))
            {
                bool modified = false;

                // Check if tree already has a collider
                Collider existingCollider = obj.GetComponent<Collider>();
                if (existingCollider == null)
                {
                    // Add collider
                    if (useCapsuleCollider)
                    {
                        CapsuleCollider capsule = obj.AddComponent<CapsuleCollider>();
                        capsule.radius = colliderRadius;
                        capsule.height = colliderHeight;
                        capsule.center = new Vector3(0, colliderHeight * 0.5f, 0); // Center at bottom, extend upward
                    }
                    else
                    {
                        SphereCollider sphere = obj.AddComponent<SphereCollider>();
                        sphere.radius = colliderRadius;
                    }
                    modified = true;
                }
                else
                {
                    treesSkipped++;
                }

                // Check if tree already has TreeCollision component
                TreeCollision treeCollision = obj.GetComponent<TreeCollision>();
                if (treeCollision == null)
                {
                    treeCollision = obj.AddComponent<TreeCollision>();
                    treeCollision.GameManager = gameManager;
                    modified = true;
                }
                else if (treeCollision.GameManager == null)
                {
                    treeCollision.GameManager = gameManager;
                    modified = true;
                }

                if (modified)
                {
                    treesSetup++;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        Debug.Log($"Tree setup complete! Setup: {treesSetup}, Skipped: {treesSkipped}");
        EditorUtility.DisplayDialog("Tree Setup Complete", 
            $"Setup: {treesSetup} trees\nSkipped: {treesSkipped} trees", "OK");
    }
}
