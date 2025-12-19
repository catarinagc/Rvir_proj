using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HeadSelection : MonoBehaviour
{
    [Header("Raycast")]
    public float maxDistance = 10f;
    public Transform reticle;

    [Header("Reticle Colors")]
    public Color normalColor = Color.white;
    public Color hitColor = Color.red;

    [Header("Sticky Target")]
    [Tooltip("How long (seconds) to keep last target after ray leaves")]
    public float stickTime = 0.15f;

    private XRHeadNodDetector nodDetector;
    private Renderer reticleRenderer;

    // Current raycast hit
    private GameObject currentObject;

    // Sticky cache
    private GameObject stickyObject;
    private float stickyTimer;

    // Locked during nod
    private GameObject lockedObject;
    private bool triggerPressedLastFrame = false;

    void Start()
    {
        nodDetector = GetComponent<XRHeadNodDetector>();
        if (nodDetector != null)
        {
            //nodDetector.OnNodStart += LockCurrentObject;
            nodDetector.OnNod += TryInteract;
        }

        if (reticle != null)
            reticleRenderer = reticle.GetComponent<Renderer>();
    }

    void Update()
    {
        HandleRaycast();
        UpdateStickyTarget();
        HandleControllerInput();
    }

    // ===============================
    // Raycast aiming
    // ===============================
    void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            currentObject = hit.collider.gameObject;

            // Refresh sticky target
            //stickyObject = currentObject;
            //stickyTimer = stickTime;

            if (currentObject.GetComponent<InteractableHead>() != null)
            {
                Debug.Log(currentObject);
                stickyObject = currentObject;
                stickyTimer = stickTime;
            }
            

            if (reticle != null)
            {
                reticle.position = hit.point;
                reticle.rotation = Quaternion.LookRotation(-hit.normal);
            }

            if (reticleRenderer != null)
                reticleRenderer.material.color = hitColor;
        }
        else
        {
            currentObject = null;

            if (reticle != null)
            {
                reticle.position = transform.position + transform.forward * maxDistance;
                reticle.rotation = Quaternion.LookRotation(transform.forward);
            }

            if (reticleRenderer != null)
                reticleRenderer.material.color = normalColor;
        }
    }

    // ===============================
    // Sticky grace window
    // ===============================
    void UpdateStickyTarget()
    {
        if (stickyObject == null)
            return;

        stickyTimer -= Time.deltaTime;

        if (stickyTimer <= 0f)
        {
            stickyObject = null;
        }
    }

    // ===============================
    // Nod start → lock sticky target
    // ===============================
    // void LockCurrentObject()
    // {
    //     lockedObject = stickyObject;

    //     if (lockedObject != null)
    //         Debug.Log("Locked (sticky): " + lockedObject.name);
    // }

    // ===============================
    // Nod end → interact
    // ===============================
    void TryInteract()
    {
        if (stickyObject == null)
            return;

        var interactable = stickyObject.GetComponent<InteractableHead>();
        if (interactable != null)
            interactable.Interact();

        stickyObject = null;
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

     void InteractWithObjectButton()
    {
        var interactable = currentObject?.GetComponent<InteractableButton>();
        if (interactable != null)
            interactable.Interact();

        //Debug.Log("Interacted with: " + currentObject.name);
    }
}
