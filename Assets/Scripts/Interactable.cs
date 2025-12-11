using UnityEngine;

public class Interactable : MonoBehaviour
{

    public GameObject GameManager;
    private Manager managerScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        managerScript = GameManager.GetComponent<Manager>();
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void Interact()
    {
        Debug.Log("Player got target");
        Object.Destroy(this.gameObject);
        managerScript.addPoint();
    }
}
