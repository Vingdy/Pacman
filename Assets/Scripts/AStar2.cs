using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType
{
    Normal, // 正常
    Obstacle, // 障碍物
    Start, // 起点
    End // 终点
}

public class MapGrid // 排序接口
{
    // 记录坐标
    public int y;
    public int x;

    public int f; // 总消耗
    public int g; // 当前点到起点的消耗
    public int h; // 当前点到终点的消耗

    public GridType type; //格子类型
    public MapGrid fatherNode; // 父节点

    public int CompareTo(object obj) // 排序比较方法 ICloneable 的方法
    {
        MapGrid other = (MapGrid)obj;
        if (this.f < other.f) // 升序
        {
            return -1;
        }

        if (this.f > other.f) // 降序
        {
            return 1;
        }

        return 0;
    }

    internal void Reset()
    {
        f = g = h = 0;
        fatherNode = null;
    }
}

public class AStar2
{
    private int row = 5;
    private int col = 10;
    private MapGrid[,] grids; // 格子数组

    private List<MapGrid> openList; // 开启列表
    private List<MapGrid> closeList; // 关闭列表

    // 开始结束点位置
    private int yStart;
    private int xStart;

    private int yEnd;
    private int xEnd;

    // private List<Vector2> obstacles;

    // 从重点回溯路径到起点（寻路成功后）
    private State<string> fatherNodeLocation;

    public AStar2(List<Vector2> obstacles, bool isObstacle, int row, int col)
    {
        this.grids = new MapGrid[row, col];
        this.row = row;
        this.col = col;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                // 初始化格子，记录格子坐标
                grids[i, j] = new MapGrid();
                grids[i, j].y = i;
                grids[i, j].x = j;
                grids[i, j].type = isObstacle ? GridType.Normal : GridType.Obstacle;
            }
        }

        // 生成障碍物
        foreach (var pos in obstacles)
        {
            // Debug.Log(pos);
            grids[(int)pos.y, (int)pos.x].type = isObstacle ? GridType.Obstacle : GridType.Normal;
        }

        openList = new List<MapGrid>();
        closeList = new List<MapGrid>();
    }

    public List<Vector2> Find(Vector2 start, Vector2 end)
    {
        // 如果起点终点是障碍则返回空
        if (grids[(int)start.y, (int)start.x].type == GridType.Obstacle ||
            grids[(int)end.y, (int)end.x].type == GridType.Obstacle)
        {
            return null;
        }

        // 初始化起点和终点
        yStart = (int)start.y;
        xStart = (int)start.x;

        yEnd = (int)end.y;
        xEnd = (int)end.x;

        // 重置所有格子的数据
        foreach (var grid in grids)
        {
            grid.Reset();
        }

        // 清空 openList 和 closeList
        openList.Clear();
        closeList.Clear();

        // 将起点加入到 openList
        openList.Add(grids[yStart, xStart]);

        // 开始寻路
        while (NextStep() == 0)
        {
        }

        // 寻路完成后，从终点回溯路径到起点
        List<Vector2> path = new List<Vector2>();
        MapGrid currentNode = grids[yEnd, xEnd];
        while (currentNode != null)
        {
            path.Add(new Vector2(currentNode.x, currentNode.y));
            currentNode = currentNode.fatherNode;
        }

        path.Reverse(); // 反转路径，使其从起点到终点

        return path;
    }

    int NextStep()
    {
        if (openList.Count == 0) // 没有可走的点
        {
            Debug.Log("No path found");
            return -1;
        }

        // 排序 openList，以确保 F 值最小的格子在最前面
        openList.Sort((a, b) => a.f.CompareTo(b.f));

        // 获取 F 值最小的格子
        MapGrid currentGrid = openList[0];
        openList.RemoveAt(0);
        closeList.Add(currentGrid); // 将当前格子加入关闭列表

        if (currentGrid.y == yEnd && currentGrid.x == xEnd) // 如果当前格子是终点
        {
            Debug.Log("Path found");
            return 1;
        }

        // 获取当前格子周围的可走格子
        List<MapGrid> neighbors = GetNeighbors(currentGrid);

        // 遍历周围的格子
        foreach (MapGrid neighbor in neighbors)
        {
            if (closeList.Contains(neighbor)) // 如果该格子已经在关闭列表中，跳过
                continue;

            // 计算该格子的 G 值
            int tentativeGScore = currentGrid.g + 1;

            // 如果该格子不在 openList 中，或者新的 G 值更小
            if (!openList.Contains(neighbor) || tentativeGScore < neighbor.g)
            {
                // 更新该格子的父节点和 G 值
                neighbor.fatherNode = currentGrid;
                neighbor.g = tentativeGScore;
                neighbor.h = Mathf.Abs(neighbor.y - yEnd) + Mathf.Abs(neighbor.x - xEnd); // 曼哈顿距离作为启发函数 H
                neighbor.f = neighbor.g + neighbor.h;

                if (!openList.Contains(neighbor)) // 如果该格子不在 openList 中，则加入 openList
                    openList.Add(neighbor);
            }
        }

        return 0;
    }

    List<MapGrid> GetNeighbors(MapGrid grid)
    {
        List<MapGrid> neighbors = new List<MapGrid>();

        // 上下左右四个方向
        int[,] directions = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int y = grid.y + directions[i, 0];
            int x = grid.x + directions[i, 1];

            if (y >= 0 && y < row && x >= 0 && x < col && grids[y, x].type != GridType.Obstacle)
            {
                neighbors.Add(grids[y, x]);
            }
        }

        // 八个方向
        // for (int i = -1; i <= 1; i++)
        // {
        //     for (int j = -1; j <= 1; j++)
        //     {
        //         if (i == 0 && j == 0) // 忽略当前格子自身
        //             continue;
        //
        //         int y = grid.y + i;
        //         int x = grid.x + j;
        //
        //         if (y >= 0 && y < row && x >= 0 && x < col && grids[y, x].type != GridType.Obstacle)
        //         {
        //             neighbors.Add(grids[y, x]);
        //         }
        //     }
        // }

        return neighbors;
    }
}