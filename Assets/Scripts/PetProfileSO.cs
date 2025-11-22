
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
    [Tooltip("桌宠漫游的概率（0-1之间，0表示不漫游，1表示总是漫游）")]
    [Range(0f, 1f)]
    public float wanderProbability = 0f; // 默认为0，暂时不进行漫游

    [Header("对话系统集成（旧版Dialogue System for Unity）")]
    public DialogueDatabase dialogueDatabase; // 该IP使用的对话数据库（旧版）
    public string startConversationTitle; // 初始剧情对话的标题（旧版）
    public string touchConversationTitle; // 点击时触发的闲聊对话标题（旧版）
    [Tooltip("点击触发的闲聊对话的显示时间（秒）")]
    public float touchConversationDuration = 4f; // 默认4秒
    [Tooltip("闲置时随机触发的对话标题列表")]
    public System.Collections.Generic.List<string> idleChatterTitles;

    [Header("新对话系统配置（GalDialogue System）")]
    [Tooltip("对话数据库JSON文件路径（相对于Resources文件夹，如：Dialogue/database.json）")]
    public string dialogueDatabasePath;
    [Tooltip("剧情对话UI预制体")]
    public GameObject storyDialogueUIPrefab;
    [Tooltip("气泡对话UI预制体")]
    public GameObject bubbleDialogueUIPrefab;
    [Tooltip("选择对话UI预制体")]
    public GameObject choiceDialogueUIPrefab;
    [Tooltip("历史记录UI预制体")]
    public GameObject historyUIPrefab;
    [Tooltip("初始对话节点ID")]
    public string startDialogueNodeID;
    [Tooltip("点击对话节点ID")]
    public string touchDialogueNodeID;
    [Tooltip("闲置对话节点ID列表")]
    public System.Collections.Generic.List<string> idleDialogueNodeIDs;
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

    [Header("剧情模式配置")]
    [Tooltip("桌面壁纸Sprite（剧情模式的房间背景）")]
    public Sprite desktopWallpaper;
    [Tooltip("桌宠在剧情模式中的目标位置（相对于屏幕中心，0,0表示屏幕中心）")]
    public Vector2 storyModePetPosition = Vector2.zero;
    [Tooltip("桌宠移动到剧情模式位置的动画时长（秒）")]
    public float storyModeMoveDuration = 1f;
}