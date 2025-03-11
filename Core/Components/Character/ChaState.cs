using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色状态类：管理角色的所有状态、属性、技能和Buff
/// 作为战斗系统的核心组件，处理角色的移动、旋转、动画和战斗数据
/// </summary>
public class ChaState : MonoBehaviour
{
    #region 状态控制
    // 基础控制状态（移动、旋转、技能等）
    private ChaControlState _controlState = new ChaControlState(true, true, true);

    // 时间轴控制状态（用于剧情和特殊事件控制）
    public ChaControlState timelineControlState = new ChaControlState(true, true, true);

    /// <summary>
    /// 当前有效的控制状态（基础状态与时间轴状态的组合）
    /// </summary>
    public ChaControlState controlState
    {
        get { return this._controlState + this.timelineControlState; }
    }

    /// <summary>
    /// 免疫时间（秒）- 在此期间不受伤害
    /// </summary>
    public float immuneTime
    {
        get { return _immuneTime; }
        set { _immuneTime = Mathf.Max(_immuneTime, value); } // 取较大值，避免被较小值覆盖
    }
    private float _immuneTime = 0.00f;

    /// <summary>
    /// 角色是否处于蓄力状态
    /// </summary>
    public bool charging = false;

    /// <summary>
    /// 移动方向角度
    /// </summary>
    public float moveDegree
    {
        get { return _wishToMoveDegree; }
    }
    private float _wishToMoveDegree = 0.00f;

    /// <summary>
    /// 面向方向角度
    /// </summary>
    public float faceDegree
    {
        get { return _wishToFaceDegree; }
    }
    private float _wishToFaceDegree;

    /// <summary>
    /// 角色是否已死亡
    /// </summary>
    public bool dead = false;
    #endregion

    #region 移动与动画
    // 当前移动指令
    private Vector3 moveOrder = new Vector3();

    // 强制移动列表（如击退、弹射等）
    private List<MovePreorder> forceMove = new List<MovePreorder>();

    // 动画播放队列
    private List<string> animOrder = new List<string>();

    // 旋转目标角度
    private float rotateToOrder;

    // 强制旋转列表
    private List<float> forceRotate = new List<float>();
    #endregion

    #region 角色属性
    /// <summary>
    /// 角色资源值（生命值、魔法值等）
    /// </summary>
    public ChaResource resource = new ChaResource(1);

    /// <summary>
    /// 角色阵营（0=中立，1=玩家方，2=敌方等）
    /// </summary>
    public int side = 0;

    /// <summary>
    /// 角色标签（用于技能、Buff等目标选择）
    /// </summary>
    public string[] tags = new string[0];

    /// <summary>
    /// 当前有效属性（基础+Buff+装备）
    /// </summary>
    public ChaProperty property
    {
        get { return _prop; }
    }
    private ChaProperty _prop = ChaProperty.zero;

    /// <summary>
    /// 移动速度（带曲线计算）
    /// 使用曲线函数平衡不同数值范围
    /// </summary>
    public float moveSpeed
    {
        get { return this._prop.moveSpeed * 5.600f / (this._prop.moveSpeed + 100.000f) + 0.200f; }
    }

    /// <summary>
    /// 行动速度（带曲线计算）
    /// 影响动画播放速度和技能冷却
    /// </summary>
    public float actionSpeed
    {
        get { return this._prop.actionSpeed * 4.90f / (_prop.actionSpeed + 390.00f) + 0.100f; }
    }

    /// <summary>
    /// 基础属性
    /// </summary>
    public ChaProperty baseProp = new ChaProperty(100, 100, 0, 20, 100);

    /// <summary>
    /// Buff加成属性 [0]=增益，[1]=减益
    /// </summary>
    public ChaProperty[] buffProp = new ChaProperty[2] { ChaProperty.zero, ChaProperty.zero };

    /// <summary>
    /// 装备加成属性
    /// </summary>
    public ChaProperty equipmentProp = ChaProperty.zero;

    public List<SkillObj> skills = new List<SkillObj>();

    public List<BuffObj> buffs = new List<BuffObj>();

    private UnitMove unitMove;
    private UnitAnim unitAnim;
    private UnitRotate unitRotate;
    private Animator animator;
    private UnitBindManager bindPoints;
    private GameObject viewContainer;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 初始化角色状态
    /// </summary>
    void Start()
    {
        rotateToOrder = transform.rotation.eulerAngles.y;
        synchronizedUnits();
        AttrRecheck();
    }

    /// <summary>
    /// 角色状态的主要更新逻辑
    /// </summary>
    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        
        if (!dead)
        {
            // 更新免疫时间
            UpdateImmuneTime(deltaTime);
            
            // 更新技能冷却
            UpdateSkillCooldowns(deltaTime);
            
            // 更新Buff状态
            ProcessBuffs(deltaTime);
            
            // 处理角色移动
            ProcessMovement(deltaTime);
            
            // 处理角色旋转
            ProcessRotation();
            
            // 处理角色动画
            ProcessAnimation();
        }
        else
        {
            // 角色死亡状态更新
            UpdateDeadState();
        }
    }
    #endregion

    #region 更新辅助方法
    /// <summary>
    /// 更新免疫时间
    /// </summary>
    private void UpdateImmuneTime(float deltaTime)
    {
        if (_immuneTime > 0) 
            _immuneTime -= deltaTime;
    }

    /// <summary>
    /// 更新技能冷却时间
    /// </summary>
    private void UpdateSkillCooldowns(float deltaTime)
            {
        foreach (var skill in skills)
        {
            if (skill.cooldown > 0)
                skill.cooldown -= deltaTime;
                }
            }

    /// <summary>
    /// 处理所有激活的Buff
    /// </summary>
    private void ProcessBuffs(float deltaTime)
    {
        List<BuffObj> buffToRemove = new List<BuffObj>();
        
        foreach (var buff in buffs)
            {
            // 更新持续时间
            if (!buff.permanent) 
                buff.duration -= deltaTime;
                
            buff.timeElapsed += deltaTime;

            // 处理周期性效果
            ProcessBuffTick(buff);

            // 检查Buff是否应该移除
            if (ShouldRemoveBuff(buff))
            {
                buff.model.onRemoved?.Invoke(buff);
                buffToRemove.Add(buff);
            }
        }
        
        // 移除过期的Buff
        if (buffToRemove.Count > 0)
        {
            foreach (var buff in buffToRemove)
            {
                buffs.Remove(buff);
            }
            
            // 重新计算属性
            AttrRecheck();
        }
    }

    /// <summary>
    /// 处理Buff的周期性效果
    /// </summary>
    private void ProcessBuffTick(BuffObj buff)
    {
        if (buff.model.tickTime > 0 && buff.model.onTick != null)
                {
            int currentTick = Mathf.RoundToInt(buff.timeElapsed * 1000);
            int tickInterval = Mathf.RoundToInt(buff.model.tickTime * 1000);
            
            if (currentTick % tickInterval == 0)
                    {
                buff.model.onTick(buff);
                buff.ticked += 1;
                    }
                }
    }

    /// <summary>
    /// 检查Buff是否应该被移除
    /// </summary>
    private bool ShouldRemoveBuff(BuffObj buff)
    {
        return buff.duration <= 0 || buff.stack <= 0;
    }

    /// <summary>
    /// 处理角色移动逻辑
    /// </summary>
    private void ProcessMovement(float deltaTime)
    {
        bool wantsToMove = moveOrder != Vector3.zero;
        
        // 根据移动方向更新移动角度
        if (wantsToMove)
                _wishToMoveDegree = Mathf.Atan2(moveOrder.x, moveOrder.z) * 180 / Mathf.PI;

        ChaControlState currentControlState = this.controlState;
        bool canMove = currentControlState.canMove && moveOrder != Vector3.zero;

            if (unitMove)
            {
            // 如果不能移动，清空移动指令
            if (!currentControlState.canMove) 
                moveOrder = Vector3.zero;
                
            // 处理强制移动（如击退）
            ProcessForcedMovement(deltaTime);
            
            // 应用移动
            unitMove.MoveBy(moveOrder);
            moveOrder = Vector3.zero;
        }
    }

    /// <summary>
    /// 处理强制移动
    /// </summary>
    private void ProcessForcedMovement(float deltaTime)
    {
        int index = 0;
        while (index < forceMove.Count)
        {
            // 添加强制移动到当前移动
            moveOrder += forceMove[index].VeloInTime(deltaTime);
            
            // 检查强制移动是否结束
            if (forceMove[index].duration <= 0)
                forceMove.RemoveAt(index);
            else
                index++;
        }
    }

    /// <summary>
    /// 处理角色旋转逻辑
    /// </summary>
    private void ProcessRotation()
    {
            _wishToFaceDegree = rotateToOrder;
        
        // 如果没有移动，面向与移动方向相同
        if (moveOrder == Vector3.zero) 
            _wishToMoveDegree = _wishToFaceDegree;
            
            if (unitRotate)
            {
            // 如果不能旋转，保持当前角度
            if (!controlState.canRotate) 
                rotateToOrder = transform.rotation.eulerAngles.y;
                
            // 应用强制旋转
            foreach (float rotation in forceRotate)
                {
                rotateToOrder += rotation;
                }
            
            // 执行旋转
                unitRotate.RotateTo(rotateToOrder);
                forceRotate.Clear();
            }
    }

    /// <summary>
    /// 处理角色动画逻辑
    /// </summary>
    private void ProcessAnimation()
    {
            if (unitAnim)
            {
            // 设置动画速度
                unitAnim.timeScale = this.actionSpeed;
            
            // 根据移动状态播放不同动画
            if (moveOrder == Vector3.zero)
                {
                    animOrder.Add("Stand");
                }
                else
                {
                float moveDegree = Mathf.Atan2(moveOrder.x, moveOrder.z) * 180 / Mathf.PI;
                if (moveDegree > 180) moveDegree -= 360;
                
                string directionSuffix = Utils.GetTailStringByDegree(transform.rotation.eulerAngles.y, moveDegree);
                animOrder.Add("Move" + directionSuffix);
                }
            
            // 播放所有队列中的动画
            foreach (string anim in animOrder)
                {
                unitAnim.Play(anim);
                }
            
                animOrder.Clear();
            }
        
        // 更新Animator速度
            if (animator)
            {
                animator.speed = this.actionSpeed;
            }
        }

    /// <summary>
    /// 更新死亡状态
    /// </summary>
    private void UpdateDeadState()
        {
            _wishToFaceDegree = transform.rotation.eulerAngles.y * 180.00f / Mathf.PI;
            _wishToMoveDegree = _wishToFaceDegree;
        }
    #endregion

    #region 组件同步
    /// <summary>
    /// 同步获取角色相关的组件引用
    /// </summary>
    private void synchronizedUnits()
    {
        if (!unitMove) unitMove = GetComponent<UnitMove>();
        if (!unitAnim) unitAnim = GetComponent<UnitAnim>();
        if (!unitRotate) unitRotate = GetComponent<UnitRotate>();
        if (!animator) animator = GetComponent<Animator>();
        if (!bindPoints) bindPoints = GetComponent<UnitBindManager>();
        if (!viewContainer) viewContainer = GetComponentInChildren<ViewContainer>()?.gameObject;
    }
    #endregion

    public void OrderMove(Vector3 move)
    {
        this.moveOrder.x = move.x;
        this.moveOrder.z = move.z;
    }

    public void AddForceMove(MovePreorder move)
    {
        this.forceMove.Add(move);
    }

    public void OrderRotateTo(float degree)
    {
        this.rotateToOrder = degree;
    }

    public void AddForceRotate(float degree)
    {
        this.forceRotate.Add(degree);
    }

    public void Play(string animName)
    {
        animOrder.Add(animName);
    }

    public void Kill()
    {
        this.dead = true;
        if (unitAnim)
        {
            unitAnim.Play("Dead");
        }
        if (this.gameObject != SceneVariants.MainActor())
            this.gameObject.AddComponent<UnitRemover>().duration = 5.0f;
    }

    private void AttrRecheck()
    {
        _controlState.Origin();
        this._prop.Zero();

        for (var i = 0; i < buffProp.Length; i++) buffProp[i].Zero();
        for (int i = 0; i < this.buffs.Count; i++)
        {
            for (int j = 0; j < Mathf.Min(buffProp.Length, buffs[i].model.propMod.Length); j++)
            {
                buffProp[j] += buffs[i].model.propMod[j] * buffs[i].stack;
            }
            _controlState += buffs[i].model.stateMod;
        }

        this._prop = (this.baseProp + this.equipmentProp + this.buffProp[0]) * this.buffProp[1];

        if (unitMove)
        {
            unitMove.bodyRadius = this._prop.bodyRadius;
        }
    }

    public void ModResource(ChaResource value)
    {
        this.resource += value;
        this.resource.hp = Mathf.Clamp(this.resource.hp, 0, this._prop.hp);
        this.resource.ammo = Mathf.Clamp(this.resource.ammo, 0, this._prop.ammo);
        this.resource.stamina = Mathf.Clamp(this.resource.stamina, 0, 100);
        if (this.resource.hp <= 0)
        {
            this.Kill();
        }
    }

    public void PlaySightEffect(string bindPointKey, string effect, string effectKey = "", bool loop = false)
    {
        bindPoints.AddBindGameObject(bindPointKey, "Prefabs/" + effect, effectKey, loop);
    }

    public void StopSightEffect(string bindPointKey, string effectKey)
    {
        bindPoints.RemoveBindGameObject(bindPointKey, effectKey);
    }

    public bool CanBeKilledByDamageInfo(DamageInfo damageInfo)
    {
        if (this.immuneTime > 0 || damageInfo.isHeal() == true) return false;
        int dValue = damageInfo.DamageValue(false);
        return dValue >= this.resource.hp;
    }

    public void AddBuff(AddBuffInfo buff)
    {
        List<GameObject> bCaster = new List<GameObject>();
        if (buff.caster) bCaster.Add(buff.caster);
        List<BuffObj> hasOnes = GetBuffById(buff.buffModel.id, bCaster);
        int modStack = Mathf.Min(buff.addStack, buff.buffModel.maxStack);
        bool toRemove = false;
        BuffObj toAddBuff = null;
        if (hasOnes.Count > 0)
        {
            //已经存在
            hasOnes[0].buffParam = new Dictionary<string, object>();
            if (buff.buffParam != null)
            {
                foreach (KeyValuePair<string, object> kv in buff.buffParam) { hasOnes[0].buffParam[kv.Key] = kv.Value; }
                ;
            }

            hasOnes[0].duration = (buff.durationSetTo == true) ? buff.duration : (buff.duration + hasOnes[0].duration);
            int afterAdd = hasOnes[0].stack + modStack;
            modStack = afterAdd >= hasOnes[0].model.maxStack ?
                (hasOnes[0].model.maxStack - hasOnes[0].stack) :
                (afterAdd <= 0 ? (0 - hasOnes[0].stack) : modStack);
            hasOnes[0].stack += modStack;
            hasOnes[0].permanent = buff.permanent;
            toAddBuff = hasOnes[0];
            toRemove = hasOnes[0].stack <= 0;
        }
        else
        {
            //新建
            toAddBuff = new BuffObj(
                buff.buffModel,
                buff.caster,
                this.gameObject,
                buff.duration,
                buff.addStack,
                buff.permanent,
                buff.buffParam
            );
            buffs.Add(toAddBuff);
            buffs.Sort((a, b) => {
                return a.model.priority.CompareTo(b.model.priority);
            });
        }
        if (toRemove == false && buff.buffModel.onOccur != null)
        {
            buff.buffModel.onOccur(toAddBuff, modStack);
        }
        AttrRecheck();
    }

    public List<BuffObj> GetBuffById(string id, List<GameObject> caster = null)
    {
        List<BuffObj> res = new List<BuffObj>();
        for (int i = 0; i < this.buffs.Count; i++)
        {
            if (buffs[i].model.id == id && (caster == null || caster.Count <= 0 || caster.Contains(buffs[i].caster) == true))
            {
                res.Add(buffs[i]);
            }
        }
        return res;
    }

    public SkillObj GetSkillById(string id)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            if (skills[i].model.id == id)
            {
                return skills[i];
            }
        }
        return null;
    }

    public bool CastSkill(string id)
    {
        if (this.controlState.canUseSkill == false) return false; //不能用技能就不放了
        SkillObj skillObj = GetSkillById(id);
        if (skillObj == null || skillObj.cooldown > 0) return false;
        bool castSuccess = false;
        if (this.resource.Enough(skillObj.model.condition) == true)
        {
            TimelineObj timeline = new TimelineObj(
                skillObj.model.effect, this.gameObject, skillObj
            );
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].model.onCast != null)
                {
                    timeline = buffs[i].model.onCast(buffs[i], skillObj, timeline);
                }
            }
            if (timeline != null)
            {
                this.ModResource(-1 * skillObj.model.cost);
                SceneVariants.CreateTimeline(timeline);
                castSuccess = true;
            }

        }
        skillObj.cooldown = 0.1f;   //无论成功与否，都会进入gcd
        return castSuccess;
    }

    public void InitBaseProp(ChaProperty cProp)
    {
        this.baseProp = cProp;
        this.AttrRecheck();
        this.resource.hp = this._prop.hp;
        this.resource.ammo = this._prop.ammo;
        this.resource.stamina = 100;
    }

    public void LearnSkill(SkillModel skillModel, int level = 1)
    {
        this.skills.Add(new SkillObj(skillModel, level));
        if (skillModel.buff != null)
        {
            for (int i = 0; i < skillModel.buff.Length; i++)
            {
                AddBuffInfo abi = skillModel.buff[i];
                abi.permanent = true;
                abi.duration = 10;
                abi.durationSetTo = true;
                this.AddBuff(abi);
            }
        }
    }

    public void SetView(GameObject view, Dictionary<string, AnimInfo> animInfo)
    {
        if (view == null) return;
        synchronizedUnits();
        view.transform.SetParent(viewContainer.transform);
        view.transform.position = new Vector3(0, this.gameObject.transform.position.y, 0);
        this.gameObject.transform.position = new Vector3(
            this.gameObject.transform.position.x,
            0,
            this.gameObject.transform.position.z
        );
        this.gameObject.GetComponent<UnitAnim>().animInfo = animInfo;
    }

    public void SetImmuneTime(float time)
    {
        this._immuneTime = Mathf.Max(this._immuneTime, time);
    }

    public bool HasTag(string tag)
    {
        if (this.tags == null || this.tags.Length <= 0) return false;
        for (int i = 0; i < this.tags.Length; i++)
        {
            if (tags[i] == tag)
            {
                return true;
            }
        }
        return false;
    }
}