using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggedState : PetBaseState
{
    public DraggedState(PetController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("进入[被拖拽]状态");
        controller.Animator.Play("Dragged"); // 假设你有一个名为"Dragged"的动画状态
    }

    public override void Update()
    {
        // 由于宠物现在位于自己的Screen Space - Overlay Canvas中，可以直接使用鼠标屏幕坐标
        controller.RectTransform.position = Input.mousePosition;

        // 如果鼠标松开，则切换回Idle状态
        if (Input.GetMouseButtonUp(0))
        {
            stateMachine.SwitchState(new IdleState(controller));
        }
    }
}
