using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : PetBaseState
{
    private float idleTimer;
    private float idleDuration;

    public IdleState(PetController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("进入[发呆]状态");
        controller.Animator.Play("Idle"); // 假设你有一个名为"Idle"的动画状态
        idleDuration = Random.Range(controller.Profile.idleTimeMin, controller.Profile.idleTimeMax);
        idleTimer = 0f;
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            // 发呆结束，切换到闲逛状态
            stateMachine.SwitchState(new WanderState(controller));
        }
    }
}
