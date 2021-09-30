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
    Vector3 halfExtents;
    [SerializeField] private float pushSpeed = 1.35f;

    //lock the x or z axis depending on movement


    // Start is called before the first frame update
    void Start()
    {
        parentObject = objectToMove;
        m_started = true;
        interactTime = StartPushInteractTime;
        halfExtents = new Vector3(.49f, .99f, .49f);//(objectToMove.transform.localScale.x / 2) - 0.1f, (objectToMove.transform.localScale.y / 2) - 0.1f, (objectToMove.transform.localScale.z / 2) - 0.1f);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (isBeingMoved)//they way i move the moveable object needs to be changed (wont work on a slope) and isnt very good atm
        {
            if(player.GetComponent<CharacterController>().enabled == true)
            {
                parentObject.GetComponent<Rigidbody>().velocity = (player.transform.forward * Input.GetAxisRaw("Vertical")).normalized * pushSpeed;
            }
        }
        else
        {
            if(parentObject.GetComponent<Rigidbody>().useGravity == false)//make shift bool to check if other sides are being moved
            {
                parentObject.GetComponent<Rigidbody>().velocity = Vector3.zero;//dont let it be moved when the player isnt moving it

            }
        }
    }
    /* 
     
    need to lock Y so player cant jump
         lock x or z so player can only push and pull



        dont let them jump
        start/end push animation

         */
    Vector3 lookAtPos;
    public float moveToPushSpeed = 1.1f;
    public void Interact()
    {
        isBeingMoved = !isBeingMoved;
        if (isBeingMoved)
        {
            //it is now being pushed
            player.GetComponent<InteractionController>().PushAnim(true);
            
            interactTime = StopPushinginteractTime;

            parentObject.GetComponent<Rigidbody>().useGravity = true;
            Sequence mySequence = DOTween.Sequence();
            Vector3 movPos = transform.position;
            movPos.y += .08f;
            
            lookAtPos = parentObject.GetComponent<MeshRenderer>().bounds.center;
            float moveDuration = 0;
            float movedis = Vector3.Distance(player.transform.position, movPos);
            if (movedis > 0.25f)
                moveDuration = movedis / moveToPushSpeed;
            else
                moveDuration = 0.1f;

            //Debug.Log(moveDuration);
            mySequence.Insert(0, player.transform.DOMove(movPos, moveDuration, false)); //time needs to depend on how close you are. so if you are in position, you dont have to wait
            mySequence.Insert(1, player.transform.DOLookAt(lookAtPos, 1,AxisConstraint.Y));//depends how far away player is looking
            //if you change   ^ to 0 the rotation doesnt work. why?
            //maybe because it uses players current pos not the pos to where they will moved to
            //when doing the calcs for rotation

            mySequence.Insert(1, Camera.main.transform.DOLocalRotate(Vector3.zero, 1));//same as above
            Camera.main.GetComponent<MouseLook>().ResetXRotation();
            
            

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
            parentObject.GetComponent<Rigidbody>().useGravity = false; 
            player.GetComponent<CharController>().SetIsMovingObject(false);
            interactTime = StartPushInteractTime;
            //objectToMove.transform.parent = null;
        }
    }

    public float GetInteractTime()
    {
        return interactTime;
    }

    public bool CanBeInteractedWith()
    {
        //check if the interaction is being blocked (for example, no room for player)
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
            //Draw a cube where the OverlapBox is
            if(m_started)
                Gizmos.DrawWireCube(transform.position, halfExtents * 2);
                
    }
}
