using System.Diagnostics;
using UnityEngine;

public class HeadGestureDetector : MonoBehaviour
{
    // [Header("Gesture Settings")]
    // public float upwardAngleThreshold = -10f;  // head looking up (pitch is negative)
    // public float minAngularSpeed = 30f;        // degrees per second
    // public float gestureCooldown = 0.0f;

    // private float lastPitch;
    // private float cooldownTimer = 0f;

    // public System.Action OnLookUpGesture;

    // void Start()
    // {
    //     lastPitch = GetPitch(transform.rotation);
    // }

    // void Update()
    // {
    //     cooldownTimer -= Time.deltaTime;

    //     float pitch = GetPitch(transform.rotation);
    //     float pitchDelta = pitch - lastPitch;
    //     float angularSpeed = pitchDelta / Time.deltaTime;  // deg per second

    //     // Detect quick look-up gesture
    //     if (cooldownTimer <= 0f &&
    //         angularSpeed < -minAngularSpeed &&      // rapid upward motion
    //         pitch < upwardAngleThreshold)          // AND looking upward
    //     {
    //         cooldownTimer = gestureCooldown;
    //         OnLookUpGesture?.Invoke();
    //     }

    //     lastPitch = pitch;
        
    // }

    // private float GetPitch(Quaternion q)
    // {
    //     Vector3 e = q.eulerAngles;
    //     // Convert Unity's 0�360 pitch into -180�180
    //     float pitch = e.x > 180 ? e.x - 360 : e.x;
    //     return pitch;
    // }

    [Header("Sensitivity (Lower = Easier)")]
    [Tooltip("How far down the user must look")]
    public float lookDownAngle = 8f;     // ↓ lowered

    [Tooltip("How far up the user must return")]
    public float lookUpAngle = -2f;      // ↓ lowered

    [Tooltip("Minimum upward speed (deg/sec)")]
    public float minUpwardSpeed = 4f;    // ↓ much lower

    [Header("Smoothing")]
    [Tooltip("Smooths jittery headset motion")]
    public float pitchSmoothing = 8f;

    [Header("Timing")]
    public float cooldown = 0.3f;        // ↓ faster reuse

    // public Action OnNod;

    float lastPitch;
    float cooldownTimer;
    bool nodArmed = false;

    public System.Action OnLookUpGesture;

    void Start()
    {
        lastPitch = GetPitch(transform.rotation);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        float pitch = GetPitch(transform.rotation);
        float delta = pitch - lastPitch;
        float speed = delta / Mathf.Max(Time.deltaTime, 0.0001f);

        // STEP 1 — Look down arms the nod
        if (pitch > lookDownAngle)
        {
            nodArmed = true;
        }

        // STEP 2 — Fast upward motion confirms nod
        if (nodArmed &&
            cooldownTimer <= 0f &&
            speed < -minUpwardSpeed &&
            pitch < lookUpAngle)
        {
            nodArmed = false;
            cooldownTimer = cooldown;

            // Debug.Log("HEAD NOD DETECTED");
            OnLookUpGesture?.Invoke();
        }

        lastPitch = pitch;
    }

    float GetPitch(Quaternion rot)
    {
        float pitch = rot.eulerAngles.x;
        return pitch > 180f ? pitch - 360f : pitch;
    }
}
