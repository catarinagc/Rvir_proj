using UnityEngine;

public class HeadSelection : MonoBehaviour
{
    public float rayLength = 1.0f;
    public float maxDistance = 10f;
    public Transform reticle;

    private GameObject currentObject;

    void Update()
    {
        if (!reticle) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            reticle.position = hit.point;
            reticle.rotation = Quaternion.LookRotation(-hit.normal);
            currentObject = hit.collider.gameObject;
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }
        else
        {
            //reticle.position = ray.origin + ray.direction * maxDistance;
            reticle.position = transform.position + transform.forward * maxDistance;
            reticle.rotation = transform.rotation;
            currentObject = null;
        }

        // Example interaction input (trigger / mouse click / key)
        if (/*Input.GetButtonDown("Fire1") && */ currentObject != null)
        {
            InteractWithObject();
        }
    }

    void InteractWithObject()
    {
        var interactable = currentObject.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Interact();
        }

        Debug.Log("Interacted with: " + currentObject.name);
    }
}
