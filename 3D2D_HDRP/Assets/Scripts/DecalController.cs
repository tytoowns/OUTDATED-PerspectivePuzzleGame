using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using DG.Tweening;
using Knife.HDRPOutline;
using Knife;
using Knife.HDRPOutline.Core;

public class DecalController : MonoBehaviour
{

    [SerializeField] private VolumeProfile volumeProfile; // PostProcessingVolume for adding effects
    [SerializeField] private GameObject decalParent;
    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private Material decalMaterial;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Image crosshair;
    [SerializeField] private CharController charController;
    [SerializeField] private Camera mainCam;

    [SerializeField] private float decalFadeInTime = .5f;
    [SerializeField] private float decalFadeOutTime = .25f;
    [SerializeField] private float decalMovementSpeed = 15f;
    [SerializeField] private float decalOOBCheckOffset = 1.5f;

    [SerializeField] private float teleportLandingOffset = 2f; //distance away from the wall
    [SerializeField] private float teleportSpeed = 2f; //distance away from the wall
    [SerializeField] private float teleportTurnPoint = .75f;
    [SerializeField] private float teleportTurnCutoffDistance = 2f;

    [SerializeField] private Volume TeleportPostProVolume;
    [SerializeField] private float TeleportPostProVolumeFadeInTime = .5f;
    [SerializeField] private float TeleportPostProVolumeFadeOutTime = .25f;

    [SerializeField] private ArmController armController;

    float xRotation = 0f;
    float yRotation = 0f;

    private bool isDecalOn;
    private Vector3 decalDefaultSize;

    private bool isTeleporting = false;
    private bool teleportAvailable;
    private Vector3 teleportPosition;

    private Color decalDefaultColor;
    private HDRPOutline hDRPOutline;

    private void Awake()
    {
        TeleportPostProVolume.weight = 0;
        decalDefaultSize = decalProjector.size; // set the defaultSize
        decalProjector.fadeFactor = 0;

        DOTween.Init();

        volumeProfile.TryGet<HDRPOutline>(out hDRPOutline);
        if (hDRPOutline != null)
        {
            hDRPOutline.width.value = 0;
        }
        currentObject = decalParent;
        moving2DplatformPos = decalParent.transform.position;

        decalDefaultColor = decalMaterial.GetColor("_EmissiveColor");
    }


    private GameObject currentObject;
    private Vector3 moving2DplatformPos;

    private Vector3 objectMovement;

    public LayerMask IgnoreMe;

    // Update is called once per frame
    void Update()
    {
        #region playerInput
        if (charController.GetIsGrounded() && !charController.GetIsMovingObject()) // if you are grounded
        {
            if (Input.GetMouseButtonDown(0) && characterController.enabled) // and press down the left moust button
            {
                SetDecalOn(true); // turn on the Decal
                //canMove = false; // and stop movement
                //can move is set to false in decal controller as we need to check the decal is full on object before we continue
            }
            if (Input.GetMouseButtonUp(0))
            {
                SetDecalOn(false);
                if (!isTeleporting)
                {
                    characterController.enabled = true; //reEnable the chacterController

                }
            }
        }
        #endregion

        if (isDecalOn)
        {
            if (crosshair.color != Color.clear)
            {
                crosshair.DOColor(Color.clear, .35f);
            }

            //check if 2d platform is moving
            RaycastHit parenthit;
            if (Physics.Raycast(decalParent.transform.position, decalParent.transform.TransformDirection(Vector3.forward), out parenthit, Mathf.Infinity, ~IgnoreMe))
            {
                if (parenthit.collider.gameObject == currentObject) //raycast, see if we're on the same object 
                {
                    if (moving2DplatformPos != parenthit.collider.gameObject.transform.position)//check if the object is moving
                    {
                        //calculate how it moved
                        objectMovement = parenthit.collider.gameObject.transform.position - moving2DplatformPos;
                        moving2DplatformPos = parenthit.collider.gameObject.transform.position;

                        decalParent.transform.position += objectMovement; //add the platforms movement to the decalparents position
                        objectMovement = new Vector3(0, 0, 0); // might not need this
                    }
                }
                else // if we moved the decal onto another object
                {
                    currentObject = parenthit.collider.gameObject;
                    moving2DplatformPos = parenthit.collider.gameObject.transform.position;
                }
            }

            #region DecalMovement
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            if (x != 0 || z != 0)
            {
                float rotSpeed = decalMovementSpeed / Vector3.Distance(decalParent.transform.position, parenthit.collider.gameObject.transform.position);
                x = x * rotSpeed * Time.deltaTime;
                z = z * rotSpeed * Time.deltaTime;

                TryToMove(x, 0);
                TryToMove(0, z);
            }
            #endregion

            //check if player can teleport / isnt blocked
            if (CanTeleport(parenthit))
            {
                decalMaterial.SetColor("_EmissiveColor", decalDefaultColor);
                teleportAvailable = true;
            }
            else
            {
                decalMaterial.SetColor("_EmissiveColor", Color.red);
                teleportAvailable = false;
            }

            //Teleport Player to 2D point
            if (Input.GetKeyDown(KeyCode.E) && teleportAvailable)
            {
                TeleportPlayer(teleportPosition, parenthit.point);
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(decalParent.transform.position, decalParent.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ~IgnoreMe))
            {
                if (!isTeleporting)
                {
                    if (hit.collider.gameObject.CompareTag("Start") && IsDecalFullyOnObject() && charController.GetIsGrounded()) //if crosshair over start turn green, else turn white
                    {
                        crosshair.DOColor(Color.green, .35f);
                    }
                    else
                    {
                        crosshair.DOColor(Color.white, .35f);
                    }
                }
            }
        }
    }

    private Tween decalFadetween;
    public void SetDecalOn(bool decalOn)
    {
        if (decalOn)
        {     
            //reset rotation and position and color
            decalParent.transform.localRotation = new Quaternion(0, 0, 0, 0);
            decalParent.transform.localPosition = new Vector3(0, 0, 0);
            decalMaterial.SetColor("_EmissiveColor", decalDefaultColor);

            if (IsDecalFullyOnObject()) //check if the decal will be completly on 2dable surface
            {
                armController.ChangeArmState(ArmController.ArmState.up);
                characterController.enabled = false; //reEnable the chacterController
                hDRPOutline.width.value = 1; //turn on the outline
                isDecalOn = true; //turn the decal on

                decalFadetween.Kill();
                decalFadetween = DOTween.To(() => decalProjector.fadeFactor, xx => decalProjector.fadeFactor = xx, 1, decalFadeInTime);

                xRotation = 0;//reset up down
                yRotation = 0;//reset left right
            }
        }
        else
        {
            armController.ChangeArmState(ArmController.ArmState.down);
            //DOTween.To(() => hDRPOutline.width.value, xx => hDRPOutline.width.value = xx, 0, 1);
            hDRPOutline.width.value = 0; //turn off outline
            if (!isTeleporting) //if the player is not teleporting, turn on the crosshair, else the teleporting animation will fade it back in after once complete
            {
                crosshair.DOColor(Color.white, .75f);
                characterController.enabled = true; //reEnable the chacterController
            }
            isDecalOn = false; //turn of decal
            decalFadetween.Kill();
            decalFadetween = DOTween.To(() => decalProjector.fadeFactor, xx => decalProjector.fadeFactor = xx, 0, decalFadeOutTime); //fade out the decal projector to 0
         
            //decalProjector.enabled = false;

        }
    }


    //checks if the decal renderer will be fully on the 2dable object
    private bool IsDecalFullyOnObject()
    {
        // reset rotation (cam is parent to decal parent so will face same way we are looking)
        //decalParent.transform.localRotation = new Quaternion(0, 0, 0, 0);

        Vector3 leftTop = mainCam.transform.position + transform.right * -(decalDefaultSize.x / 1.5f) + transform.up * (decalDefaultSize.y  / 1.5f);  //1   y - n odd
        Vector3 leftBot = mainCam.transform.position + transform.right * -(decalDefaultSize.x / 1.5f) + transform.up * -(decalDefaultSize.y / 1.5f); //2   y - y even
        Vector3 rightTop = mainCam.transform.position + transform.right * (decalDefaultSize.x / 1.5f) + transform.up * (decalDefaultSize.y  / 1.5f); //3   n - n odd
        Vector3 rightBot = mainCam.transform.position + transform.right * (decalDefaultSize.x / 1.5f) + transform.up * -(decalDefaultSize.y / 1.5f);//4   n - y even
        Vector3 CamForward = mainCam.transform.TransformDirection(Vector3.forward);

        RaycastHit[] hits = new RaycastHit[4];
        Physics.Raycast(leftTop, CamForward, out hits[0], Mathf.Infinity, ~IgnoreMe);
        Physics.Raycast(leftBot, CamForward, out hits[1], Mathf.Infinity, ~IgnoreMe);
        Physics.Raycast(rightTop, CamForward, out hits[2], Mathf.Infinity, ~IgnoreMe);
        Physics.Raycast(rightBot, CamForward, out hits[3], Mathf.Infinity, ~IgnoreMe);

        #region IsDecalFullyOnObject debugRays
        /*
        Debug.DrawRay(leftTop, CamForward * hits[0].distance, Color.magenta, 2);//top left
        Debug.DrawRay(leftBot, CamForward * hits[1].distance, Color.cyan, 2);   //bottom left
        Debug.DrawRay(rightTop, CamForward * hits[2].distance, Color.yellow, 2); //top right
        Debug.DrawRay(rightBot, CamForward * hits[3].distance, Color.blue, 2);   //bot right
        */
        #endregion

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null || !hits[i].collider.CompareTag("Start")) //for each one check if its null or isnt on a start area
            {
                return false;
            }
        }
        return true;
    }

    private void TryToMove(float xInput, float zInput)
    {
        Quaternion currentRotation = decalParent.transform.localRotation;
        Vector3 movementVector = new Vector3(xRotation - zInput, yRotation + xInput, 0);
        decalParent.transform.localRotation = Quaternion.Euler(movementVector);

        //sides
        Vector3 midddleTop = decalParent.transform.position + transform.up * (decalDefaultSize.y / decalOOBCheckOffset);     //1   y - n odd
        Vector3 midddleBot = decalParent.transform.position + transform.up * -(decalDefaultSize.y / decalOOBCheckOffset);    //2   y - y even
        Vector3 midddleRight = decalParent.transform.position + transform.right * (decalDefaultSize.x / decalOOBCheckOffset);//3   n - n odd
        Vector3 midddleLeft = decalParent.transform.position + transform.right * -(decalDefaultSize.x / decalOOBCheckOffset);//4   n - y even

        #region corner vectors
        //corners
        Vector3 topLeft = decalParent.transform.position + transform.up * (decalDefaultSize.y / 1f) + transform.right * -(decalDefaultSize.y / 1f);
        Vector3 bottomLeft = decalParent.transform.position + transform.up * -(decalDefaultSize.y / 1f) + transform.right * -(decalDefaultSize.y / 1f);
        Vector3 topRight = decalParent.transform.position + transform.up * (decalDefaultSize.y / 1f) + transform.right * (decalDefaultSize.y / 1f);
        Vector3 bottomRight = decalParent.transform.position + transform.up * -(decalDefaultSize.y / 1f) + transform.right * (decalDefaultSize.y / 1f);
        #endregion

        if(CheckPointOutOfBounds(midddleTop) || CheckPointOutOfBounds(midddleBot) || CheckPointOutOfBounds(midddleRight) || CheckPointOutOfBounds(midddleLeft))
        {
            decalParent.transform.localRotation = currentRotation; // move back
        }
        else
        {
            xRotation -= zInput; //only add to the rotation after its save to move
            yRotation += xInput;
        }
    }

    //checks if a point is OOB
    private bool CheckPointOutOfBounds(Vector3 raycastStartPoint)
    {
        Vector3 DecalParentForward = decalParent.transform.TransformDirection(Vector3.forward);
        //Debug.DrawRay(raycastStartPoint, DecalParentForward * 10, Color.red, 2);
        bool hasRaycastHit;
        hasRaycastHit = Physics.Raycast(raycastStartPoint, DecalParentForward, out RaycastHit hit, Mathf.Infinity, ~IgnoreMe);
        return !hasRaycastHit || !hit.transform.CompareTag("Start") && !hit.transform.CompareTag("2Dable") && !hit.transform.CompareTag("2DableMoving");//check if its OOB. 
    }

    private Vector3 pos123;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos123, new Vector3(1, 2, 1));
    }

    private bool CanTeleport(RaycastHit decalPos)
    {
        Vector3 teleportPos = decalPos.point + (decalPos.normal * teleportLandingOffset);
        RaycastHit downHit;
        RaycastHit upHit;

        if (Physics.Raycast(teleportPos, Vector3.down , out downHit, 1.7f)) //check if theres not enough room below player
        {
            if (Physics.Raycast(downHit.point, Vector3.up, out upHit, 2f)) //check if theres not enough room above player
            {
                return false;
            }
            else
            {
                
                teleportPosition = downHit.point;
                teleportPosition.y += 1 + characterController.skinWidth; //push the player up so they dont clip into the floor & add skinwidth
                if (CheckIfTeleportBlocked(teleportPosition))
                {
                    return false;
                }
                return true;
            }
        }

        if (Physics.Raycast(teleportPos, Vector3.up, out upHit, .3f)) //check if theres not enough room above player
        {

            if (Physics.Raycast(upHit.point, Vector3.down, out downHit, 2f)) //check if theres not enough room below player
            {
                return false;
            }
            else
            {
                teleportPosition = upHit.point;
                teleportPosition.y -= 1 + characterController.skinWidth;//push the player down so they dont clip into the ceiling
                if (CheckIfTeleportBlocked(teleportPosition))
                {
                    return false;
                }
                return true;
            }
        }

        //if theres room above and below just teleport them

        teleportPosition = teleportPos;
        teleportPosition.y -= mainCam.transform.localPosition.y; //minus the camera offset so that when you teleport you stay at the same level (since teleport goes off center of player not cam)

        if (CheckIfTeleportBlocked(teleportPosition))
        {
            return false;
        }

        return true;
    }

    private bool CheckIfTeleportBlocked(Vector3 tpPos)
    {
        pos123 = tpPos;
        Collider[] hitColliders = Physics.OverlapBox(tpPos, new Vector3(.5f, 1, .5f));
        if (hitColliders.Length != 0)
        {
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.layer == 11)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private void TeleportPlayer(Vector3 teleportPos, Vector3 teleportPosWithoutOffset)
    {
        
        DOTween.To(() => TeleportPostProVolume.weight, xx => TeleportPostProVolume.weight = xx, 1, TeleportPostProVolumeFadeInTime);
        DOTween.To(() => mainCam.fieldOfView, xx => mainCam.fieldOfView = xx, 60, TeleportPostProVolumeFadeInTime);

        Vector3 turnPointPos =  Vector3.Lerp(teleportPosWithoutOffset, mainCam.transform.position, teleportTurnPoint);
        turnPointPos.y = teleportPos.y;

        float teleportDuration = Vector3.Distance(decalParent.transform.position, turnPointPos); //calculate the time it will take to travel x distance at a set speed
        teleportDuration += Vector3.Distance(turnPointPos, teleportPos);

        if(Vector3.Distance(decalParent.transform.position, teleportPosWithoutOffset) < teleportTurnCutoffDistance)
        {
            turnPointPos = teleportPos;
        }

        teleportDuration /= teleportSpeed;

        Vector3[] pathPoints = new[] {turnPointPos, teleportPos};
        Sequence mySequence = DOTween.Sequence();
        mySequence.Insert(0, transform.DOPath(pathPoints, teleportDuration, PathType.CatmullRom));
        //mySequence.Insert(1, transform.DOMove(teleportPos, teleportDuration, false)); 
        mySequence.AppendCallback(EnableCharCont);

        characterController.enabled = false; //disable the characterController so we can teleport the player
        isTeleporting = true;
        SetDecalOn(false); //turn the decal off
        mySequence.Play();

        void EnableCharCont()
        {
            DOTween.To(() => TeleportPostProVolume.weight, xx => TeleportPostProVolume.weight = xx, 0, TeleportPostProVolumeFadeOutTime);
            DOTween.To(() => mainCam.fieldOfView, xx => mainCam.fieldOfView = xx, 80, TeleportPostProVolumeFadeInTime);

            characterController.enabled = true; //reEnable the chacterController
            crosshair.DOColor(Color.white, .75f);
            isTeleporting = false;
        }
    }
}