using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
    /// <summary>
    /// 范围效果（AOE）脚本集合
    /// 包含AOE创建、移除、触发和移动等回调函数
    /// </summary>
    public class AoE
    {
        #region 回调函数字典

        /// <summary>
        /// AOE创建时回调函数字典
        /// 存储所有可用的AOE创建回调函数
        /// </summary>
        public static Dictionary<string, AoeOnCreate> onCreateFunc = new Dictionary<string, AoeOnCreate>(){
            {"CreateSightEffect", CreateSightEffect}
        };

        /// <summary>
        /// AOE移除时回调函数字典
        /// 存储所有可用的AOE移除回调函数
        /// </summary>
        public static Dictionary<string, AoeOnRemoved> onRemovedFunc = new Dictionary<string, AoeOnRemoved>(){
            {"DoDamageOnRemoved", DoDamageOnRemoved},
            {"CreateAoeOnRemoved", CreateAoeOnRemoved},
            {"BarrelExplosed", BarrelExplosed}
        };

        /// <summary>
        /// AOE周期触发回调函数字典
        /// 存储所有可用的AOE周期触发回调函数
        /// </summary>
        public static Dictionary<string, AoeOnTick> onTickFunc = new Dictionary<string, AoeOnTick>(){
            {"BlackHole", BlackHole}
        };

        /// <summary>
        /// 角色进入AOE区域回调函数字典
        /// 存储所有可用的角色进入AOE区域回调函数
        /// </summary>
        public static Dictionary<string, AoeOnCharacterEnter> onChaEnterFunc = new Dictionary<string, AoeOnCharacterEnter>(){
            {"DoDamageToEnterCha", DoDamageToEnterCha}
        };

        /// <summary>
        /// 角色离开AOE区域回调函数字典
        /// 存储所有可用的角色离开AOE区域回调函数
        /// </summary>
        public static Dictionary<string, AoeOnCharacterLeave> onChaLeaveFunc = new Dictionary<string, AoeOnCharacterLeave>()
        {

        };

        /// <summary>
        /// 子弹进入AOE区域回调函数字典
        /// 存储所有可用的子弹进入AOE区域回调函数
        /// </summary>
        public static Dictionary<string, AoeOnBulletEnter> onBulletEnterFunc = new Dictionary<string, AoeOnBulletEnter>(){
            {"BlockBullets", BlockBullets},
            {"SpaceMonkeyBallHit", SpaceMonkeyBallHit}
        };

        /// <summary>
        /// 子弹离开AOE区域回调函数字典
        /// 存储所有可用的子弹离开AOE区域回调函数
        /// </summary>
        public static Dictionary<string, AoeOnBulletLeave> onBulletLeaveFunc = new Dictionary<string, AoeOnBulletLeave>()
        {

        };

        /// <summary>
        /// AOE移动轨迹计算函数字典
        /// 存储所有可用的AOE移动轨迹计算函数
        /// </summary>
        public static Dictionary<string, AoeTween> aoeTweenFunc = new Dictionary<string, AoeTween>(){
            {"AroundCaster", AroundCaster},
            {"SpaceMonkeyBallRolling", SpaceMonkeyBallRolling}
        };

        #endregion

        #region AOE移动轨迹函数

        /// <summary>
        /// 围绕释放者旋转的移动轨迹函数
        /// 使AOE效果围绕施放者旋转
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        /// <param name="t">时间参数</param>
        /// <returns>AOE移动信息</returns>
        private static AoeMoveInfo AroundCaster(GameObject aoe, float t)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (aoeState == null || aoeState.caster == null) return null;
            Vector3 b = aoeState.caster.transform.position;

            // 获取旋转参数
            float dis = aoeState.tweenParam.Length > 0 ? (float)aoeState.tweenParam[0] : 0;     // 旋转半径
            float degPlus = aoeState.tweenParam.Length > 1 ? (float)aoeState.tweenParam[1] : 0; // 角速度（度/秒）
            float cDeg = degPlus * t;                                                           // 当前旋转角度
            float dr = cDeg * Mathf.PI / 180;                                                   // 转换为弧度

            // 计算目标位置（相对于当前位置的偏移量）
            Vector3 targetP = new Vector3(
                b.x + Mathf.Sin(dr) * dis - aoe.transform.position.x,
                0,
                b.z + Mathf.Cos(dr) * dis - aoe.transform.position.z
            );

            // 返回移动信息，包括移动类型、位置偏移和旋转角度
            return new AoeMoveInfo(MoveType.fly, targetP, cDeg % 360);
        }

        #endregion

        #region 子弹碰撞处理

        /// <summary>
        /// 阻挡子弹函数
        /// 阻挡敌方子弹经过AOE区域
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        /// <param name="bullets">进入区域的子弹列表</param>
        private static void BlockBullets(GameObject aoe, List<GameObject> bullets)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;
            
            // 获取阻挡参数
            AoeModel am = aoeState.model;
            bool countLimited = am.onBulletEnterParams.Length > 0 ? (bool)am.onBulletEnterParams[0] : false;
            int times = aoeState.param.ContainsKey("times") ? (int)aoeState.param["times"] : 1;

            // 获取施放者阵营
            int side = -1;
            if (aoeState.caster)
            {
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            // 处理每个进入的子弹
            for (int i = 0; i < bullets.Count; i++)
            {
                // 获取子弹阵营
                BulletState bs = bullets[i].GetComponent<BulletState>();
                int bSide = -1;
                if (bs && bs.caster)
                {
                    ChaState bcs = bs.caster.GetComponent<ChaState>();
                    if (bcs) bSide = bcs.side;
                }
                
                // 只阻挡敌方子弹
                if (side != bSide)
                {
                    // 移除子弹并创建命中特效
                    SceneVariants.RemoveBullet(bullets[i], false);
                    SceneVariants.CreateSightEffect(
                        "Effect/HitEffect_B", 
                        aoe.transform.position, 
                        aoe.transform.eulerAngles.y
                    );
                }
            }

            // 减少可阻挡次数（如果有限制）
            times -= 1;
        }

        /// <summary>
        /// 太空猴球滚动轨迹计算函数
        /// 实现球类AOE的物理滚动效果
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        /// <param name="t">时间参数</param>
        /// <returns>AOE移动信息</returns>
        private static AoeMoveInfo SpaceMonkeyBallRolling(GameObject aoe, float t)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return null;

            Vector3 velocity = aoeState.tweenParam.Length > 0 ? (Vector3)aoeState.tweenParam[0] : Vector3.zero;
            velocity *= Time.fixedDeltaTime; //算的是一个tick的，所以得在这里乘一下，回头再读取的地方除一下，这是因为设计者在设计这个函数时候思考环境不同所产生的必须要的“牺牲”
            List<Vector3> forces = aoeState.param.ContainsKey("forces") ? (List<Vector3>)aoeState.param["forces"] : null;
            if (forces != null)
            {
                for (int i = 0; i < forces.Count; i++)
                {
                    velocity += forces[i] * Time.fixedDeltaTime;
                }
            }

            float dis = Mathf.Sqrt(Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.z, 2));
            float rr = Mathf.Atan2(velocity.x, velocity.z);
            float rotateTo = rr * 180 / Mathf.PI;

            return new AoeMoveInfo(MoveType.fly, new Vector3(Mathf.Sin(rr) * dis, 0, Mathf.Cos(rr) * dis), rotateTo);
        }

        private static void SpaceMonkeyBallHit(GameObject aoe, List<GameObject> bullets)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            float baseForce = aoeState.model.onBulletEnterParams.Length > 0 ? (float)aoeState.model.onBulletEnterParams[0] : 0;
            if (baseForce == 0) return;

            int side = -1;
            if (aoeState.caster)
            {
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                side = ccs.side;
            }

            if (aoeState.param.ContainsKey("forces") == false)
            {
                aoeState.param["forces"] = new List<Vector3>();
            }
            for (int i = 0; i < bullets.Count; i++)
            {
                BulletState bs = bullets[i].GetComponent<BulletState>();
                int bSide = -1;

                if (bs)
                {
                    if (bs.caster)
                    {
                        ChaState bcs = bs.caster.GetComponent<ChaState>();
                        if (bcs) bSide = bcs.side;
                    }
                    if (bSide == side)
                    {
                        Vector3 bMove = bs.velocity * baseForce;    //算了，就直接乘把，凑合凑合
                        ((List<Vector3>)aoeState.param["forces"]).Add(bMove);
                        SceneVariants.RemoveBullet(bullets[i]);
                    }
                }
            }

            float scaleTo = 1 + ((List<Vector3>)aoeState.param["forces"]).Count * 0.05f;
            aoeState.radius = 0.25f * scaleTo;
            aoeState.SetViewScale(scaleTo);
            aoeState.ModViewY(aoeState.radius);
        }

        #endregion

        #region 角色碰撞处理

        /// <summary>
        /// 对进入区域的角色造成伤害
        /// 当角色进入AOE区域时触发伤害效果
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        /// <param name="characters">进入区域的角色列表</param>
        private static void DoDamageToEnterCha(GameObject aoe, List<GameObject> characters)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onChaEnterParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            string effect = p.Length > 5 ? (string)p[5] : "";
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            int side = -1;
            if (aoeState.caster)
            {
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            for (int i = 0; i < characters.Count; i++)
            {
                ChaState cs = characters[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && side != cs.side) || (toAlly == true && side == cs.side)))
                {
                    Vector3 chaToAoe = characters[i].transform.position - aoe.transform.position;
                    SceneVariants.CreateDamage(
                        aoeState.caster, characters[i],
                        damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                        0.05f, new DamageInfoTag[] { DamageInfoTag.directDamage }
                    );
                    if (hurtAction == true) cs.Play("Hurt");
                    if (effect != "") cs.PlaySightEffect(bp, effect);
                }
            }
        }

        #endregion

        #region AOE效果触发

        /// <summary>
        /// AOE结束时造成伤害
        /// 在AOE效果移除时对范围内角色造成伤害
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        private static void DoDamageOnRemoved(GameObject aoe)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            object[] p = aoeState.model.onRemovedParams;
            Damage baseDamage = p.Length > 0 ? (Damage)p[0] : new Damage(0);
            float damageTimes = p.Length > 1 ? (float)p[1] : 0;
            bool toFoe = p.Length > 2 ? (bool)p[2] : true;
            bool toAlly = p.Length > 3 ? (bool)p[3] : false;
            bool hurtAction = p.Length > 4 ? (bool)p[4] : false;
            string effect = p.Length > 5 ? (string)p[5] : "";
            string bp = p.Length > 6 ? (string)p[6] : "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            int side = -1;
            if (aoeState.caster)
            {
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            for (int i = 0; i < aoeState.characterInRange.Count; i++)
            {
                ChaState cs = aoeState.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && ((toFoe == true && side != cs.side) || (toAlly == true && side == cs.side)))
                {
                    Vector3 chaToAoe = aoeState.characterInRange[i].transform.position - aoe.transform.position;
                    SceneVariants.CreateDamage(
                        aoeState.caster, aoeState.characterInRange[i],
                        damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                        0.05f, new DamageInfoTag[] { DamageInfoTag.directDamage }
                    );
                    if (hurtAction == true) cs.Play("Hurt");
                    if (effect != "") cs.PlaySightEffect(bp, effect);
                }
            }
        }

        /// <summary>
        /// 黑洞效果
        /// 周期性地吸引周围角色并造成伤害
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        private static void BlackHole(GameObject aoe)
        {
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            for (int i = 0; i < ast.characterInRange.Count; i++)
            {
                ChaState cs = ast.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false)
                {
                    Vector3 disV = aoe.transform.position - ast.characterInRange[i].transform.position;
                    float distance = Mathf.Sqrt(Mathf.Pow(disV.x, 2) + Mathf.Pow(disV.z, 2));
                    float inTime = distance / (distance + 1.00f);   //1米是0.5秒，之后越来越大，但增幅是变小的
                    cs.AddForceMove(new MovePreorder(
                        disV * inTime, 1.00f
                    ));
                }
            }
        }

        /// <summary>
        /// 创建视觉特效
        /// 在AOE创建时生成视觉特效
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        private static void CreateSightEffect(GameObject aoe)
        {
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            object[] p = ast.model.onCreateParams;
            string prefab = p.Length > 0 ? (string)p[0] : "";
            SceneVariants.CreateSightEffect(
                prefab, aoe.transform.position, aoe.transform.eulerAngles.y
            );
        }

        /// <summary>
        /// AOE结束时创建新的AOE
        /// 在当前AOE效果移除时创建一个新的AOE效果
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        private static void CreateAoeOnRemoved(GameObject aoe)
        {
            AoeState ast = aoe.GetComponent<AoeState>();
            if (!ast) return;
            object[] p = ast.model.onRemovedParams;
            if (p.Length <= 0) return;
            string id = (string)p[0];
            if (id == "" || DesingerTables.AoE.data.ContainsKey(id) == false) return;
            AoeModel model = DesingerTables.AoE.data[id];
            float radius = p.Length > 1 ? (float)p[1] : 0.01f;
            float duration = p.Length > 2 ? (float)p[2] : 0;
            string aoeTweenId = p.Length > 3 ? (string)p[3] : "";
            AoeTween tween = null;
            if (aoeTweenId != "" && DesignerScripts.AoE.aoeTweenFunc.ContainsKey(aoeTweenId))
            {
                tween = DesignerScripts.AoE.aoeTweenFunc[aoeTweenId];
            }
            object[] tp = new object[0];
            if (p.Length > 4) tp = (object[])p[4];
            Dictionary<string, object> ap = null;
            if (p.Length > 5) ap = (Dictionary<string, object>)p[5];
            AoeLauncher al = new AoeLauncher(
                model, ast.caster, aoe.transform.position, radius,
                duration, aoe.transform.eulerAngles.y, tween, tp, ap
            );
            SceneVariants.CreateAoE(al);
        }

        /// <summary>
        /// 桶爆炸效果
        /// 处理桶类物体被摧毁时的爆炸效果
        /// </summary>
        /// <param name="aoe">AOE对象</param>
        private static void BarrelExplosed(GameObject aoe)
        {
            AoeState aoeState = aoe.GetComponent<AoeState>();
            if (!aoeState) return;

            //new Damage(0, 50), 0.15f, true, false, true, "Effect/HitEffect_A", "Body"
            Damage baseDamage = new Damage(0, 50);
            float damageTimes = 0.15f;
            string effect = "Effect/HitEffect_A";
            string bp = "Body";

            Damage damage = baseDamage * (aoeState.propWhileCreate.attack * damageTimes);

            int side = -1;
            if (aoeState.caster)
            {
                ChaState ccs = aoeState.caster.GetComponent<ChaState>();
                if (ccs) side = ccs.side;
            }

            for (int i = 0; i < aoeState.characterInRange.Count; i++)
            {
                ChaState cs = aoeState.characterInRange[i].GetComponent<ChaState>();
                if (cs && cs.dead == false && side != cs.side)
                {
                    if (cs.HasTag("Barrel") == true)
                    {
                        SceneVariants.CreateDamage(
                            (GameObject)aoeState.param["Barrel"], aoeState.characterInRange[i],
                            new Damage(0, 9999), 0f, 0f, new DamageInfoTag[] { DamageInfoTag.directDamage }
                        );
                    }
                    else
                    {
                        Vector3 chaToAoe = aoeState.characterInRange[i].transform.position - aoe.transform.position;
                        SceneVariants.CreateDamage(
                            aoeState.caster, aoeState.characterInRange[i],
                            damage, Mathf.Atan2(chaToAoe.x, chaToAoe.z) * 180 / Mathf.PI,
                            0.05f, new DamageInfoTag[] { DamageInfoTag.directDamage }
                        );
                        cs.Play("Hurt");
                        cs.PlaySightEffect(bp, effect);
                    }

                }
            }
        }

        #endregion
    }
}