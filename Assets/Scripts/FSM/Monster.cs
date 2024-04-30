using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Blinky 更喜欢追击，有更大的追踪范围，标准速度
// Clyde 追踪范围较小，但追踪到玩家后会加速
// Inky 会全图追踪玩家，速度较慢
// Pinky 不会追击，仅会巡逻，速度较快
// Astar 算法存在问题，Finish
public class Monster : MonoBehaviour
{
    StateMachine<Monster> StateMachine = new StateMachine<Monster>();


    public List<Vector2> leaveHomePath;
    
    public float Speed = 4f;

    public AStar2 finder;
    public int row = 29;
    public int col = 26;

    public bool canseePlayer = false;
    public Vector2 lastSeenPlayer = Vector2.zero;
    
    bool Cango(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + dir, 1 << LayerMask.NameToLayer("Map"));
        
        return hit != true;
    }
    class PatrolState : State<Monster>
    {
        private Vector2 dest;
        private Vector2 curDir; // 当前移动方向，用来防止反方向走

        private Vector2[] dirs = new Vector2[] // 所有可移动方向
        {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right
        };

        public override void Enter(Monster e)
        {
            dest = e.transform.position;
            curDir = Vector2.zero;
        }
        
        public override void Update(Monster e)
        {
            Vector2 p = Vector2.MoveTowards(e.transform.position, dest, e.Speed*Time.fixedDeltaTime);
            e.GetComponent<Rigidbody2D>().MovePosition(p);

            // Debug.Log((Vector2)e.transform.position == dest);
            if ((Vector2)e.transform.position == dest)
            {
                // 转换到追踪
                if (e.canseePlayer && e.gameObject.tag != "Pinky")
                {
                    Vector2 start = dest + GameContorller.instance.leftBottom;
                    Vector2 end = e.lastSeenPlayer + GameContorller.instance.leftBottom;
                    
                    List<Vector2> path = new List<Vector2>();
                    path = e.finder.Find(start, end);
                    // 转换坐标从地图坐标到场景坐标
                    if (path != null)
                    {
                        for (int i = 0; i < path.Count; i++)
                        {
                            path[i] -= GameContorller.instance.leftBottom;
                        }
                        e.StateMachine.ChangeState(new SeekState(path));
                        if (e.gameObject.tag == "Clyde")
                        {
                            e.Speed += 2f;
                        }
                        return;
                    }
                }
                
                List<Vector2> walkable = new List<Vector2>();
                for (int i = 0 ; i < dirs.Length; i++)
                {
                    if (dirs[i] == -curDir)
                    {
                        continue;
                    }
                    if (e.Cango(dirs[i]))
                    {
                        walkable.Add(dirs[i]);
                    }
                }

                int index = Random.Range(0, walkable.Count);
                curDir = walkable[index];
                
                e.GetComponent<Animator>().SetFloat("DirX", curDir.x);
                e.GetComponent<Animator>().SetFloat("DirY", curDir.y);

                dest += curDir;
            }
        }
    }
    class WaypointState : State<Monster>
    {
        private List<Vector2> path; // 路径
        private int index; // 当前往哪个路径点走
        public WaypointState(List<Vector2> path)
        {
            this.path = path;
            this.index = 0;
        }

        public override void Update(Monster e)
        {
            // 绘图 Debug
            for (int i = index; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i+1]);
            }
            
            // Debug.Log(index);
            Vector2 p = Vector2.MoveTowards(e.transform.position, path[index], e.Speed*Time.fixedDeltaTime);
            e.GetComponent<Rigidbody2D>().MovePosition(p);

            if ((Vector2)e.transform.position == path[index])
            {
                index++;

                if (index == path.Count)
                {
                    // FSM -> 巡逻
                    print("reach");
                    if (e.gameObject.tag == "Clyde")
                    {
                        e.Speed -= 2f;
                    }
                    e.StateMachine.ChangeState(new PatrolState());
                }
                else
                {
                    Vector2 dir = path[index] - path[index - 1];
                    e.GetComponent<Animator>().SetFloat("DirX", dir.x);
                    e.GetComponent<Animator>().SetFloat("DirY", dir.y);
                }
            }
        }
    }

    class LeaveHomeState : WaypointState
    {
        public LeaveHomeState(List<Vector2> path)
            : base(path)
        {

        }
    }

    class SeekState : WaypointState
    {
        public SeekState(List<Vector2> path)
            : base(path)
        {

        }

        public override void Update(Monster e)
        {
            // 转换到逃跑状态
            if (GameContorller.instance.isPlayerPowerUp)
            {
                Vector2 corner = GameContorller.instance.GetEscapeCorner((Vector2)e.gameObject.transform.position)+GameContorller.instance.leftBottom;
                Vector2 start = (Vector2)e.transform.position + GameContorller.instance.leftBottom;
                List<Vector2> path = e.finder.Find(start, corner);
                Debug.Log(start);
                Debug.Log(corner);
                foreach (var p in path)
                {
                    Debug.Log(p);
                }
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(path[i], path[i+1]);
                }
                if (path != null)
                {
                    Debug.Log("Find Escape Path Success");
                    for (int i = 0; i < path.Count; i++)
                    {
                        path[i] -= GameContorller.instance.leftBottom;
                    }
                    e.StateMachine.ChangeState(new EscapeState(path));
                }
                else
                {
                    Debug.LogWarning("Find Escape Path Error");
                }
            }
            base.Update(e);
            // else
            // {
            //     e.StateMachine.ChangeState(new PatrolState());
            // }
        }
    }

    class EscapeState : WaypointState
    {
        public EscapeState(List<Vector2> path)
            : base(path)
        {

        }
    }
    void Start()
    {
        List<Vector2> walkable = GameContorller.instance.GenWalkableList();

        finder = new AStar2(walkable, false, row, col);
        
        StateMachine.Init(this, new LeaveHomeState(leaveHomePath));
    }
    
    void FixedUpdate()
    {
        if (GameContorller.instance.gameOver)
            return;

        if (GameContorller.instance.isPlayerPowerUp)
        {
            GetComponent<SpriteRenderer>().material.color = new Color(0.2f, 0.2f, 0.8f,1.0f);
        }
        else
        {
            GetComponent<SpriteRenderer>().material.color = Color.white;
        }
        
        StateMachine.Update();
    }


}
