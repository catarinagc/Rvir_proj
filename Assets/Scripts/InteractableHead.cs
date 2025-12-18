using UnityEngine;

public class InteractableHead : MonoBehaviour
{

    public GameObject GameManager;
    private Manager managerScript;
    private ObjectMover mover;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        managerScript = GameManager.GetComponent<Manager>();
        mover = GetComponent<ObjectMover>();
    }

    public void Interact()
    {
        Debug.Log("Player got target");
        mover.ForceDespawnByPlayer();
        managerScript.addPointHead();
    }
}
