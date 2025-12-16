using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class HeadSelection : MonoBehaviour
{
    public float maxDistance = 10f;
    public Transform reticle;

    public Color normalColor = Color.white;
    public Color hitColor = Color.red;

    private GameObject currentObject;
    private HeadGestureDetector detector;
    private XRHeadNodDetector nodDetector;
    private Renderer reticleRenderer;
    private bool triggerPressedLastFrame = false;


    void Start()
    {
        //detector = GetComponent<HeadGestureDetector>();
        nodDetector = GetComponent<XRHeadNodDetector>();
        if (nodDetector != null)
        {
            nodDetector.OnNod += TryInteract;
        }

        if (reticle != null)
        {
            reticleRenderer = reticle.GetComponent<Renderer>();
        }
    }

    void TryInteract()
    {
        if (currentObject != null)
            InteractWithObjectHead();
    }

    void Update()
    {
        HandleRaycast();
        HandleControllerInput();
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (reticle != null)
            {
                reticle.position = hit.point;
                reticle.rotation = Quaternion.LookRotation(-hit.normal);
            }
            if (reticleRenderer != null && reticleRenderer.material != null)
            {
                reticleRenderer.material.color = hitColor;
            }
            currentObject = hit.collider.gameObject;
        }
        else
        {
            if (reticle != null)
            {
                reticle.position = transform.position + transform.forward * maxDistance;
                reticle.rotation = transform.rotation;
            }
            if (reticleRenderer != null && reticleRenderer.material != null)
            {
                reticleRenderer.material.color = normalColor;
            }
            currentObject = null;
        }
    }

    void HandleControllerInput()
    {
        bool triggerValue = false;
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
            .TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue))
        {
            // Edge detection: only fire when trigger is newly pressed
            if (triggerValue && !triggerPressedLastFrame)
            {
                InteractWithObjectButton();
            }

            triggerPressedLastFrame = triggerValue;
        }
    }


    void InteractWithObjectHead()
    {
        var interactable = currentObject?.GetComponent<InteractableHead>();
        if (interactable != null)
            interactable.Interact();

        Debug.Log("Interacted with: " + currentObject.name);
    }

    void InteractWithObjectButton()
    {
        var interactable = currentObject?.GetComponent<InteractableButton>();
        if (interactable != null)
            interactable.Interact();

        //Debug.Log("Interacted with: " + currentObject.name);
    }
}

