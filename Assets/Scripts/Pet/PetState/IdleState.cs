using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class IdleState : PetBaseState
{
    private float idleTimer;
    private float idleDuration;
    
    private float chatterTriggerTime = -1f; // 本次发呆期间触发闲聊的时间点, -1f代表不触发
    private bool hasChattered = false; // 标记本次发呆是否已经聊过天了

    public IdleState(PetController controller) : base(controller) { }
    

    public override void Enter()
    {
        Debug.Log("进入[发呆]状态");
        controller.Animator.Play("Idle");
        idleDuration = Random.Range(controller.Profile.idleTimeMin, controller.Profile.idleTimeMax);
        idleTimer = 0f;
        hasChattered = false;
        chatterTriggerTime = -1f;

        // 显示剧情模式按钮
        if (controller.storyModeButton != null) {
            controller.storyModeButton.SetActive(true);
        }
        
        // 决定在本次发呆期间是否要闲聊 (例如，50%的几率)
        // 并且确保有闲聊内容可说
        if (Random.value < 0.5f && controller.Profile.idleChatterTitles.Count > 0)
        {
            // 在发呆持续时间内随机一个时间点来触发闲聊
            // 为了避免在发呆快结束时才说话，可以稍微限制一下范围，比如在前80%的时间内
            chatterTriggerTime = Random.Range(1f, idleDuration * 0.8f);
        }
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            // 发呆结束，切换到闲逛状态
            stateMachine.SwitchState(new WanderState(controller));
        }

        // 如果设置了闲聊时间，并且还没聊过，并且时间到了
        if (chatterTriggerTime > 0 && !hasChattered && idleTimer >= chatterTriggerTime)
        {
            TryStartIdleChatter();
            hasChattered = true; // 标记为已聊过，防止重复触发
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
            string conversationTitle = controller.Profile.idleChatterTitles[index];
            // 改为使用Bark，它会使用宠物身上的StandardBarkUI
            // Bark的第一个参数是对话标题，第二个是说话者
            DialogueManager.Bark(conversationTitle, controller.transform);
        }
    }
}
