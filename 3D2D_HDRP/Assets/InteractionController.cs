using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class InteractionController : MonoBehaviour
{
    public LayerMask InteractIgnoreLayers;

    [SerializeField] private float interactDistance = 1.5f;
    [SerializeField] private Animator armAnimator;
    [SerializeField] private CharController charController;
    [SerializeField] private CharacterController controller;//try to get rid of this maybe


    //interact vars
    private float holdETimer = 0f;
    private float interactTime = .5f;
    private GameObject interactingItem;// when holding E on an object, temp store it so we know if they look away
    private bool itemSet;
    private bool alreadyInteracted;

    [SerializeField] private Image crosshair;
    [SerializeField] private Image interactProgressCircle;
    [SerializeField] private TMP_Text interactText;

    [SerializeField] private float interactAppearTime = .4f;
    [SerializeField] private float interactFadeTime = .4f;
    private Tween interactFadetween;

    private float objectInteractTime = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, interactDistance, ~InteractIgnoreLayers))
        {
            if (hit.collider.GetComponent<IInteractable>() != null && hit.collider.GetComponent<IInteractable>().CanBeInteractedWith() == true)
            {
                //ui stuff
                interactText.DOColor(Color.white, interactAppearTime);//READ READ READ TO-DO: Hide normal cursor
                objectInteractTime = hit.collider.GetComponent<IInteractable>().GetInteractTime();
                if (objectInteractTime > 0)
                {
                    interactProgressCircle.DOColor(Color.white, interactAppearTime);
                }
                else
                {
                    interactProgressCircle.DOColor(Color.clear, interactAppearTime);
                }

                //timer for hold E interactions
                if (Input.GetKey(KeyCode.E) && !alreadyInteracted)//if you have not interacted yet (and holding E), continue
                {

                    interactFadetween.Kill();
                    interactFadetween = DOTween.To(() => interactProgressCircle.fillAmount, xx => interactProgressCircle.fillAmount = xx, 0, Mathf.Clamp(objectInteractTime - holdETimer, 0, Mathf.Infinity));

                    holdETimer += Time.deltaTime; //increase the time you have held E (the interact key)

                    if (!itemSet)//when first interacting we need to set the item interacting with
                    {
                        interactTime = objectInteractTime;
                        interactingItem = hit.transform.gameObject;
                        itemSet = true;
                    }
                    else if (interactingItem == hit.transform.gameObject)//then compare the current item being looked at to make sure its the same
                    {
                        if (holdETimer >= interactTime)
                        {
                            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("MoveableObjects") && charController.GetIsGrounded())
                            {
                                //MoveToPushObject(hit.collider.gameObject);
                                //need to be grounded to interact with moveable objects
                                hit.collider.GetComponent<IInteractable>().Interact();
                                //PushObject(hit);
                                alreadyInteracted = true;
                            }
                            else
                            {
                                hit.collider.GetComponent<IInteractable>().Interact();
                                alreadyInteracted = true;
                            }

                        }
                    }

                }
            }
            else
            {
                Debug.Log("hiding here 1");////////////////////////////
                HideInteractUI();
            }
        }
        else
        {
            HideInteractUI();
        }



        if (Input.GetKeyUp(KeyCode.E))//on key up, reset values
        {
            interactFadetween.Kill();
            interactFadetween = DOTween.To(() => interactProgressCircle.fillAmount, xx => interactProgressCircle.fillAmount = xx, 1, .3f);
            itemSet = false;
            alreadyInteracted = false;
            holdETimer = 0;
        }
    }

    private void HideInteractUI()
    {
        interactText.DOColor(Color.clear, interactFadeTime);
        interactProgressCircle.DOColor(Color.clear, interactFadeTime);
        interactFadetween.Kill();
        interactFadetween = DOTween.To(() => interactProgressCircle.fillAmount, xx => interactProgressCircle.fillAmount = xx, 1, .3f);
    }

    public void PushAnim(bool startPushing)
    {
        if(startPushing)
            armAnimator.SetTrigger("StartPush");
        else
            armAnimator.SetTrigger("EndPush");
    }
}

/*
    private void CheckIfCanPush(GameObject objToPush, RaycastHit hit)
    {

    }

    private void MoveToPushObject(GameObject obj)
    {
        Transform closestPoint = transform;

        List<float> distance = new List<float>();
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), Color.magenta, 5);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, interactDistance, ~InteractIgnoreLayers))
        {
            if (obj.transform.Find("sideCenterTransforms"))
            {
                Transform sidesParent = obj.transform.Find("sideCenterTransforms");
                foreach (Transform child in sidesParent)
                {
                    Debug.DrawLine(hit.point, child.transform.position,Color.red,10);
                    distance.Add(Vector3.Distance(hit.point, child.transform.position));
                    //Debug.Log(Vector3.Distance(hit.transform.position, child.transform.position));
                }
                int index = distance.IndexOf(distance.Min());

                closestPoint = sidesParent.GetChild(index);
                //transform.position = closestPoint.position;
                //do a capsule check here at closest point
                //check hit side if Physics.OverlapCapsule
                //if so, no UI
                //else let user press E and push
                //move player to the side transform
                Debug.Log(closestPoint.name);
                Debug.DrawLine(closestPoint.position, Camera.main.transform.position, Color.green,10);


                //closest point - raycast down - move player height up/2? = end pos

                Sequence mySequence = DOTween.Sequence();
                mySequence.Insert(0, transform.DOMove(closestPoint.position, 1));
                lookPos = obj.transform.position;
                //lookPos.y = Camera.main.transform.position.y;


                //Vector3.RotateTowards(transform.position, lookPos, 0, 0);
                //mySequence.Insert(0, transform.DOLookAt(lookPos, 1,AxisConstraint.Z));

                mySequence.AppendCallback(EnableCharCont);
                controller.enabled = false; //disable the characterController so we can teleport the player
                mySequence.Play();

                void EnableCharCont()
                {
                    mySequence.Kill();
                    controller.enabled = true; //reEnable the chacterController
                    obj.GetComponent<IInteractable>().Interact(); // now lock obj to player
                }

            }
        }



    }
    private Vector3 lookPos;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(lookPos, new Vector3(.2f, .2f, .2f));
    }

    private void PushObject(RaycastHit hitObject)
    {
        isMovingObject = !isMovingObject;
        charController.SetIsMovingObject(isMovingObject);
        if (isMovingObject)
        {
            if (this.transform.childCount > 3)
            {
                objectMoving = this.gameObject.transform.GetChild(3).gameObject;
                //lerp move player to adjecent to object face in the center, set distance away
                armAnimator.SetTrigger("StartPush");
            }
        }
        else
        {
            armAnimator.SetTrigger("EndPush");
        }
    }
*/

