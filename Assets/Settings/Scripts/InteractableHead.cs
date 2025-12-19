using UnityEngine;

public class InteractableHead : MonoBehaviour
{

    public GameObject GameManager;
    private Manager managerScript;
    private ManagerTutorial managerTutorial;
    private ObjectMover mover;

    public bool isTutorial = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isTutorial)
        {
            managerTutorial = GameManager.GetComponent<ManagerTutorial>();
        }
        else
        {
            managerScript = GameManager.GetComponent<Manager>();         
            mover = GetComponent<ObjectMover>();
        }
    }

    public void Interact()
    {
        Debug.Log("Player got target");
        if (isTutorial)
        {
            managerTutorial.addPointHead();
            gameObject.SetActive(false);
        }
        else
        {
            mover.ForceDespawnByPlayer();
            managerScript.addPointHead();
        }
    }
}
