using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class ArmController : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private GameObject playerArm;
    [SerializeField] private GameObject decalParent;
    private bool armWasUp;
    

    // Update is called once per frame
    void Update()
    {
        if (armWasUp)
            CopyRotation();
    }

    private void ResetRotation()
    {
        //playerArm.transform.rotation = Quaternion.Euler(0, 0, 0);
        //lerp to rotation
        //playerArm.transform.DORotate(decalParent.transform.rotation.eulerAngles, 1);
        //Vector3 resetPoint = new Vector3(Camera.main.transform.eulerAngles.z, Camera.main.transform.eulerAngles.y - 90, -Camera.main.transform.eulerAngles.x);
        playerArm.transform.DOLocalRotate(new Vector3(0.087f, 3.187f, -7.428f), .25f, RotateMode.Fast);

    }

    private void CopyRotation()
    {
        // to do - limit rotation
        playerArm.transform.rotation = Quaternion.Euler(decalParent.transform.eulerAngles.z, decalParent.transform.eulerAngles.y - 90, -decalParent.transform.eulerAngles.x);
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
            armWasUp = true;
            animator.SetTrigger("GoUp");
        }
    }
}
