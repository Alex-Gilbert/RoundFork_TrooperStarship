using UnityEngine;
using System.Collections;

public class Capsule : MonoBehaviour 
{
    public bool empty;
    private Animator anim;

    bool broken = false;
    int Health = 4;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnInteract()
    {
        if (!broken)
        {
            Health--;
            if (Health <= 0)
            {
                anim.SetTrigger("Break");
                GameBroadcaster.Instance.OnCapsuleBroken(gameObject, empty ? 0 : 1);
                broken = true;
            }
        }
    }
}
