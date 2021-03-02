using System.Collections;
using System.Collections.Generic;
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
        {
            CopyRotation();
        }
        else
        {
            //ResetRotation(); // only should reset rotation once not constantly?
        }
    }

    private void ResetRotation()
    {
        //playerArm.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void CopyRotation()
    {
        //playerArm.transform.rotation = Quaternion.Euler( 0, decalParent.transform.localEulerAngles.y, -decalParent.transform.localEulerAngles.x);
    }

    public void ChangeArmState(bool armDown)
    {
        if(armDown)
        {
            if(armWasUp)
            animator.SetTrigger("GoRest");
            armWasUp = false;
        }   
        else
        {
            armWasUp = true;
            animator.SetTrigger("GoUp");
        }
    }
}
