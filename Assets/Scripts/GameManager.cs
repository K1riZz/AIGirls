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
    }

    void Start()
    {
        // 游戏开始时，根据当前选择的Profile生成宠物
        if (currentPetProfile != null)
        {
            PetManager.Instance.SpawnPet(currentPetProfile);
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
}