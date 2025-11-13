using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }

    public PetController ActivePet { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 如果当前没有激活的宠物，则尝试在场景中查找一个
        if (ActivePet == null)
        {
            ActivePet = FindObjectOfType<PetController>();
            // 注意：此时ActivePet的初始化依赖于其自身的Start()和initialProfile
        }
    }

    public void SpawnPet(PetProfileSO profile)
    {
        if (ActivePet != null)
        {
            Destroy(ActivePet.gameObject);
        }

        if (profile.petPrefab != null)
        {
            GameObject petInstance = Instantiate(profile.petPrefab);
            // PetController现在位于子对象上，所以使用GetComponentInChildren
            ActivePet = petInstance.GetComponentInChildren<PetController>();
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
