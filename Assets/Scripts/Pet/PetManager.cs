// --- PetManager.cs ---
using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }
    
    public PetController ActivePet { get; private set; }
    public Transform petSpawnParent; // 宠物应该生成在哪个Canvas下

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnPet(PetProfileSO profile)
    {
        if (ActivePet != null)
        {
            Destroy(ActivePet.gameObject);
        }

        if (profile.petPrefab != null)
        {
            GameObject petInstance = Instantiate(profile.petPrefab, petSpawnParent);
            ActivePet = petInstance.GetComponent<PetController>();
            if (ActivePet != null)
            {
                ActivePet.Initialize(profile);
            }
            else
            {
                Debug.LogError($"预制体 {profile.petPrefab.name} 上没有找到 PetController 脚本!");
            }
        }
    }
}