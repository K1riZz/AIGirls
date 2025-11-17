
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
    [Tooltip("点击触发的闲聊对话的显示时间（秒）")]
    public float touchConversationDuration = 4f; // 默认4秒
    [Tooltip("闲置时随机触发的对话标题列表")]
    public System.Collections.Generic.List<string> idleChatterTitles;
    [Header("闲置闲聊")]
    [Tooltip("触发闲置闲聊的最小间隔时间（秒）")]
    public float idleChatterIntervalMin = 15f;
    [Tooltip("触发闲置闲聊的最大间隔时间（秒）")]
    public float idleChatterIntervalMax = 45f;

    [Header("小游戏系统 - 快速点击检测")]
    [Tooltip("触发小游戏所需的点击次数")]
    public int rapidClickRequiredClicks = 10;
    [Tooltip("检测快速点击的时间窗口（秒）")]
    public float rapidClickTimeWindow = 2f;
    [Tooltip("两次点击之间的最大时间间隔（秒），超过此时间则重置计数")]
    public float rapidClickMaxInterval = 0.5f;

    [Header("小游戏系统 - 泡泡消除")]
    [Tooltip("游戏时长（秒）")]
    public float bubbleGameDuration = 30f;
    [Tooltip("泡泡初始数量")]
    public int bubbleInitialCount = 15;
    [Tooltip("泡泡最大数量")]
    public int bubbleMaxCount = 25;
    [Tooltip("泡泡生成间隔（秒）")]
    public float bubbleSpawnInterval = 1.5f;
    [Tooltip("泡泡大小范围")]
    public Vector2 bubbleSizeRange = new Vector2(50f, 120f);
    [Tooltip("泡泡颜色列表")]
    public Color[] bubbleColors = new Color[]
    {
        new Color(1f, 0.5f, 0.5f, 0.8f), // 红色
        new Color(0.5f, 1f, 0.5f, 0.8f), // 绿色
        new Color(0.5f, 0.5f, 1f, 0.8f), // 蓝色
        new Color(1f, 1f, 0.5f, 0.8f),   // 黄色
        new Color(1f, 0.5f, 1f, 0.8f),   // 粉色
        new Color(0.5f, 1f, 1f, 0.8f),   // 青色
    };
    [Tooltip("泡泡精灵（开发者可自定义）")]
    public Sprite bubbleSprite;
    [Tooltip("UI文本字体（如果为空则使用默认字体）")]
    public TMPro.TMP_FontAsset uiFontAsset;
}