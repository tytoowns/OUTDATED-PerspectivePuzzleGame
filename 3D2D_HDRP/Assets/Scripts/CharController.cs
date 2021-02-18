using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{

    [SerializeField] private DecalController decalController;
    [SerializeField] private CharacterController controller;
    
    [SerializeField] private float WalkSpeed = 12f;
    [SerializeField] private float SprintSpeed = 16f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpheight = 2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = .4f;

    [SerializeField] private float interactDistance = .5f;
    
    private bool isGrounded;
    private Vector3 velocity;
    private float speed;
    static float SprintLerpT = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        speed = WalkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); //check if grounded

        if (Input.GetKeyDown(KeyCode.E) && controller.enabled == true)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, interactDistance))
            {
                if (hit.collider.GetComponent<IInteractable>() != null)
                {
                    hit.collider.GetComponent<IInteractable>().Interact();
                }
            }
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButton("Sprint") && isGrounded)
        {
            speed = Mathf.Lerp(speed, SprintSpeed, SprintLerpT);
            SprintLerpT += 0.1f * Time.deltaTime;
        }
        else
        {
            SprintLerpT = 0;
            speed = WalkSpeed;
        }

        if(controller.enabled) //if character controller enabled
        {
            Vector3 move = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized;
            move = move.normalized;

            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpheight * -2 * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }
}
