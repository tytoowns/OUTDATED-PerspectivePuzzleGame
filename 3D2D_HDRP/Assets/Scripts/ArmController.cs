using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class ArmController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private GameObject playerArm;
    [SerializeField] private GameObject decalParent;
    [SerializeField] private float maxVerticalRot = 5f;
    [SerializeField] private float minVerticalRot = 5f;
    [SerializeField] private float maxHorizontalRot = 5f;
    [SerializeField] private float minHorizontalRot = 5f;
    [SerializeField] private float movementRatio = .5f;

    private Vector3 defaultRotation;
    private Vector3 startingRotation;

    public enum ArmState {up, down};
    private ArmState armState;

    private void OnEnable()
    {
        armState = ArmState.down;
        //set default rotation
        defaultRotation = new Vector3 (playerArm.transform.localEulerAngles.x, playerArm.transform.localEulerAngles.y, playerArm.transform.localEulerAngles.z);
    }

    void Update()
    {
        if (armState == ArmState.up)
            CopyRotation();
    }

    private void ResetRotation()
    {
        playerArm.transform.DOLocalRotate(defaultRotation, .25f, RotateMode.Fast);
    }

    private void CopyRotation()
    {
        //get current rot
        Vector3 currentRotation = decalParent.transform.eulerAngles;

        Vector3 xRotCheck = currentRotation, yRotCheck = currentRotation;
        xRotCheck.y = startingRotation.y;
        yRotCheck.x = startingRotation.x;
        
        if(CheckIfCanRotate(currentRotation))
        {
            //playerArm.transform.rotation = Quaternion.Euler(currentRotation.z, currentRotation.y - 90, -currentRotation.x);
            Vector3 forwardVector = -decalParent.transform.right;
            playerArm.transform.rotation = Quaternion.LookRotation(forwardVector, decalParent.transform.up);
        }
        /*
        if(CheckIfCanRotate(xRotCheck))
        {
            Vector3 forwardVector = -decalParent.transform.right;
            forwardVector.y = startingRotation.x;
            playerArm.transform.localRotation = Quaternion.LookRotation(forwardVector, decalParent.transform.up);
            //playerArm.transform.rotation = Quaternion.LookRotation(forwardVector, decalParent.transform.up);
        }
        if(CheckIfCanRotate(yRotCheck))
        {
            //Vector3 forwardVector = -decalParent.transform.right;
            //forwardVector.x = startingRotation.x;
            //playerArm.transform.localRotation = Quaternion.LookRotation(forwardVector, decalParent.transform.up);

            //playerArm.transform.rotation = Quaternion.LookRotation(forwardVector, decalParent.transform.up);
        }
        */
    }

    bool CheckIfCanRotate(Vector3 currentRot)
    {
        //compare difference to starting Rot
        Vector3 difference = new Vector3(Mathf.DeltaAngle(currentRot.x, startingRotation.x), Mathf.DeltaAngle(currentRot.y, startingRotation.y), Mathf.DeltaAngle(currentRot.z, startingRotation.z));

        if (difference.x < maxVerticalRot && difference.x > minVerticalRot)//if the difference is less than the Max
        {
            if (difference.y < maxHorizontalRot && difference.y > minHorizontalRot)//and if the differnce is bigger than the Min
            {
                return true;
            }
        }
        return false;
    }

    public void ChangeArmState(ArmState newState)
    {
        if(newState == ArmState.down)
        {
            if(armState == ArmState.up)
                animator.SetTrigger("GoRest");
            ResetRotation();
        }   
        else
        {
            startingRotation = new Vector3(decalParent.transform.eulerAngles.x, decalParent.transform.eulerAngles.y, decalParent.transform.eulerAngles.z);
            animator.SetTrigger("GoUp");
        }
        armState = newState;
    }
}
