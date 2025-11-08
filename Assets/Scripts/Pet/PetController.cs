using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PetStateMachine))]
[RequireComponent(typeof(PetInteraction))]
[RequireComponent(typeof(Animator))]
public class PetController : MonoBehaviour
{
    public PetProfileSO Profile { get; private set; }
    public PetStateMachine StateMachine { get; private set; }
    public Animator Animator { get; private set; }
    public RectTransform RectTransform { get; private set; }

    [Header("UI元素")]
    public GameObject storyModeButton; // 在Inspector中指定剧情模式按钮
    [Header("玩家输入")]
    public GameObject playerInputContainer; // 玩家输入框的容器

    public float Affection { get; set; }
    // 模拟的可移动桌面区域
    public Rect WalkableArea { get; set; }

    // 玩家是否正在输入
    public bool IsPlayerTyping { get; set; }

    // 闲置闲聊计时器
    public float idleChatterTimer = 0f;
    public float nextChatterTime = 0f;

    void Awake()
    {
        StateMachine = GetComponent<PetStateMachine>();
        Animator = GetComponent<Animator>();
        RectTransform = GetComponent<RectTransform>();
        IsPlayerTyping = false;
    }

    public void Initialize(PetProfileSO profile)
    {
        this.Profile = profile;
        this.name = $"Pet_{profile.petName}";

        // 初始化好感度等数值, 从Dialogue System的变量中读取
        Affection = DialogueLua.GetVariable("Affection").AsFloat;

        // 初始化状态机
        StateMachine.Initialize(this);

        // 默认显示输入框并跟随宠物
        if (playerInputContainer != null)
        {
            playerInputContainer.SetActive(true);
        }

        // 初始化闲聊计时器
        ResetIdleChatterTimer();
    }

    /// <summary>
    /// 重置闲置闲聊计时器，并设置下一次闲聊的随机时间。
    /// </summary>
    public void ResetIdleChatterTimer()
    {
        idleChatterTimer = 0f;
        nextChatterTime = Random.Range(Profile.idleChatterIntervalMin, Profile.idleChatterIntervalMax);
        Debug.Log($"[PetController] 下次闲聊将在 {nextChatterTime} 秒后。");
    }

    /// <summary>
    /// 根据当前的运行环境（编辑器或打包后的程序）更新宠物的可活动桌面范围。
    /// 这个方法应该在每次进入桌面模式时调用，以确保活动范围的准确性。
    /// </summary>
    public void UpdateWalkableArea()
    {
        float screenWidth;
        float screenHeight;

#if UNITY_EDITOR
        // 在编辑器中，使用主摄像机的像素尺寸来精确计算活动范围，以获得最准确的预览
        // 这能确保宠物始终在Game窗口内活动
        screenWidth = Camera.main.pixelWidth;
        screenHeight = Camera.main.pixelHeight;
#else
        // 在打包后的程序中，我们使用整个显示器的分辨率
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;
#endif

        var petRect = RectTransform.rect;
        float halfWidth = petRect.width / 2;
        float halfHeight = petRect.height / 2;

        WalkableArea = new Rect(
            halfWidth, halfHeight,
            screenWidth - petRect.width, screenHeight - petRect.height);
        Debug.Log($"[PetController] 更新活动范围为: {WalkableArea}");
    }

    // 由PetInteraction调用
    public void OnClicked()
    {
        if (Profile == null)
        {
            Debug.LogError("引用失败.", this);
            return;
        }

        // 触发一个简单的对话
        if (!string.IsNullOrEmpty(Profile.touchConversationTitle))
        {
            // 如果没有其他对话正在进行，则触发点击对话
            if (!DialogueManager.IsConversationActive)
            {
                TriggerBark(Profile.touchConversationTitle, Profile.touchConversationDuration);
            }
        }
    }

    /// <summary>
    /// 触发一个会自动隐藏的Bark对话。
    /// </summary>
    /// <param name="conversationTitle">对话标题</param>
    /// <param name="duration">显示时长（秒）</param>
    public void TriggerBark(string conversationTitle, float duration)
    {
        if (string.IsNullOrEmpty(conversationTitle) || duration <= 0 || DialogueManager.IsConversationActive)
        {
            return;
        }
        
       // 停止之前可能正在运行的任何BarkThenHide协程，防止UI冲突
        StopCoroutine("BarkThenHide");
        StartCoroutine(BarkThenHide(conversationTitle, duration));

    }
    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }

    /// <summary>
    /// 停止宠物的移动，强制切换到Idle状态。
    /// </summary>
    public void StopMovement()
    {
        if (StateMachine.CurrentState is WanderState)
        {
            StateMachine.SwitchState(new IdleState(this));
        }
    }
    
    /// <summary>
    /// 显示玩家输入的内容作为一个Bark气泡。
    /// </summary>
    /// <param name="message">玩家输入的消息</param>
    public void ShowPlayerBark(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        // 如果宠物正在移动，则强制切换到Idle状态
        if (StateMachine.CurrentState is WanderState)
        {
            StateMachine.SwitchState(new IdleState(this));
        }

        // 使用Dialogue System的BarkString来显示任意文本
        // 这会打断任何当前正在显示的Bark
        // BarkString(文本, 说话者, 听者, 序列)
        DialogueManager.BarkString(message, this.transform);

        Debug.Log($"[PetController] 显示玩家输入: '{message}'");
        // 重置闲聊计时器，避免立即触发闲聊
        ResetIdleChatterTimer();
    }


private IEnumerator BarkThenHide(string conversationTitle, float duration)
    {
        // 1. 触发Bark，让它显示出来
        DialogueManager.Bark(conversationTitle, this.transform);
        Debug.Log($"[PetController] Bark '{conversationTitle}' 显示，持续 {duration} 秒。");

        // 2. 等待指定的时长
        yield return new WaitForSeconds(duration);

        // 3. 手动隐藏Bark UI
        var barkUI = DialogueActor.GetBarkUI(this.transform);
        if (barkUI != null && barkUI.isPlaying)
        {
            // 尝试将 IBarkUI 转换为 StandardBarkUI 以访问其特有成员
            var standardBarkUI = barkUI as StandardBarkUI;
            if (standardBarkUI != null)
            {
                // StandardBarkUI doesn't expose currentConversationTitle directly.
                // We assume if barkUI.isPlaying is true, it's the one we triggered.
                //Debug.Log($"[PetController] Bark '{conversationTitle}' 隐藏。");
                standardBarkUI.Hide();
            } else {
                // 如果不是StandardBarkUI，我们无法进行精确检查，作为备用方案直接隐藏
                //Debug.Log($"[PetController] Bark '{conversationTitle}' 隐藏。");
                barkUI.Hide();
            }
        }
    }


}