using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class ArmController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private GameObject playerArm;
    [SerializeField] private GameObject decalParent;
    [SerializeField] private float MaxVerticalRot = 5f;
    [SerializeField] private float MinVerticalRot = 5f;
    [SerializeField] private float MaxHorizontalRot = 5f;
    [SerializeField] private float MinHorizontalRot = 5f;

    private bool armWasUp;
    private Vector3 defaultRotation;
    private Vector3 startingRotation;

    private void OnEnable()
    {
        //set default rotation
        defaultRotation = new Vector3 (playerArm.transform.localEulerAngles.x, playerArm.transform.localEulerAngles.y, playerArm.transform.localEulerAngles.z);
    }

    void Update()
    {
        if (armWasUp)
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

        if(CheckIfCanRotate(currentRotation))
        {
            playerArm.transform.rotation = Quaternion.Euler(currentRotation.z, currentRotation.y - 90, -currentRotation.x);
        }
    }

    bool CheckIfCanRotate(Vector3 currentRot)
    {
        //compare difference to starting Rot
        Vector3 difference = new Vector3(Mathf.DeltaAngle(currentRot.x, startingRotation.x), Mathf.DeltaAngle(currentRot.y, startingRotation.y), Mathf.DeltaAngle(currentRot.z, startingRotation.z));

        print("CurrentRot " + currentRot);
        print("StartingRot " + startingRotation);
        print("Difference " + difference);

        if (difference.x < MaxVerticalRot && difference.x > MinVerticalRot)//if the difference is less than the Max
        {
            if (difference.y < MaxHorizontalRot && difference.y > MinHorizontalRot)//and if the differnce is bigger than the Min
            {
                return true;
            }
        }
        return false;
    }

    public void ChangeArmState(bool armDown)
    {
        if(armDown)
        {
            if(armWasUp)
            animator.SetTrigger("GoRest");
            armWasUp = false;
            ResetRotation();
        }   
        else
        {
            startingRotation = new Vector3(decalParent.transform.eulerAngles.x, decalParent.transform.eulerAngles.y, decalParent.transform.eulerAngles.z);
            armWasUp = true;
            animator.SetTrigger("GoUp");
        }
    }
}
