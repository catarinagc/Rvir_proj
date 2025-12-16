using UnityEngine;
using UnityEngine.XR;
using System;

public class XRHeadNodDetector : MonoBehaviour
{
    [Header("Very Easy Sensitivity")]
    public float downAngle = 5f;
    public float upAngle = -1f;
    public float minUpSpeed = 2f;

    [Header("Timing")]
    public float cooldown = 0.25f;

    public Action OnNod;

    float lastPitch;
    float cooldownTimer;
    bool armed;

    InputDevice hmd;

    void Start()
    {
        hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        lastPitch = GetPitch();
    }

    void Update()
    {
        if (!hmd.isValid)
            hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        cooldownTimer -= Time.deltaTime;

        float pitch = GetPitch();
        float delta = pitch - lastPitch;
        float speed = delta / Mathf.Max(Time.deltaTime, 0.0001f);

        // Arm on tiny downward movement
        if (pitch > downAngle)
            armed = true;

        // Trigger on tiny upward movement
        if (armed &&
            cooldownTimer <= 0f &&
            speed < -minUpSpeed &&
            pitch < upAngle)
        {
            armed = false;
            cooldownTimer = cooldown;

            Debug.Log("XR NOD DETECTED");
            OnNod?.Invoke();
        }

        lastPitch = pitch;
    }

    float GetPitch()
    {
        if (hmd.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rot))
        {
            float pitch = rot.eulerAngles.x;
            return pitch > 180 ? pitch - 360 : pitch;
        }
        return lastPitch;
    }
}
