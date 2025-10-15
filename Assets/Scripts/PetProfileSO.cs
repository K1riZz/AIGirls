
using UnityEngine;
using PixelCrushers.DialogueSystem; 

[CreateAssetMenu(fileName = "NewPetProfile", menuName = "DesktopPet/Pet Profile")]
public class PetProfileSO : ScriptableObject
{
    [Header("基本信息")]
    public string petID = "pet_001";
    public string petName = "宠物名称";
    public GameObject petPrefab; // 宠物的预制体
    public Sprite petAvatar;

    [Header("行为属性")]
    public float moveSpeed = 100f;
    public float idleTimeMin = 3f;
    public float idleTimeMax = 8f;

    [Header("对话系统集成")]
    public DialogueDatabase dialogueDatabase; // 该IP使用的对话数据库
    public string startConversationTitle; // 初始剧情对话的标题
    public string touchConversationTitle; // 点击时触发的闲聊对话标题
    [Tooltip("闲置时随机触发的对话标题列表")]
    public System.Collections.Generic.List<string> idleChatterTitles;
}