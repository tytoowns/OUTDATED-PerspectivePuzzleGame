using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour, IInteractable
{
    public UnityEvent activateEvent;
    [SerializeField] private Animator animator;

    public void Start()
    {
        if (activateEvent == null)
            activateEvent = new UnityEvent();
    }

    public void Interact()
    {
        activateEvent.Invoke();
        animator.SetTrigger("SlideDown");
    }
}
