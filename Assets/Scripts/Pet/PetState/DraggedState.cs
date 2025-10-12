using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggedState : PetBaseState
{
    private Canvas parentCanvas;

    public DraggedState(PetController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("进入[被拖拽]状态");
        controller.Animator.Play("Dragged"); // 假设你有一个名为"Dragged"的动画状态
        parentCanvas = controller.GetComponentInParent<Canvas>();
    }

    public override void Update()
    {
        // 更新宠物位置到鼠标位置
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            Input.mousePosition, 
            parentCanvas.worldCamera, 
            out pos);
        controller.transform.position = parentCanvas.transform.TransformPoint(pos);

        // 如果鼠标松开，则切换回Idle状态
        if (Input.GetMouseButtonUp(0))
        {
            stateMachine.SwitchState(new IdleState(controller));
        }
    }
}
