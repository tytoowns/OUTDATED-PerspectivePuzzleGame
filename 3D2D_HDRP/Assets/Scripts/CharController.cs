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

    private Vector3 vel;


    private bool isMovingObject;
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

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButton("Sprint") && isGrounded && !isMovingObject)//CHANGE - maybe remove is grounded so that you cant start sprinting while in mid air, but keeps speed if started before 
        {
            speed = Mathf.Lerp(speed, SprintSpeed, SprintLerpT);
            SprintLerpT += 0.1f * Time.deltaTime;
        }
        else
        {
            SprintLerpT = 0;
            speed = WalkSpeed;
        }

        if(controller.enabled) //if character controller enabled then let the play move
        {
            float movementSpeed = speed;
            Vector3 move = (transform.forward * Input.GetAxisRaw("Vertical")).normalized;// W and S

            //if moving object, slow movement speed
            if (isMovingObject)
                movementSpeed = movementSpeed / 4;
            else
                move += (transform.right * Input.GetAxisRaw("Horizontal")).normalized;


            //move player
            controller.Move(move * movementSpeed * Time.deltaTime);
            vel = controller.velocity;

            //jump
            if (Input.GetButtonDown("Jump") && isGrounded && !isMovingObject)
            {
                velocity.y = Mathf.Sqrt(jumpheight * -2 * gravity);
            }

            //add gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }




    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public void SetIsMovingObject(bool MovingObj)
    {
        isMovingObject = MovingObj;
    }

    public bool GetIsMovingObject()
    {
        return isMovingObject;
    }

    public Vector3 GetVel()
    {
        return vel;
    }
}
