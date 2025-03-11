using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 饼图控制器：用于在UI系统中创建可配置的饼状图形
/// 基于Mesh动态生成，支持调整半径、起始角度和扇形角度
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PieChartController : MonoBehaviour
{
    /// <summary>
    /// 饼图半径
    /// </summary>
    public float radius = 2.0f;
    
    /// <summary>
    /// 起始角度（0-360度）
    /// </summary>
    [Range(0, 360)]
    public float startAngleDegree = 0;
    
    /// <summary>
    /// 扇形角度范围（0-360度）
    /// </summary>
    [Range(0, 360)]
    public float angleDegree = 100;

    /// <summary>
    /// 角度精度（越高越平滑，但性能消耗越大）
    /// </summary>
    public int angleDegreePrecision = 1000;
    
    /// <summary>
    /// 半径精度（越高越精确，但性能消耗越大）
    /// </summary>
    public int radiusPrecision = 1000;

    /// <summary>
    /// 网格过滤器组件引用
    /// </summary>
    private MeshFilter meshFilter;

    /// <summary>
    /// 扇形网格创建器实例
    /// </summary>
    private SectorMeshCreator creator = new SectorMeshCreator();

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    /// <summary>
    /// 每帧更新饼图状态
    /// </summary>
    private void Update()
    {
        if (meshFilter != null)
        {
            meshFilter.mesh = creator.CreateMesh(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision);
        }
    }

    /// <summary>
    /// 扇形网格创建器：负责生成饼图的网格数据
    /// </summary>
    private class SectorMeshCreator
    {
        /// <summary>
        /// 当前饼图半径
        /// </summary>
        private float radius;
        
        /// <summary>
        /// 当前起始角度
        /// </summary>
        private float startAngleDegree;
        
        /// <summary>
        /// 当前扇形角度
        /// </summary>
        private float angleDegree;

        /// <summary>
        /// 缓存的网格对象，避免重复创建
        /// </summary>
        private Mesh cacheMesh;

        /// <summary>
        /// 创建或更新扇形网格
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="startAngleDegree">起始角度</param>
        /// <param name="angleDegree">扇形角度</param>
        /// <param name="angleDegreePrecision">角度精度</param>
        /// <param name="radiusPrecision">半径精度</param>
        /// <returns>生成的网格对象</returns>
        public Mesh CreateMesh(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
        {
            // 检查参数是否变化，避免不必要的重建
            if (CheckParametersChanged(radius, startAngleDegree, angleDegree, angleDegreePrecision, radiusPrecision))
            {
                Mesh newMesh = Create(radius, startAngleDegree, angleDegree);
                if (newMesh != null)
                {
                    cacheMesh = newMesh;
                    this.radius = radius;
                    this.startAngleDegree = startAngleDegree;
                    this.angleDegree = angleDegree;
                }
            }
            return cacheMesh;
        }

        /// <summary>
        /// 计算给定角度的单位圆上的点
        /// </summary>
        /// <param name="angle">角度（度）</param>
        /// <returns>单位圆上的点坐标</returns>
        private Vector3 CalcPoint(float angle)
        {
            angle = angle % 360;
            // 处理特殊角度
            if (angle == 0)
            {
                return new Vector3(1, 0, 0);
            }
            else if (angle == 180)
            {
                return new Vector3(-1, 0, 0);
            }

            // 基于象限计算点坐标
            if (angle <= 45 || angle > 315)
            {
                return new Vector3(1, Mathf.Tan(Mathf.Deg2Rad * angle), 0);
            }
            else if (angle <= 135)
            {
                return new Vector3(1 / Mathf.Tan(Mathf.Deg2Rad * angle), 1, 0);
            }
            else if (angle <= 225)
            {
                return new Vector3(-1, -Mathf.Tan(Mathf.Deg2Rad * angle), 0);
            }
            else
            {
                return new Vector3(-1 / Mathf.Tan(Mathf.Deg2Rad * angle), -1, 0);
            }
        }

        /// <summary>
        /// 创建扇形网格
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="startAngleDegree">起始角度</param>
        /// <param name="angleDegree">扇形角度</param>
        /// <returns>创建的网格对象</returns>
        private Mesh Create(float radius, float startAngleDegree, float angleDegree)
        {
            // 如果起始角为360度，重置为0度
            if (startAngleDegree == 360)
            {
                startAngleDegree = 0;
            }
            
            Mesh mesh = new Mesh();
            List<Vector3> calcVertices = new List<Vector3>();
            
            // 添加中心点和起始点
            calcVertices.Add(Vector3.zero);
            calcVertices.Add(CalcPoint(startAngleDegree));

            // 定义特殊角度（45度的倍数）和对应的单位圆上的点
            float[] specialAngle = new float[] { 45, 135, 225, 315 };
            Vector3[] specialPoint = new Vector3[]
            {
                new Vector3(1,1,0).normalized,
                new Vector3(-1,1,0).normalized,
                new Vector3(-1,-1,0).normalized,
                new Vector3(1,-1,0).normalized
            };

            // 添加扇形内的特殊角度点
            for(int i = 0; i < specialAngle.Length; ++i)
            {
                if (startAngleDegree < specialAngle[i] && specialAngle[i] - startAngleDegree < angleDegree)
                {
                    calcVertices.Add(specialPoint[i]);
                }
            }

            // 处理跨越360度的情况
            for (int i = 0; i < specialAngle.Length; ++i)
            {
                if (startAngleDegree < specialAngle[i] + 360 && specialAngle[i] + 360 - startAngleDegree < angleDegree)
                {
                    calcVertices.Add(specialPoint[i]);
                }
            }
            
            // 添加结束点
            calcVertices.Add(CalcPoint(startAngleDegree + angleDegree));

            // 创建顶点数组
            Vector3[] vertices = new Vector3[calcVertices.Count];
            // 创建UV坐标数组
            Vector2[] uvs = new Vector2[vertices.Length];

            // 设置顶点和UV坐标
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = calcVertices[i] * radius;
                uvs[i] = new Vector2(calcVertices[i].x * 0.5f + 0.5f, calcVertices[i].y * 0.5f + 0.5f);
            }

            // 创建三角形索引数组
            int[] triangles = new int[(vertices.Length - 2) * 3];
            for (int i = 0, vi = 1; i < triangles.Length; i += 3, vi++)
            {
                triangles[i] = 0;        // 中心点
                triangles[i + 2] = vi;   // 当前点
                triangles[i + 1] = vi + 1; // 下一个点
            }
            
            // 设置网格数据
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            return mesh;
        }

        /// <summary>
        /// 检查参数是否发生变化，决定是否需要重新生成网格
        /// </summary>
        /// <returns>参数是否变化</returns>
        private bool CheckParametersChanged(float radius, float startAngleDegree, float angleDegree, int angleDegreePrecision, int radiusPrecision)
        {
            return (int)(startAngleDegree - this.startAngleDegree) != 0 ||
                (int)((angleDegree - this.angleDegree) * angleDegreePrecision) != 0 ||
                (int)((radius - this.radius) * radiusPrecision) != 0;
        }
    }
}
