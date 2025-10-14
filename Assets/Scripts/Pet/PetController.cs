// --- PetController.cs ---
using UnityEngine;
using PixelCrushers.DialogueSystem;
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

        // 设定模拟的桌面范围 (基于父Canvas)
        var parentRect = transform.parent.GetComponent<RectTransform>().rect;
        WalkableArea = new Rect(parentRect.xMin, parentRect.yMin, parentRect.width, parentRect.height);

        // 初始化状态机
        StateMachine.Initialize(this);
    }

    // 由PetInteraction调用
    public void OnClicked()
    {
        if (Profile == null)
        {
            Debug.LogError("PetController.Profile is null! Make sure it's initialized correctly from PetManager. Check if GameManager has a PetProfile assigned.", this);
            return;
        }

        // 触发一个简单的对话
        if (!string.IsNullOrEmpty(Profile.touchConversationTitle))
        {
            Debug.Log($"Starting conversation: {Profile.touchConversationTitle}");
            PixelCrushers.DialogueSystem.DialogueManager.StartConversation(Profile.touchConversationTitle, this.transform);
        }
    }

    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }
}