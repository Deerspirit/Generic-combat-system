using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位动画系统：控制游戏中单位的动画播放
/// 支持优先级控制、随机动画和动画速度调整
/// </summary>
public class UnitAnim : MonoBehaviour
{
    #region 公共属性
    /// <summary>
    /// 动画播放速度倍率
    /// </summary>
    [Tooltip("动画播放速度的倍率，1.0为正常速度")]
    public float timeScale = 1.0f;

    /// <summary>
    /// 动画信息字典，键为动画逻辑名称，值为动画信息
    /// </summary>
    [Tooltip("动画信息配置，包含动画名称、优先级和持续时间等")]
    public Dictionary<string, AnimInfo> animInfo;
    #endregion

    #region 私有属性
    /// <summary>
    /// Unity动画控制器
    /// </summary>
    private Animator animator;

    /// <summary>
    /// 当前播放的动画信息
    /// </summary>
    private AnimInfo currentAnimation = null;

    /// <summary>
    /// 当前动画的优先级持续时间
    /// </summary>
    private float priorityDuration = 0.0f;

    /// <summary>
    /// 当前动画的优先级，如果没有正在播放的动画或优先级已过期，则为0
    /// </summary>
    private int currentAnimPriority
    {
        get
        {
            if (currentAnimation == null) 
                return 0;
                
            return priorityDuration <= 0 ? 0 : currentAnimation.priority;
        }
    }

    /// <summary>
    /// 记录上一次尝试查找Animator的时间，避免频繁查找
    /// </summary>
    private float lastAnimatorSearchTime = 0.0f;

    /// <summary>
    /// 查找Animator的间隔时间（秒）
    /// </summary>
    private const float AnimatorSearchInterval = 1.0f;
    #endregion

    #region Unity生命周期
    /// <summary>
    /// 初始化，获取组件引用
    /// </summary>
    void Start()
    {
        FindAnimator();
    }

    /// <summary>
    /// 固定更新，处理动画状态
    /// </summary>
    void FixedUpdate()
    {
        // 确保动画器组件可用
        EnsureAnimatorAvailable();

        // 检查是否可以进行动画处理
        if (!CanProcessAnimation())
            return;

        // 更新优先级持续时间
        UpdatePriorityDuration();
    }
    #endregion

    #region 动画处理
    /// <summary>
    /// 确保动画器组件可用
    /// </summary>
    private void EnsureAnimatorAvailable()
    {
        if (animator != null) 
            return;
            
        // 每隔一段时间尝试查找Animator，避免频繁查找
        float currentTime = Time.time;
        if (currentTime - lastAnimatorSearchTime < AnimatorSearchInterval)
            return;
            
        lastAnimatorSearchTime = currentTime;
        FindAnimator();
    }

    /// <summary>
    /// 查找Animator组件
    /// </summary>
    private void FindAnimator()
    {
        // 首先在自身查找
        animator = GetComponent<Animator>();
        
        // 如果自身没有，则在子对象中查找
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 检查是否可以进行动画处理
    /// </summary>
    /// <returns>是否可以处理动画</returns>
    private bool CanProcessAnimation()
    {
        return animator != null && animInfo != null && animInfo.Count > 0;
    }

    /// <summary>
    /// 更新优先级持续时间
    /// </summary>
    private void UpdatePriorityDuration()
    {
        if (priorityDuration > 0)
            priorityDuration -= Time.fixedDeltaTime * timeScale;
    }
    #endregion

    #region 公共接口
    /// <summary>
    /// 播放指定名称的动画
    /// </summary>
    /// <param name="animName">动画逻辑名称</param>
    /// <returns>是否成功播放</returns>
    public bool Play(string animName)
    {
        // 检查动画信息是否有效
        if (!CanProcessAnimation())
            return false;
            
        // 检查动画是否存在
        if (!animInfo.ContainsKey(animName))
            return false;
            
        // 如果已经在播放指定动画，则不重复播放
        if (currentAnimation != null && currentAnimation.key == animName)
            return true;
            
        // 获取要播放的动画信息
        AnimInfo animationToPlay = animInfo[animName];
        
        // 检查优先级
        if (currentAnimPriority > animationToPlay.priority)
            return false;
            
        // 从动画集合中随机选择一个具体动画
        SingleAnimInfo selectedAnimation = animationToPlay.RandomKey();
        
        // 播放动画
        animator.Play(selectedAnimation.animName);
        
        // 更新当前动画状态
        currentAnimation = animationToPlay;
        priorityDuration = selectedAnimation.duration;
        
        return true;
    }

    /// <summary>
    /// 设置动画控制器
    /// </summary>
    /// <param name="newAnimator">新的动画控制器</param>
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    /// <summary>
    /// 获取当前播放的动画名称
    /// </summary>
    /// <returns>当前动画名称，如果没有则返回空字符串</returns>
    public string GetCurrentAnimationName()
    {
        return currentAnimation?.key ?? string.Empty;
    }

    /// <summary>
    /// 设置动画播放速度
    /// </summary>
    /// <param name="scale">速度倍率</param>
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0.01f, scale);
        
        if (animator != null)
            animator.speed = timeScale;
    }

    /// <summary>
    /// 重置动画状态，允许立即播放任何动画
    /// </summary>
    public void ResetAnimationState()
    {
        currentAnimation = null;
        priorityDuration = 0.0f;
    }
    #endregion
}