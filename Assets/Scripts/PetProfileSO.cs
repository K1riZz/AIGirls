// --- PetProfileSO.cs ---
using UnityEngine;
using PixelCrushers.DialogueSystem; // Dialogue System的命名空间

[CreateAssetMenu(fileName = "NewPetProfile", menuName = "DesktopPet/Pet Profile")]
public class PetProfileSO : ScriptableObject
{
    [Header("基本信息")]
    public string petID = "pet_001";
    public string petName = "小派蒙";
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
}