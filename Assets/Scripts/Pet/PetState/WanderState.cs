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
        controller.Animator.Play("Walk"); // 假设你有一个名为"Walk"的动画状态

        // 在可移动区域内随机一个目标点
        float randomX = Random.Range(controller.WalkableArea.xMin, controller.WalkableArea.xMax);
        float randomY = Random.Range(controller.WalkableArea.yMin, controller.WalkableArea.yMax);
        targetPosition = new Vector2(randomX, randomY);
    }

    public override void Update()
    {
        // 移动
        controller.RectTransform.anchoredPosition = Vector2.MoveTowards(
            controller.RectTransform.anchoredPosition,
            targetPosition,
            controller.Profile.moveSpeed * Time.deltaTime
        );

        // 到达目标点
        if (Vector2.Distance(controller.RectTransform.anchoredPosition, targetPosition) < 1f)
        {
            stateMachine.SwitchState(new IdleState(controller));
        }
    }

    public override void Exit()
    {
        controller.Animator.Play("Idle");
    }
}
