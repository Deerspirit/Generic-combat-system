using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工具类
/// 包含通用的几何计算和辅助方法
/// </summary>
public class Utils
{
    #region 碰撞检测

    /// <summary>
    /// 检测圆形是否与多个矩形碰撞（List版本）
    /// </summary>
    /// <param name="circlePivot">圆心位置</param>
    /// <param name="circleRadius">圆半径</param>
    /// <param name="rects">矩形列表</param>
    /// <returns>是否有碰撞</returns>
    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, List<Rect> rects)
    {
        if (rects.Count <= 0) 
            return false;
            
        for (int i = 0; i < rects.Count; i++)
        {
            if (CircleHitRect(circlePivot, circleRadius, rects[i]))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检测圆形是否与多个矩形碰撞（数组版本）
    /// </summary>
    /// <param name="circlePivot">圆心位置</param>
    /// <param name="circleRadius">圆半径</param>
    /// <param name="rects">矩形数组</param>
    /// <returns>是否有碰撞</returns>
    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, Rect[] rects)
    {
        List<Rect> rectangleList = new List<Rect>();
        for (int i = 0; i < rects.Length; i++)
        {
            rectangleList.Add(rects[i]);
        }
        return CircleHitRects(circlePivot, circleRadius, rectangleList);
    }

    /// <summary>
    /// 检测圆形是否与单个矩形碰撞
    /// </summary>
    /// <param name="circlePivot">圆心位置</param>
    /// <param name="circleRadius">圆半径</param>
    /// <param name="rect">矩形</param>
    /// <returns>是否有碰撞</returns>
    public static bool CircleHitRect(Vector2 circlePivot, float circleRadius, Rect rect)
    {
        // 确定圆心相对于矩形的位置
        // xp: 0=在左侧, 1=在水平范围内, 2=在右侧
        // yp: 0=在下方, 1=在垂直范围内, 2=在上方
        int xp = circlePivot.x < rect.x ? 0 : (circlePivot.x > rect.x + rect.width ? 2 : 1);
        int yp = circlePivot.y < rect.y ? 0 : (circlePivot.y > rect.y + rect.height ? 2 : 1);

        // 如果圆心在矩形内部，必定碰撞
        if (yp == 1 && xp == 1) 
            return true;

        // 圆心在矩形的上方或下方，但水平位置在矩形范围内
        if (yp != 1 && xp == 1)
        {
            float halfHeight = rect.height / 2;
            float distanceToCenter = Mathf.Abs(circlePivot.y - (rect.y + halfHeight));
            return (distanceToCenter <= circleRadius + halfHeight);
        }
        // 圆心在矩形的左侧或右侧，但垂直位置在矩形范围内
        else if (yp == 1 && xp != 1)
        {
            float halfWidth = rect.width / 2;
            float distanceToCenter = Mathf.Abs(circlePivot.x - (rect.x + halfWidth));
            return (distanceToCenter <= circleRadius + halfWidth);
        }
        // 圆心在矩形的四个角落区域
        else
        {
            // 检查圆是否与矩形的角点碰撞
            return InRange(
                circlePivot.x, circlePivot.y,
                xp == 0 ? rect.x : (rect.x + rect.width),
                yp == 0 ? rect.y : (rect.y + rect.height),
                circleRadius
            );
        }
    }

    /// <summary>
    /// 检测两个矩形是否碰撞
    /// </summary>
    /// <param name="a">第一个矩形</param>
    /// <param name="b">第二个矩形</param>
    /// <returns>是否有碰撞</returns>
    public static bool RectCollide(Rect a, Rect b)
    {
        // 计算矩形的右侧和上侧坐标
        float aRight = a.x + a.width;
        float bRight = b.x + b.width;
        float aTop = a.y + a.height;
        float bTop = b.y + b.height;
        
        // 检查两个矩形是否在水平和垂直方向上有重叠
        return (
            // 水平方向重叠检查
            ((a.x >= b.x && a.x <= bRight) ||
             (b.x >= a.x && b.x <= aRight))
            && 
            // 垂直方向重叠检查
            ((a.y >= b.y && a.y <= bTop) ||
             (b.y >= a.y && b.y <= aTop))
        );
    }

    /// <summary>
    /// 检测点是否在指定点的范围内（圆形范围）
    /// </summary>
    /// <param name="x1">第一个点的X坐标</param>
    /// <param name="y1">第一个点的Y坐标</param>
    /// <param name="x2">第二个点的X坐标</param>
    /// <param name="y2">第二个点的Y坐标</param>
    /// <param name="range">距离范围</param>
    /// <returns>是否在范围内</returns>
    public static bool InRange(float x1, float y1, float x2, float y2, float range)
    {
        // 使用勾股定理计算两点间距离的平方，并与范围的平方比较
        return Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2) <= Mathf.Pow(range, 2);
    }

    #endregion

    #region 方向计算

    /// <summary>
    /// 根据面向角度和移动角度获取方向字符串
    /// </summary>
    /// <param name="faceDegree">面向角度</param>
    /// <param name="moveDegree">移动角度</param>
    /// <returns>方向字符串（Forward, Left, Right, Back）</returns>
    public static string GetTailStringByDegree(float faceDegree, float moveDegree)
    {
        // 标准化角度到180-540范围（便于计算）
        float faceAngle = faceDegree;
        float moveAngle = moveDegree;
        
        while (faceAngle < 180) faceAngle += 360;
        while (moveAngle < 180) moveAngle += 360;
        
        faceAngle = faceAngle % 360;
        moveAngle = moveAngle % 360;
        
        // 计算相对角度差
        float angleDifference = moveAngle - faceAngle;
        
        // 标准化角度差到-180到180范围
        if (angleDifference > 180)
        {
            angleDifference -= 360;
        }
        else if (angleDifference < -180)
        {
            angleDifference += 360;
        }
        
        // 根据角度差返回方向字符串
        if (angleDifference >= -45 && angleDifference <= 45)
        {
            return "Forward"; // 前
        }
        else if (angleDifference < -45 && angleDifference >= -135)
        {
            return "Left";    // 左
        }
        else if (angleDifference > 45 && angleDifference <= 135)
        {
            return "Right";   // 右
        }
        else
        {
            return "Back";    // 后
        }
    }

    #endregion
}