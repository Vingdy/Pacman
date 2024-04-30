using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<TOwner> : MonoBehaviour
{
    public virtual void Enter(TOwner e)
    {
        
    }

    public virtual void Update(TOwner e)
    {
        
    }

    public virtual void Exit(TOwner e)
    {
        
    }
}
