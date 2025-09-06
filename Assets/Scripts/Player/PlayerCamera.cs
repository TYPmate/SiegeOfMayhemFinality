using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player camera movement and mouse look controls
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] public int mouseSensitivity = 900;
    public Camera cam;
    float xRotation;
    private float mouseX, mouseY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Input system callback for mouse look input
    /// </summary>
    /// <param name="input">Vector2 input from mouse movement</param>
    private void OnLook(InputValue input)
    {
        mouseX = input.Get<Vector2>().x;
        mouseY = input.Get<Vector2>().y;
    }

    void Update()
    {
        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        // Calculate vertical rotation with sensitivity modifier
        xRotation -= mouseY * Time.deltaTime * (mouseSensitivity * PersistentManager.sensModifier);
        xRotation = Mathf.Clamp(xRotation, -80f, 60f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, -55, 0);

        // Apply horizontal rotation with sensitivity modifier
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * (mouseSensitivity * PersistentManager.sensModifier));
    }
}