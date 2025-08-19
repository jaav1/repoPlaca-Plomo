using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class CarController : MonoBehaviour
{
    [HideInInspector] public bool ignitionAuthorized = false;

    [Header("Suspensión")]
    public float maxTiltAngle = 5f;
    public float tiltSmoothSpeed = 5f;

    [Header("Movimiento")]
    public float acceleration = 10f;
    public float turnSpeed = 50f;
    public float brakeForce = 20f;

    [Header("Audio")]
    public AudioClip engineClip;
    public AudioClip brakeClip;

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isDriving = false;
    private bool brakeSoundPlayed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        audioSource.clip = engineClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.Stop();
        audioSource.volume = 0f;

        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (!isDriving)
        {
            audioSource.volume = 0f;
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 force = transform.forward * v * acceleration;
        rb.AddForce(force, ForceMode.Acceleration);

        if (Mathf.Abs(v) > 0.1f)
        {
            float direccionGiro = (v > 0) ? 1f : -1f;
            Quaternion turnOffset = Quaternion.Euler(0, h * turnSpeed * Time.fixedDeltaTime * direccionGiro, 0);
            rb.MoveRotation(rb.rotation * turnOffset);
        }

        audioSource.volume = Mathf.Clamp01(rb.linearVelocity.magnitude / 10f);

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 brake = -rb.linearVelocity.normalized * brakeForce;
            rb.AddForce(brake, ForceMode.Acceleration);

            if (!brakeSoundPlayed && brakeClip != null)
            {
                audioSource.PlayOneShot(brakeClip);
                brakeSoundPlayed = true;
            }
        }
        else
        {
            brakeSoundPlayed = false;
        }

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float tiltAngle = Mathf.Lerp(0, -maxTiltAngle, forwardSpeed / 20f);

        Quaternion currentRot = rb.rotation;
        Quaternion tiltRot = Quaternion.Euler(tiltAngle, currentRot.eulerAngles.y, currentRot.eulerAngles.z);
        rb.MoveRotation(Quaternion.Slerp(currentRot, tiltRot, tiltSmoothSpeed * Time.fixedDeltaTime));
    }

    public void StartDriving()
    {
        if (!ignitionAuthorized)
        {
            Debug.LogWarning("Intento de encender el coche sin autorización.");
            return;
        }

        isDriving = true;
        rb.constraints = RigidbodyConstraints.None;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    public void EnableDriving()
    {
        isDriving = true;
        Debug.Log("🚗 Coche activado. Puedes conducir.");

        rb.constraints = RigidbodyConstraints.None;

        if (!audioSource.isPlaying && engineClip != null)
        {
            audioSource.clip = engineClip;
            audioSource.Play();
        }
    }

    public void StopDriving()
    {
        if (rb == null)
        {
            Debug.LogError("Rigidbody no está asignado en CarController.");
            return;
        }

        isDriving = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (audioSource != null)
            audioSource.Stop();

        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }
}