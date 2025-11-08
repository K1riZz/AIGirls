using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class IdleState : PetBaseState
{
    private float idleTimer;
    private float idleDuration;

    public IdleState(PetController controller) : base(controller) { }
    

    public override void Enter()
    {
        Debug.Log("进入[发呆]状态");
        controller.Animator.Play("Idle");
        idleDuration = Random.Range(controller.Profile.idleTimeMin, controller.Profile.idleTimeMax);
        idleTimer = 0f;

        // 显示剧情模式按钮
        if (controller.storyModeButton != null) {
            controller.storyModeButton.SetActive(true);
        }
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        controller.idleChatterTimer += Time.deltaTime;

        // 检查是否到了闲聊时间
        if (controller.idleChatterTimer >= controller.nextChatterTime)
        {
            TryStartIdleChatter();
        }

        // 发呆时间结束后再切换状态，确保所有发呆期间的逻辑都已执行
        if (idleTimer >= idleDuration)
        {
            // 发呆结束，切换到闲逛状态
            stateMachine.SwitchState(new WanderState(controller));
        }
    }

    public override void Exit()
    {
        // 隐藏剧情模式按钮
        if (controller.storyModeButton != null) {
            controller.storyModeButton.SetActive(false);
        }
    }

    private void TryStartIdleChatter()
    {
        // 检查当前是否有其他对话正在进行
        if (!DialogueManager.IsConversationActive && controller.Profile.idleChatterTitles.Count > 0)
        {
            Debug.Log("尝试触发闲置闲聊...");
            int index = Random.Range(0, controller.Profile.idleChatterTitles.Count);
            string chatterTitle = controller.Profile.idleChatterTitles[index];
            // 调用Controller中的通用方法来触发一个会自动消失的Bark
            controller.TriggerBark(chatterTitle, controller.Profile.touchConversationDuration);
            // 闲聊后，重置计时器
            controller.ResetIdleChatterTimer();
        }
    }
}
