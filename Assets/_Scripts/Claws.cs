using UnityEngine;
using System.Collections;

public class Claws : MonoBehaviour 
{
    void RaiseAttack()
    {
        GameBroadcaster.Instance.OnPlayerSwipeAttacked();
    }
}
