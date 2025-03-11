using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

/// <summary>
/// 网格信息类
/// 定义地图上单个网格的属性和行为
/// </summary>
public class GridInfo
{
    /// <summary>
    /// 网格预制体路径
    /// </summary>
    public string prefabPath;

    /// <summary>
    /// 地面单位是否可通过此网格
    /// </summary>
    public bool groundCanPass;

    /// <summary>
    /// 飞行单位是否可通过此网格
    /// </summary>
    public bool flyCanPass;

    /// <summary>
    /// 创建网格信息
    /// </summary>
    /// <param name="prefabPath">预制体路径</param>
    /// <param name="characterCanPass">地面单位是否可通过</param>
    /// <param name="bulletCanPass">飞行单位是否可通过</param>
    public GridInfo(string prefabPath, bool characterCanPass = true, bool bulletCanPass = true)
    {
        this.prefabPath = prefabPath;
        this.groundCanPass = characterCanPass;
        this.flyCanPass = bulletCanPass;
    }

    /// <summary>
    /// 空网格，表示地图边界外的区域
    /// 默认不可通行
    /// </summary>
    public static GridInfo VoidGrid { get; } = new GridInfo("", false, false);
}

/// <summary>
/// 地图信息类
/// 管理整个地图的网格布局和寻路相关功能
/// </summary>
public class MapInfo
{
    /// <summary>
    /// 网格二维数组，表示地图布局
    /// </summary>
    public GridInfo[,] grid;

    /// <summary>
    /// 单个网格的大小（宽度和高度）
    /// </summary>
    public Vector2 gridSize { get; }

    /// <summary>
    /// 地图边界矩形
    /// </summary>
    public Rect border { get; }

    /// <summary>
    /// 创建地图信息
    /// </summary>
    /// <param name="map">网格二维数组</param>
    /// <param name="gridSize">网格大小</param>
    public MapInfo(GridInfo[,] map, Vector2 gridSize)
    {
        this.grid = map;
        // 确保网格大小至少为0.1
        this.gridSize = new Vector2(Mathf.Max(0.1f, gridSize.x), Mathf.Max(0.1f, gridSize.y));
        // 计算地图边界
        this.border = new Rect(
            -gridSize.x / 2.00f,                  // 左边界
            -gridSize.y / 2.00f,                  // 下边界
            gridSize.x * MapWidth(),              // 宽度
            gridSize.y * MapHeight()              // 高度
        );
    }

    /// <summary>
    /// 获取地图宽度（网格数量）
    /// </summary>
    /// <returns>地图宽度</returns>
    public int MapWidth()
    {
        return grid.GetLength(0);
    }

    /// <summary>
    /// 获取地图高度（网格数量）
    /// </summary>
    /// <returns>地图高度</returns>
    public int MapHeight()
    {
        return grid.GetLength(1);
    }

    /// <summary>
    /// 根据世界坐标获取网格信息
    /// </summary>
    /// <param name="pos">世界坐标</param>
    /// <returns>对应位置的网格信息</returns>
    public GridInfo GetGridInPosition(Vector3 pos)
    {
        // 获取网格坐标
        Vector2Int gridPos = GetGridPosByMeter(pos.x, pos.z);
        
        // 检查是否在地图范围内
        if (gridPos.x < 0 || gridPos.x >= MapWidth() || gridPos.y < 0 || gridPos.y >= MapHeight())
            return GridInfo.VoidGrid;
            
        return grid[gridPos.x, gridPos.y];
    }

    /// <summary>
    /// 将世界坐标转换为网格坐标
    /// </summary>
    /// <param name="x">世界坐标X</param>
    /// <param name="z">世界坐标Z</param>
    /// <returns>网格坐标</returns>
    public Vector2Int GetGridPosByMeter(float x, float z)
    {
        return new Vector2Int(
            // 四舍五入计算网格位置
            Mathf.RoundToInt(x / gridSize.x),
            Mathf.RoundToInt(z / gridSize.y)
        );
    }

    /// <summary>
    /// 检查指定网格是否可通行
    /// </summary>
    /// <param name="gridX">网格X坐标</param>
    /// <param name="gridY">网格Y坐标</param>
    /// <param name="moveType">移动类型</param>
    /// <param name="ignoreBorder">是否忽略边界检查</param>
    /// <returns>是否可通行</returns>
    public bool CanGridPasses(int gridX, int gridY, MoveType moveType, bool ignoreBorder)
    {
        // 检查边界
        if (gridX < 0 || gridX >= MapWidth() || gridY < 0 || gridY >= MapHeight())
            return ignoreBorder;
            
        // 根据移动类型判断通行性
        switch (moveType)
        {
            case MoveType.ground: return grid[gridX, gridY].groundCanPass;
            case MoveType.fly: return grid[gridX, gridY].flyCanPass;
        }
        return false;
    }

    /// <summary>
    /// 检查单位是否可以放置在指定位置
    /// </summary>
    /// <param name="pos">世界坐标</param>
    /// <param name="radius">单位半径</param>
    /// <param name="moveType">移动类型</param>
    /// <returns>是否可以放置</returns>
    public bool CanUnitPlacedHere(Vector3 pos, float radius, MoveType moveType)
    {
        // 计算单位占据的网格范围
        Vector2Int topLeft = GetGridPosByMeter(pos.x - radius, pos.z - radius);
        Vector2Int bottomRight = GetGridPosByMeter(pos.x + radius, pos.z + radius);
        
        // 收集所有碰撞网格的矩形
        List<Rect> collisionRects = new List<Rect>();
        for (int i = topLeft.x; i <= bottomRight.x; i++)
        {
            for (int j = topLeft.y; j <= bottomRight.y; j++)
            {
                // 如果网格不可通行，添加到碰撞矩形列表
                if (!CanGridPasses(i, j, moveType, false))
                {
                    collisionRects.Add(new Rect(
                        (i - 0.5f) * gridSize.x,    // 左边界
                        (j - 0.5f) * gridSize.y,    // 下边界
                        gridSize.x,                  // 宽度
                        gridSize.y                   // 高度
                    ));
                }
            }
        }
        
        // 检查圆形单位是否与任何矩形碰撞
        return !Utils.CircleHitRects(new Vector2(pos.x, pos.z), radius, collisionRects);
    }

    /// <summary>
    /// 在指定范围内获取角色的随机有效位置
    /// </summary>
    /// <param name="range">范围矩形</param>
    /// <param name="chaRadius">角色半径</param>
    /// <param name="moveType">移动类型</param>
    /// <returns>随机有效位置</returns>
    public Vector3 GetRandomPosForCharacter(RectInt range, float chaRadius = 0.00f, MoveType moveType = MoveType.ground)
    {
        // 收集所有可能的有效位置
        List<Vector3> validPositions = new List<Vector3>();
        
        // 遍历指定范围内的所有网格
        for (var i = range.x; i < range.x + range.width; i++)
        {
            for (var j = range.y; j < range.y + range.height; j++)
            {
                // 计算世界坐标
                Vector3 candidatePos = new Vector3(
                    i * gridSize.x,
                    0,
                    j * gridSize.y
                );
                
                // 检查位置是否有效
                if (CanUnitPlacedHere(candidatePos, chaRadius, moveType))
                {
                    validPositions.Add(candidatePos);
                }
            }
        }
        
        // 从有效位置中随机选择一个
        return validPositions[Mathf.FloorToInt(Random.Range(0, validPositions.Count))];
    }

    /// <summary>
    /// 获取垂直方向上最近的障碍物位置
    /// </summary>
    /// <param name="pivot">起始位置</param>
    /// <param name="dir">方向（正数为右，负数为左）</param>
    /// <param name="radius">单位半径</param>
    /// <param name="moveType">移动类型</param>
    /// <param name="ignoreBorder">是否忽略边界</param>
    /// <returns>可移动的最远X坐标</returns>
    public float GetNearestVerticalBlock(Vector3 pivot, float dir, float radius, MoveType moveType, bool ignoreBorder)
    {
        // 如果方向为0，不移动
        if (dir == 0) return pivot.x;
        
        // 确定方向符号
        int directionSign = dir > 0 ? 1 : -1;
        
        // 默认可移动到的位置
        float bestX = pivot.x + dir;
        
        // 计算需要检查的网格数量
        int searchWidth = Mathf.CeilToInt((Mathf.Abs(dir) + radius) / gridSize.x + 2);
        
        // 获取起始网格坐标
        Vector2Int gridPos = GetGridPosByMeter(pivot.x, pivot.z);
        
        // 沿方向搜索障碍物
        for (var i = 0; i < searchWidth; i++)
        {
            int currentGridX = gridPos.x + directionSign * i;
            
            // 如果遇到不可通行网格
            if (!this.CanGridPasses(currentGridX, gridPos.y, moveType, ignoreBorder))
            {
                // 计算障碍物的确切位置
                float wallX = (currentGridX - directionSign * 0.5f) * gridSize.x - directionSign * radius;
                
                // 根据方向返回合适的位置
                if (directionSign > 0)
                {
                    return Mathf.Min(wallX, bestX);
                }
                else
                {
                    return Mathf.Max(wallX, bestX);
                }
            }
        }
        
        return bestX;
    }

    /// <summary>
    /// 获取水平方向上最近的障碍物位置
    /// </summary>
    /// <param name="pivot">起始位置</param>
    /// <param name="dir">方向（正数为上，负数为下）</param>
    /// <param name="radius">单位半径</param>
    /// <param name="moveType">移动类型</param>
    /// <param name="ignoreBorder">是否忽略边界</param>
    /// <returns>可移动的最远Z坐标</returns>
    public float GetNearestHorizontalBlock(Vector3 pivot, float dir, float radius, MoveType moveType, bool ignoreBorder)
    {
        // 如果方向为0，不移动
        if (dir == 0) return pivot.z;
        
        // 确定方向符号
        int directionSign = dir > 0 ? 1 : -1;
        
        // 默认可移动到的位置
        float bestZ = pivot.z + dir;
        
        // 计算需要检查的网格数量
        int searchHeight = Mathf.CeilToInt((Mathf.Abs(dir) + radius) / gridSize.y + 2);
        
        // 获取起始网格坐标
        Vector2Int gridPos = GetGridPosByMeter(pivot.x, pivot.z);
        
        // 沿方向搜索障碍物
        for (var i = 0; i < searchHeight; i++)
        {
            int currentGridY = gridPos.y + directionSign * i;
            
            // 如果遇到不可通行网格
            if (!this.CanGridPasses(gridPos.x, currentGridY, moveType, ignoreBorder))
            {
                // 计算障碍物的确切位置
                float wallZ = (currentGridY - directionSign * 0.5f) * gridSize.y - directionSign * radius;
                
                // 根据方向返回合适的位置
                if (directionSign > 0)
                {
                    return Mathf.Min(wallZ, bestZ);
                }
                else
                {
                    return Mathf.Max(wallZ, bestZ);
                }
            }
        }
        
        return bestZ;
    }

    /// <summary>
    /// 修正目标位置，考虑障碍物和可移动范围
    /// </summary>
    /// <param name="pivot">起始位置</param>
    /// <param name="radius">单位半径</param>
    /// <param name="targetPos">目标位置</param>
    /// <param name="moveType">移动类型</param>
    /// <param name="ignoreBorder">是否忽略边界</param>
    /// <returns>修正后的目标位置信息</returns>
    public MapTargetPosInfo FixTargetPosition(Vector3 pivot, float radius, Vector3 targetPos, MoveType moveType, bool ignoreBorder)
    {
        // 计算方向向量
        float xDirection = targetPos.x - pivot.x;
        float zDirection = targetPos.z - pivot.z;
        
        // 获取考虑障碍物后的最远可移动坐标
        float adjustedX = GetNearestVerticalBlock(pivot, xDirection, radius, moveType, ignoreBorder);
        float adjustedZ = GetNearestHorizontalBlock(pivot, zDirection, radius, moveType, ignoreBorder);

        // 判断是否遇到障碍物（精确到毫米级）
        bool hasObstacle = (
            Mathf.RoundToInt(adjustedX * 1000) != Mathf.RoundToInt(targetPos.x * 1000) ||
            Mathf.RoundToInt(adjustedZ * 1000) != Mathf.RoundToInt(targetPos.z * 1000)
        );
        
        // 返回修正后的位置信息
        return new MapTargetPosInfo(
            hasObstacle, 
            new Vector3(adjustedX, targetPos.y, adjustedZ)
        );
    }
}

/// <summary>
/// 地图目标位置信息结构体
/// 包含修正后的目标位置和障碍物状态
/// </summary>
public struct MapTargetPosInfo
{
    /// <summary>
    /// 是否会碰到阻碍
    /// </summary>
    public bool obstacle;

    /// <summary>
    /// 建议移动到的位置
    /// </summary>
    public Vector3 suggestPos;

    /// <summary>
    /// 创建地图目标位置信息
    /// </summary>
    /// <param name="obstacle">是否碰到障碍物</param>
    /// <param name="suggestPos">建议位置</param>
    public MapTargetPosInfo(bool obstacle, Vector3 suggestPos)
    {
        this.obstacle = obstacle;
        this.suggestPos = suggestPos;
    }
}
