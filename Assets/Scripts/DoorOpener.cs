using UnityEngine;

/// <summary>
/// Handles door opening functionality when player has the required key
/// </summary>
public class DoorOpener : MonoBehaviour
{
    public Transform door;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float openSpeed = 2f;

    private bool isOpening = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    public AudioSource doorSound;

    /// <summary>
    /// Initializes door rotation values
    /// </summary>
    void Start()
    {
        initialRotation = door.rotation;
        targetRotation = Quaternion.Euler(openRotation) * initialRotation;
    }

    /// <summary>
    /// Updates door rotation if opening sequence is active
    /// </summary>
    void Update()
    {
        if (isOpening)
        {
            door.rotation = Quaternion.Slerp(door.rotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    /// <summary>
    /// Handles trigger entry to detect player with key
    /// </summary>
    /// <param name="other">The collider entering the trigger zone</param>
    void OnTriggerEnter(Collider other)
    {
        PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
        if (playerMotor != null && playerMotor.hasKey)
        {
            if (other.CompareTag("Player"))
            {
                isOpening = true;
                playerMotor.hasKey = false;
                SoundManager.Instance.PlaySound(SoundManager.Instance.effects[3]);
            }
        }
    }
}