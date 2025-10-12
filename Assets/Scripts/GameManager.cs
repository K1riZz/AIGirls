// --- GameManager.cs ---
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public PetProfileSO currentPetProfile; // 在Inspector中指定默认的宠物

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