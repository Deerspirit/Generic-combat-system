using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignerScripts
{
	/// <summary>
	/// 时间线事件处理类
	/// 包含技能和行为时间线中各种事件的回调函数
	/// </summary>
	public class Timeline
	{
		#region 时间线事件函数字典

		/// <summary>
		/// 时间线事件函数字典
		/// 存储所有可用的时间线事件回调函数
		/// </summary>
		public static Dictionary<string, TimelineEvent> functions = new Dictionary<string, TimelineEvent>(){
			{"CasterPlayAnim", CasterPlayAnim},             // 施放者播放动画
			{"CasterForceMove", CasterForceMove},           // 施放者强制移动
			{"SetCasterControlState", SetCasterControlState}, // 设置施放者控制状态
			{"PlaySightEffectOnCaster", PlaySightEffectOnCaster}, // 在施放者身上播放视觉特效
			{"StopSightEffectOnCaster", StopSightEffectOnCaster}, // 停止施放者身上的视觉特效
			{"FireBullet", FireBullet},                     // 发射子弹
			{"CasterImmune", CasterImmune},                 // 施放者免疫
			{"CreateAoE", CreateAoE},                       // 创建AOE效果
			{"AddBuffToCaster", AddBuffToCaster},           // 为施放者添加Buff
			{"CasterAddAmmo", CasterAddAmmo},               // 为施放者增加弹药
			{"SummonCharacter", SummonCharacter}            // 召唤角色
		};

		#endregion

		#region 子弹和AOE创建函数

		/// <summary>
		/// 发射子弹
		/// 从施放者的指定挂点位置发射子弹
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=子弹发射器，[1]=挂点名称（默认为"Muzzle"）</param>
		private static void FireBullet(TimelineObj tlo, params object[] args)
		{
			// 参数检查
			if (args.Length <= 0 || tlo.caster == null)
				return;
			
			UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
			if (ubm == null)
				return;

			// 获取子弹发射器和挂点
			BulletLauncher bulletLauncher = (BulletLauncher)args[0];
			string bindPointName = args.Length > 1 ? (string)args[1] : "Muzzle";
			UnitBindPoint bindPoint = ubm.GetBindPointByKey(bindPointName);
			
			if (bindPoint == null)
				return;

			// 设置发射参数
			bulletLauncher.caster = tlo.caster;
			bulletLauncher.fireDegree = tlo.caster.transform.rotation.eulerAngles.y;
			bulletLauncher.firePosition = bindPoint.gameObject.transform.position;

			// 创建子弹
			SceneVariants.CreateBullet(bulletLauncher);
		}

		/// <summary>
		/// 创建AOE效果
		/// 在施放者周围创建范围效果
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=AOE发射器，[1]=是否在前方创建（默认为true）</param>
		private static void CreateAoE(TimelineObj tlo, params object[] args)
		{
			// 参数检查
			if (args.Length <= 0 || tlo.caster == null)
				return;

			UnitBindManager ubm = tlo.caster.GetComponent<UnitBindManager>();
			if (ubm == null)
				return;

			// 克隆AOE发射器（防止修改原始数据）
			AoeLauncher aoeLauncher = ((AoeLauncher)args[0]).Clone();
			bool createInFront = args.Length > 1 ? (bool)args[1] : true;

			// 设置发射参数
			aoeLauncher.caster = tlo.caster;
			aoeLauncher.degree += tlo.caster.transform.rotation.eulerAngles.y;

			// 计算AOE位置
			float angleInRadians = aoeLauncher.degree * Mathf.PI / 180;
			Vector3 position = aoeLauncher.position;

			float distance = Mathf.Sqrt(Mathf.Pow(position.x, 2) + Mathf.Pow(position.z, 2));
			if (createInFront)
			{
				// 如果在前方创建，需要考虑角色体积和AOE半径
				distance += tlo.caster.GetComponent<ChaState>().property.bodyRadius + aoeLauncher.radius;
			}

			// 设置最终AOE位置
			aoeLauncher.position.x = distance * Mathf.Sin(angleInRadians) + tlo.caster.transform.position.x;
			aoeLauncher.position.z = distance * Mathf.Cos(angleInRadians) + tlo.caster.transform.position.z;

			// 设置运动参数
			aoeLauncher.tweenParam = new object[]{
				new Vector3(
					distance * Mathf.Sin(angleInRadians),
					0,
					distance * Mathf.Cos(angleInRadians)
				)
			};

			// 创建AOE
			SceneVariants.CreateAoE(aoeLauncher);
		}

		#endregion

		#region 角色动画和移动控制

		/// <summary>
		/// 施放者播放动画
		/// 让施放者播放指定的动画
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=动画名称，[1]=是否获取方向后缀（默认为false），[2]=是否使用当前角度（默认为false）</param>
		private static void CasterPlayAnim(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 获取参数
			string animationName = args.Length >= 1 ? (string)(args[0]) : "";
			if (string.IsNullOrEmpty(animationName))
				return;
				
			bool getDirectionSuffix = args.Length >= 2 ? (bool)(args[1]) : false;
			bool useCurrentDegree = args.Length >= 3 ? (bool)(args[2]) : false;

			// 计算动画方向后缀
			if (getDirectionSuffix)
			{
				float faceDegree = useCurrentDegree ? characterState.faceDegree : (float)tlo.GetValue("faceDegree");
				float moveDegree = useCurrentDegree ? characterState.moveDegree : (float)tlo.GetValue("moveDegree");
				animationName += Utils.GetTailStringByDegree(faceDegree, moveDegree);
			}
			
			// 播放动画
			characterState.Play(animationName);
		}

		/// <summary>
		/// 施放者强制移动
		/// 让施放者按指定方向和距离移动
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=移动距离，[1]=移动时间（秒），[2]=角度偏移（默认为0），
		/// [3]=是否基于移动方向（默认为true），[4]=是否使用当前角度（默认为false）</param>
		private static void CasterForceMove(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 获取参数
			float distance = args.Length >= 1 ? (float)args[0] : 0.00f;
			float durationInSeconds = (args.Length >= 2 ? (float)args[1] : 0.00f) / tlo.timeScale;  // 考虑时间缩放
			float degreeOffset = args.Length >= 3 ? (float)args[2] : 0.00f;
			bool basedOnMoveDirection = args.Length >= 4 ? (bool)args[3] : true;
			bool useCurrentDegree = args.Length >= 5 ? (bool)args[4] : false;

			// 计算移动方向
			float directionDegree;
			if (basedOnMoveDirection)
			{
				directionDegree = useCurrentDegree ? characterState.moveDegree : (float)tlo.GetValue("moveDegree");
			}
			else
			{
				directionDegree = useCurrentDegree ? characterState.faceDegree : (float)tlo.GetValue("faceDegree");
			}
			
			// 应用角度偏移并转换为弧度
			float angleInRadians = (directionDegree + degreeOffset) * Mathf.PI / 180.00f;

			// 计算移动向量
			Vector3 moveDirection = new Vector3(
				Mathf.Sin(angleInRadians) * distance,
				0,
				Mathf.Cos(angleInRadians) * distance
			);
			
			// 添加强制移动
			characterState.AddForceMove(new MovePreorder(moveDirection, durationInSeconds));
		}

		/// <summary>
		/// 设置施放者控制状态
		/// 控制施放者的移动、旋转和技能使用权限
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=是否可移动，[1]=是否可旋转，[2]=是否可使用技能</param>
		private static void SetCasterControlState(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 设置控制状态
			if (args.Length >= 1) characterState.timelineControlState.canMove = (bool)args[0];
			if (args.Length >= 2) characterState.timelineControlState.canRotate = (bool)args[1];
			if (args.Length >= 3) characterState.timelineControlState.canUseSkill = (bool)args[2];
		}

		#endregion

		#region 视觉特效控制

		/// <summary>
		/// 在施放者身上播放视觉特效
		/// 在指定挂点播放特效
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=挂点名称（默认为"Body"），[1]=特效名称，
		/// [2]=特效键（默认为随机值），[3]=是否循环（默认为false）</param>
		private static void PlaySightEffectOnCaster(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 获取参数
			string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
			string effectName = args.Length >= 2 ? (string)args[1] : "";
			string effectKey = args.Length >= 3 ? (string)args[2] : Random.value.ToString();
			bool loopEffect = args.Length >= 4 ? (bool)args[3] : false;
			
			// 播放特效
			characterState.PlaySightEffect(bindPointKey, effectName, effectKey, loopEffect);
		}

		/// <summary>
		/// 停止施放者身上的视觉特效
		/// 停止指定挂点上的特效
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=挂点名称（默认为"Body"），[1]=特效键</param>
		private static void StopSightEffectOnCaster(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 获取参数
			string bindPointKey = args.Length >= 1 ? (string)args[0] : "Body";
			string effectKey = args.Length >= 2 ? (string)args[1] : "";
			
			if (string.IsNullOrEmpty(effectKey))
				return;
				
			// 停止特效
			characterState.StopSightEffect(bindPointKey, effectKey);
		}

		#endregion

		#region 角色状态修改

		/// <summary>
		/// 施放者免疫
		/// 设置施放者的伤害免疫时间
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=免疫时间（秒）</param>
		private static void CasterImmune(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null || args.Length <= 0)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 设置免疫时间
			float immuneTime = (float)args[0];
			characterState.SetImmuneTime(immuneTime);
		}

		/// <summary>
		/// 为施放者增加弹药
		/// 修改施放者的弹药数量
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=增加的弹药数量</param>
		private static void CasterAddAmmo(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null || args.Length <= 0)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 增加弹药
			int ammoToAdd = (int)args[0];
			characterState.ModResource(new ChaResource(
				characterState.resource.hp,
				ammoToAdd + characterState.resource.ammo,
				characterState.resource.stamina
			));
		}

		/// <summary>
		/// 为施放者添加Buff
		/// 向施放者添加指定的Buff效果
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=Buff添加信息</param>
		private static void AddBuffToCaster(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null || args.Length <= 0)
				return;
				
			ChaState characterState = tlo.caster.GetComponent<ChaState>();
			if (characterState == null)
				return;
				
			// 获取并更新Buff信息
			AddBuffInfo buffInfo = (AddBuffInfo)args[0];
			buffInfo.caster = tlo.caster;
			buffInfo.target = tlo.caster;
			
			// 添加Buff
			characterState.AddBuff(buffInfo);
		}

		/// <summary>
		/// 召唤角色
		/// 在施放者位置召唤一个新角色
		/// </summary>
		/// <param name="tlo">时间线对象</param>
		/// <param name="args">参数数组：[0]=预制体名称，[1]=角色属性，[2]=朝向角度，
		/// [3]=用户代理信息，[4]=标签数组，[5]=初始Buff数组</param>
		private static void SummonCharacter(TimelineObj tlo, params object[] args)
		{
			if (tlo.caster == null || args.Length <= 0)
				return;
				
			// 获取参数
			string prefabName = (string)args[0];
			if (string.IsNullOrEmpty(prefabName))
				return;
				
			int side = -1; // 默认为中立方
			Vector3 position = tlo.caster.transform.position;
			ChaProperty characterProperty = args.Length > 1 ? (ChaProperty)args[1] : new ChaProperty(100, 1);
			float facingDegree = args.Length > 2 ? (float)args[2] : 0;
			string userAgentInfo = args.Length > 3 ? (string)args[3] : "";
			string[] tags = args.Length > 4 ? (string[])args[4] : null;
			AddBuffInfo[] initialBuffs = args.Length > 5 ? (AddBuffInfo[])args[5] : new AddBuffInfo[0];

			// 创建角色
			GameObject summonedCharacter = SceneVariants.CreateCharacter(
				prefabName, side, position, characterProperty, facingDegree, userAgentInfo, tags
			);
			
			// 添加初始Buff
			ChaState summonedCharacterState = summonedCharacter.GetComponent<ChaState>();
			for (int i = 0; i < initialBuffs.Length; i++)
			{
				initialBuffs[i].caster = tlo.caster;
				initialBuffs[i].target = summonedCharacter;
				summonedCharacterState.AddBuff(initialBuffs[i]);
			}
		}

		#endregion
	}
}
