using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 线段类：表示2D空间中的线段
/// 提供线段相关的计算功能，如端点获取和相交检测
/// </summary>
public class Line
{
    /// <summary>
    /// 线段的两个端点
    /// point[0] 和 point[1] 分别表示线段的起点和终点
    /// </summary>
    public Vector2[] point;

    /// <summary>
    /// 创建线段对象
    /// </summary>
    /// <param name="point">线段的端点数组，默认为null创建零向量线段</param>
    public Line(Vector2[] point = null)
    {
        this.point = new Vector2[2] { Vector2.zero, Vector2.zero };
        if (point != null)
        {
            if (point.Length > 0)
                this.point[0] = point[0];
            if (point.Length > 1)
                this.point[1] = point[1];
        }
    }

    /// <summary>
    /// 获取线段的左侧端点（X坐标较小的点）
    /// </summary>
    /// <returns>左侧端点坐标</returns>
    public Vector2 LeftPoint()
    {
        return this.point[0].x <= this.point[1].x ? this.point[0] : this.point[1];
    }

    /// <summary>
    /// 获取线段的右侧端点（X坐标较大的点）
    /// </summary>
    /// <returns>右侧端点坐标</returns>
    public Vector2 RightPoint()
    {
        return this.point[0].x > this.point[1].x ? this.point[0] : this.point[1];
    }

    /// <summary>
    /// 获取线段的顶部端点（Y坐标较小的点）
    /// 注意：Unity中Y轴向上为正，所以顶部是Y值较小的点
    /// </summary>
    /// <returns>顶部端点坐标</returns>
    public Vector2 TopPoint()
    {
        return this.point[0].y <= this.point[1].y ? this.point[0] : this.point[1];
    }

    /// <summary>
    /// 获取线段的底部端点（Y坐标较大的点）
    /// 注意：Unity中Y轴向上为正，所以底部是Y值较大的点
    /// </summary>
    /// <returns>底部端点坐标</returns>
    public Vector2 BottomPoint()
    {
        return this.point[0].y > this.point[1].y ? this.point[0] : this.point[1];
    }

    /// <summary>
    /// 检测当前线段是否与另一条线段相交
    /// 使用快速排斥实验和跨立实验来判断
    /// </summary>
    /// <param name="other">要检测相交的另一条线段</param>
    /// <returns>如果相交返回true，否则返回false</returns>
    public bool Cross(Line other)
    {
        // 快速排斥实验：检查两线段的包围盒是否重叠
        bool boundingBoxesOverlap = 
            Mathf.Min(this.point[0].x, this.point[1].x) <= Mathf.Max(other.point[0].x, other.point[1].x) &&
            Mathf.Max(this.point[0].x, this.point[1].x) >= Mathf.Min(other.point[0].x, other.point[1].x) &&
            Mathf.Min(this.point[0].y, this.point[1].y) <= Mathf.Max(other.point[0].y, other.point[1].y) &&
            Mathf.Max(this.point[0].y, this.point[1].y) >= Mathf.Min(other.point[0].y, other.point[1].y);
            
        if (!boundingBoxesOverlap)
            return false;
            
        // 跨立实验：检查线段是否相互跨立
        bool crossStandingTest = 
            ((Line.Mul(other.point[0], this.point[0], other.point[1])) * (Line.Mul(other.point[0], other.point[1], this.point[1]))) >= 0 &&
            ((Line.Mul(this.point[0], other.point[0], this.point[1])) * (Line.Mul(this.point[0], this.point[1], other.point[1]))) >= 0;
            
        return boundingBoxesOverlap && crossStandingTest;
    }

    /// <summary>
    /// 计算叉乘：(b-a)×(c-a)的值
    /// 用于判断点c在向量ab的哪一侧
    /// 返回值大于0表示点c在向量ab的左侧
    /// 返回值小于0表示点c在向量ab的右侧
    /// 返回值等于0表示点c在向量ab上
    /// </summary>
    /// <param name="a">起点</param>
    /// <param name="b">向量终点</param>
    /// <param name="c">待判断的点</param>
    /// <returns>叉乘结果</returns>
    private static float Mul(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }
}