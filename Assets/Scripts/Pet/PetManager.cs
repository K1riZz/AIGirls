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
