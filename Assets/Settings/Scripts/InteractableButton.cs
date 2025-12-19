using UnityEngine;

public class InteractableButton : MonoBehaviour
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
            managerTutorial.addPointButton();
            gameObject.SetActive(false);
        }
        else
        {
            mover.ForceDespawnByPlayer();
            managerScript.addPointButton();
        }
    }
}
