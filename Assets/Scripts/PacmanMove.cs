using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

// 针对转向不顺的问题，考虑采用最后记录的按键作为检测值

public class PacmanMove : MonoBehaviour
{
    private Rigidbody2D rg2d;
    
    public float Speed = 4f;

    public Vector2 dest;

    private Vector2 curDir = Vector2.right;

    private Vector2 lastButtomDir = Vector2.right;
    // Start is called before the first frame update
    void Start()
    {
        // RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + Vector2.right, 1 << LayerMask.NameToLayer("Map"));
        // Debug.Log(Cango(Vector2.right));
        dest = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() 
    {
        if (GameContorller.instance.IsWin())
        {
            GameContorller.instance.OnGameWin();
            return;
        }
        if (GameContorller.instance.gameOver)
        {
            gameObject.SetActive(false);
            return;
        }
        // 1.控制角色朝着目标方向移动
        Vector2 p = Vector2.MoveTowards(transform.position, dest, Speed*Time.fixedDeltaTime);
        gameObject.GetComponent<Rigidbody2D>().MovePosition(p);
        // 2.如果角色移动到目标点了，则检测玩家按键，通过上下左右控制角色移动的方向（目标）

        var tmp = (Vector2)transform.position;

        if (tmp == dest)
        {
            if (!Cango(curDir))
            {
                curDir = Vector2.zero;
            } 
            
            if (Input.GetKey(KeyCode.UpArrow) )
            {
                lastButtomDir = Vector2.up;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                lastButtomDir = Vector2.down;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                lastButtomDir = Vector2.left;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                lastButtomDir = Vector2.right;
            }

            if (Cango(lastButtomDir))
            {
                curDir = lastButtomDir;
            }

            GetComponent<Animator>().SetFloat("DirX", curDir.x);
            GetComponent<Animator>().SetFloat("DirY", curDir.y);
            
            dest += curDir;
        }
    }

    // 使用射线判断，需要提前设置层级
    bool Cango(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + dir, 1 << LayerMask.NameToLayer("Map"));
        Debug.DrawLine(transform.position, (Vector2)transform.position + dir, Color.red);

        return hit != true;
    }
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameContorller.instance.isMonsters(collision.gameObject.tag))
        {
            GameContorller.instance.OnMeetMonster(this.gameObject, collision.gameObject);
            
            GetComponent<Animator>().SetFloat("DirX", 0);
            GetComponent<Animator>().SetFloat("DirY", 0);
        } else if (collision.gameObject.tag == "PowerUp")
        {
            GameContorller.instance.OnMeetPowerUp(collision.gameObject);
        }
    }
}
