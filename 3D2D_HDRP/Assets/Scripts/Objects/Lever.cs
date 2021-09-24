using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour, IInteractable
{
    public UnityEvent activateEvent;
    [SerializeField] private Animator animator;
    [SerializeField] private float interactTime = 0f;
    [SerializeField] private bool reusable = false;
    private bool canBeInteractedWith;

    public void Start()
    {
        canBeInteractedWith = true;
        if (activateEvent == null)
            activateEvent = new UnityEvent();
    }

    public void Interact()
    {
        activateEvent.Invoke();
        animator.SetTrigger("SlideDown");
        if(!reusable)
        {
            canBeInteractedWith = false;
        }
    }

    public float GetInteractTime()
    {
        return interactTime;
    }
    public bool CanBeInteractedWith()
    {
        return canBeInteractedWith;
    }
}
