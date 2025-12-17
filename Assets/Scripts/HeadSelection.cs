using System.Diagnostics;
using System.Threading;
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
    private Renderer reticleRenderer;
    private bool triggerPressedLastFrame = false;
    //private int greenBallCount = 0; // button interactions
    //private int redBallCount = 0; // head interactions
    //private float lastSelectionTime = 0f; // start at 0 or scene start


    void Start()
    {
        detector = GetComponent<HeadGestureDetector>();
        if (detector != null)
        {
            detector.OnLookUpGesture += TryInteract;
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
        {
            interactable.Interact();
            //greenBallCount++;

            /* float reactionTime = Time.time - lastSelectionTime;
            * lastSelectionTime = Time.time;
            * Debug.Log($"Green ball selected at {Time.time:F2}s (reaction time: {reactionTime:F2}s)"); */
        }
        //Debug.Log("Interacted with: " + currentObject.name); //possibly delete since not clear? will say Interacted with: targetHeadSelection I think

    }

    void InteractWithObjectButton()
    {
        var interactable = currentObject?.GetComponent<InteractableButton>();
        if (interactable != null)
        {
            interactable.Interact();
            //redBallCount++;

            /* float reactionTime = Time.time - lastSelectionTime;
             * lastSelectionTime = Time.time;
             * Debug.Log($"Red ball selected at {Time.time:F2}s (reaction time: {reactionTime:F2}s)"); */
        }
        //Debug.Log("Interacted with: " + currentObject.name); //why is this commented out? possibly delete?
    }


}

