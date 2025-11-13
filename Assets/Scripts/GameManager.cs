using UnityEngine;
using PixelCrushers.DialogueSystem;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PetProfileSO currentPetProfile; // 在Inspector中指定默认的宠物

    private float hourlyCheckTimer = 0f;
    private const float HOURLY_CHECK_INTERVAL = 3600f; // 每小时检查一次

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 添加Windows控制器，用于处理桌面化逻辑
        // 确保我们添加的是正确的、唯一的WindowsController
        if (GetComponent<WindowsController>() == null) {
            gameObject.AddComponent<WindowsController>();
        }
        
        // 添加桌面输入追踪器，用于记录鼠标键盘次数
        if (GetComponent<DesktopInputTracker>() == null)
        {
            gameObject.AddComponent<DesktopInputTracker>();
        }

        // 订阅对话结束事件，用于从剧情模式切换回桌面模式
        // 使用 += 来订阅事件
        PixelCrushers.DialogueSystem.DialogueManager.Instance.conversationEnded += OnConversationEnded;
       
    }

    void Start()
    {
        // 游戏开始时，根据当前选择的Profile生成宠物
        // 确保PetManager实例存在
        if (currentPetProfile != null && PetManager.Instance != null)
        {
            PetManager.Instance.SpawnPet(currentPetProfile);
            
            // 宠物生成后，立即为它设置初始的活动范围
            if (PetManager.Instance.ActivePet != null)
            {
                PetManager.Instance.ActivePet.UpdateWalkableArea();
            }
        }
    }

    void OnDestroy()
    {
        // 在GameManager销毁时，取消订阅事件以防止内存泄漏
        // 增加一个空值检查，因为DialogueManager可能已经被销毁
        if (PixelCrushers.DialogueSystem.DialogueManager.instance != null) {
            PixelCrushers.DialogueSystem.DialogueManager.Instance.conversationEnded -= OnConversationEnded;
        }
    }

    void Update()
    {
        // 每小时检查一次
        hourlyCheckTimer += Time.deltaTime;
        if (hourlyCheckTimer >= HOURLY_CHECK_INTERVAL)
        {
            hourlyCheckTimer = 0;
            CheckTimeBasedEvents();
        }
    }

    void CheckTimeBasedEvents()
    {
        // 检查当前是否是特定时间点，例如整点
        DateTime now = DateTime.Now;
        if (now.Minute == 0) // 如果是整点
        {
            // 触发一个名为 "Hourly_Chime" 的对话
            // 你可以在对话数据库中创建这个对话，并根据Lua变量(例如好感度)来决定具体说什么
            PixelCrushers.DialogueSystem.DialogueManager.StartConversation("Hourly_Chime");
        }
    }

    public void SaveGame()
    {
        // 使用Dialogue System的存档系统来保存变量（如好感度等）
        string saveData = PersistentDataManager.GetSaveData();
        PlayerPrefs.SetString("PetSaveData", saveData);
        PlayerPrefs.Save();
        Debug.Log("游戏已保存!");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("PetSaveData"))
        {
            string saveData = PlayerPrefs.GetString("PetSaveData");
            PersistentDataManager.ApplySaveData(saveData);
            Debug.Log("游戏已加载!");
        }
    }

    /// <summary>
    /// 当任何一个正式对话（Conversation）结束时，此方法会被调用。
    /// </summary>
    private void OnConversationEnded(Transform actor)
    {
        Debug.Log("剧情模式结束，返回桌面模式...");
        var pet = PetManager.Instance.ActivePet;
        if (pet == null) return;

        // 重新启用桌面模式AI和交互
        if (pet.StateMachine != null) pet.StateMachine.enabled = true;
        if (pet.GetComponent<PetInteraction>() != null) pet.GetComponent<PetInteraction>().enabled = true;

        // 关键：返回桌面模式时，立即重新计算并更新宠物的桌面活动范围
        pet.UpdateWalkableArea();
    }
}