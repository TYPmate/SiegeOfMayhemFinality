using UnityEngine;

public class NoKeyDoorOpener : MonoBehaviour
{
    public Transform door;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public float openSpeed = 2f;

    private bool isOpening = false;
    private bool hasPlayedSound = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    public AudioSource doorSound;

    void Start()
    {
        initialRotation = door.rotation;
        targetRotation = Quaternion.Euler(openRotation) * initialRotation;
    }

    void Update()
    {
        if (isOpening)
        {
            door.rotation = Quaternion.Slerp(door.rotation, targetRotation, Time.deltaTime * openSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
        if (playerMotor != null)
        {
            if (other.CompareTag("Player"))
            {
                isOpening = true;
                if (!hasPlayedSound)
                {
                    doorSound.Play();
                    hasPlayedSound = true;
                }
            }
        }
    }
}