using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float MouseSensitivity = 100f;
    [SerializeField] private Transform Playerbody;
    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if(controller.enabled) //if you can move / look around
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            Playerbody.Rotate(Vector3.up * mouseX);
        }  
    }
}
