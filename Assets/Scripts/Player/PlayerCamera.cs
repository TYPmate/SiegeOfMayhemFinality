using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] public int mouseSensitivity = 900;
    public Camera cam;
    float xRotation;
    private float mouseX, mouseY;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    private void OnLook(InputValue input)
    {
        mouseX = input.Get<Vector2>().x;
        mouseY = input.Get<Vector2>().y;
    }

    // Update is called once per frame
    void Update()
    {   
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY * Time.deltaTime * (mouseSensitivity * PersistentManager.sensModifier);
        xRotation = Mathf.Clamp(xRotation, -80f, 60f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, -55, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * (mouseSensitivity * PersistentManager.sensModifier));
    }
}
