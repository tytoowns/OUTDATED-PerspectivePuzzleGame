using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class moveableObject : MonoBehaviour, IInteractable
{
    bool m_started;
    private bool isBeingMoved;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject objectToMove;
    [SerializeField] private float StartPushInteractTime = .3f;
    [SerializeField] private float StopPushinginteractTime = .0f;
    [SerializeField] private LayerMask ignoreWhenCheckingIfBlocked;
    private float interactTime;
    private GameObject parentObject;

    // Start is called before the first frame update
    void Start()
    {
        parentObject = objectToMove;
        m_started = true;
        interactTime = StartPushInteractTime;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isBeingMoved)
        {
            if(player.GetComponent<CharacterController>().enabled == true)
            {
                parentObject.GetComponent<Rigidbody>().velocity = (player.transform.forward * Input.GetAxisRaw("Vertical")).normalized * 1.25f;
            }
            //parentobject.getcomponent<rigidbody>().velocity = player.GetComponent<CharController>().GetVel();
            //parentObject.GetComponent<Rigidbody>().AddForce(player.GetComponent<CharController>().GetVel(), ForceMode.VelocityChange);
        }
        else
        {
            if(parentObject.GetComponent<Rigidbody>().useGravity == true)//make shift bool to check if other sides are being moved
            {
                parentObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

            }
        }
    }
    /* 
     
    need to lock Y so player cant jump
         lock x or z so player can only push and pull

        lock cam
        dont let player move left/right
        dont let them jump
        start/end push animation

         */
    Vector3 lookAtPos;

    public void Interact()
    {
        Debug.Log("interact..... NOW");
        isBeingMoved = !isBeingMoved;
        if (isBeingMoved)
        {
            player.GetComponent<InteractionController>().PushAnim(true);
            //it is now being pushed
            interactTime = StopPushinginteractTime;

            parentObject.GetComponent<Rigidbody>().useGravity = false;
            Sequence mySequence = DOTween.Sequence();
            Vector3 movPos = transform.position;
            movPos.y += .08f;
            
            lookAtPos = parentObject.GetComponent<MeshRenderer>().bounds.center;

            mySequence.Insert(0, player.transform.DOMove(movPos, 1,false));
            mySequence.Insert(1, player.transform.DOLookAt(lookAtPos, 1,AxisConstraint.Y));
            //if you change   ^ to 0 the rotation doesnt work. why?
            //maybe because it uses players current pos not the pos to where they will moved to
            //when doing the calcs for rotation

            mySequence.AppendCallback(EnableCharCont);

            player.GetComponent<CharacterController>().enabled = false; //disable the characterController so we can teleport the player
            mySequence.Play();

            void EnableCharCont()
            {
                player.GetComponent<CharacterController>().enabled = true; //reEnable the chacterController
            }

            //move player into position
            //rotate them to face the pushable object


            //lock their movement to W/S
            //lock their camera rotation -- maybe not fully so they can look around a little (but that would change w/s movement. 
            player.GetComponent<CharController>().SetIsMovingObject(true);
            

            //objectToMove.transform.parent = player.transform;
        }
        else
        {
            player.GetComponent<InteractionController>().PushAnim(false);
            parentObject.GetComponent<Rigidbody>().useGravity = true; 
            player.GetComponent<CharController>().SetIsMovingObject(false);
            interactTime = StartPushInteractTime;
            //objectToMove.transform.parent = null;
        }
    }

    public float GetInteractTime()
    {
        return interactTime;
    }

    Vector3 halfExtents = new Vector3(.49f, .9f, .49f);
    public bool CanBeInteractedWith()
    {
        //check if the interaction is being blocked

        Collider[] hits = Physics.OverlapBox(transform.position, halfExtents, Quaternion.identity, ~ignoreWhenCheckingIfBlocked);

        foreach(Collider collider in hits)
        {
            if(collider.gameObject != parentObject)
            {
                return false;
            }
        }

        return true;
    }

    //shows the area it checks when it sees if it is blocked
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(lookAtPos, new Vector3(.2f, .2f, .2f));

        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            if(m_started)
                Gizmos.DrawWireCube(transform.position, halfExtents * 2);
                
    }
}
