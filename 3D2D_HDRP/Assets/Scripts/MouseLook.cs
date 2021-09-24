using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private CharController controllerScript;
    [SerializeField] private float MouseSensitivity = 100f;
    [SerializeField] private Transform Playerbody;
    float xRotation = 0f;
    float mouseX;
    float mouseY;
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
            if (!controllerScript.GetIsMovingObject())
            {
                mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
                mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
            }
            else
            {
                mouseY = 0;
                mouseX = 0;
            }
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            Playerbody.Rotate(Vector3.up * mouseX);
        }  
    }
}
