using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : PetBaseState
{
    private Vector2 targetPosition;

    public WanderState(PetController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("进入[闲逛]状态");
        controller.Animator.Play("Walk"); 

        // 在可移动区域内随机一个目标点
        // WalkableArea的xMax和yMax已经是正确的右上角坐标了
        float randomX = Random.Range(controller.WalkableArea.x, controller.WalkableArea.xMax);
        float randomY = Random.Range(controller.WalkableArea.y, controller.WalkableArea.yMax);
        targetPosition = new Vector2(randomX, randomY);
    }

    public override void Update()
    {
        // 如果应该强制保持idle状态（对话激活、菜单显示、输入框激活），则立即切换到idle状态
        if (controller.ShouldForceIdle())
        {
            stateMachine.SwitchState(new IdleState(controller));
            return;
        }

        // 在闲逛时也累加闲置计时器
        controller.idleChatterTimer += Time.deltaTime;

        // 移动
        // 由于现在是独立的Overlay Canvas，我们直接操作世界/屏幕坐标
        controller.RectTransform.position = Vector2.MoveTowards(
            controller.RectTransform.position,
            targetPosition,
            controller.Profile.moveSpeed * Time.deltaTime
        );

        // 到达目标点
        if (Vector2.Distance(controller.RectTransform.position, targetPosition) < 1f)
        {
            stateMachine.SwitchState(new IdleState(controller));
        }
    }

    public override void Exit()
    {
        controller.Animator.Play("Idle");
    }
}
