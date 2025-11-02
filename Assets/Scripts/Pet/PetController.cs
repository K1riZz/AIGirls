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

    public float Affection { get; set; }
    // 模拟的可移动桌面区域
    public Rect WalkableArea { get; set; }

    void Awake()
    {
        StateMachine = GetComponent<PetStateMachine>();
        Animator = GetComponent<Animator>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(PetProfileSO profile)
    {
        this.Profile = profile;
        this.name = $"Pet_{profile.petName}";

        // 初始化好感度等数值, 从Dialogue System的变量中读取
        Affection = DialogueLua.GetVariable("Affection").AsFloat;

        // 初始化状态机
        StateMachine.Initialize(this);
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
            // 如果当前没有对话正在进行，则触发Bark
            if (!DialogueManager.IsConversationActive)
            {
                // 调用通用的Bark方法
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
        // 如果标题为空或时长无效，则不执行
        if (string.IsNullOrEmpty(conversationTitle) || duration <= 0) return;

        StartCoroutine(BarkThenHide(conversationTitle, duration));
    }

    private IEnumerator BarkThenHide(string conversationTitle, float duration)
    {
        // 获取宠物身上的IBarkUI组件
        var barkUI = GetComponentInChildren<IBarkUI>();

        // 如果当前有其他Bark正在显示，先停止它
        if (barkUI != null && barkUI.isPlaying)
        {
            barkUI.Hide();
            // 短暂等待，确保旧的Bark UI已清理
            yield return null;
        }

        Debug.Log($"Starting bark: {conversationTitle} for {duration} seconds.");
        DialogueManager.Bark(conversationTitle, this.transform);

        // 等待指定的时间
        yield return new WaitForSeconds(duration);

        // 再次检查barkUI，如果它仍在播放，则隐藏它
        if (barkUI != null && barkUI.isPlaying)
        {
            barkUI.Hide();
        }
    }

    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }
}