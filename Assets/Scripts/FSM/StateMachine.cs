using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<TOwner> : MonoBehaviour
{
    private State<TOwner> curState = null;

    private TOwner owner; // 状态机拥有者
    
    public void Init(TOwner owner, State<TOwner> initialState)
    {
        this.owner = owner;
        ChangeState(initialState);
    }

    // 负责转换状态
    public void ChangeState(State<TOwner> newState)
    {
        // debug
        if (curState != null)
        {
            Debug.Log(curState.GetType().Name);
        }
        print(string.Format(" {0} => {1} ", curState == null?"N/A":curState.GetType().Name, newState.GetType().Name));
        
        if (curState != null)
        {
            curState.Exit(owner); // 清理
        }

        curState = newState;
        curState.Enter(owner); // 初始化
    }

    public void Update()
    {       
        curState.Update(owner);
    }
}
