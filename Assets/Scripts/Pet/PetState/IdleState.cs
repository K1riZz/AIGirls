using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class IdleState : PetBaseState
{
    private float idleTimer;
    private float idleDuration;
    private float chatterTimer;
    private float nextChatterTime;

    public IdleState(PetController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("进入[发呆]状态");
        controller.Animator.Play("Idle"); // 假设你有一个名为"Idle"的动画状态
        idleDuration = Random.Range(controller.Profile.idleTimeMin, controller.Profile.idleTimeMax);
        idleTimer = 0f;

        chatterTimer = 0f;
        SetNextChatterTime();
    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {
            // 发呆结束，切换到闲逛状态
            stateMachine.SwitchState(new WanderState(controller));
        }

        // 处理闲聊逻辑
        chatterTimer += Time.deltaTime;
        if (chatterTimer >= nextChatterTime)
        {
            TryStartIdleChatter();
            SetNextChatterTime(); // 重置计时器
        }
    }

    private void SetNextChatterTime()
    {
        // 例如，设置下一次闲聊检查在10到30秒后
        nextChatterTime = Random.Range(10f, 30f);
        chatterTimer = 0f;
    }

    private void TryStartIdleChatter()
    {
        // 检查对话是否正在进行，以及是否有可用的闲聊标题
        if (!PixelCrushers.DialogueSystem.DialogueManager.IsConversationActive && controller.Profile.idleChatterTitles.Count > 0)
        {
            int index = Random.Range(0, controller.Profile.idleChatterTitles.Count);
            string conversationTitle = controller.Profile.idleChatterTitles[index];
            PixelCrushers.DialogueSystem.DialogueManager.StartConversation(conversationTitle, controller.transform);

        }
    }
}
