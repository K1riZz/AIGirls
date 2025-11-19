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
            // 查找Pet的根对象（MainCanvas prefab实例）
            // MainCanvas prefab被实例化后，应该作为场景的根对象
            Transform petRoot = ActivePet.transform;
            while (petRoot.parent != null && petRoot.parent.name != "MainCanvas")
            {
                petRoot = petRoot.parent;
            }
            
            // 如果找到了MainCanvas，销毁整个MainCanvas实例
            // 否则销毁PetController所在的GameObject
            if (petRoot != null && petRoot.name == "MainCanvas")
            {
                Destroy(petRoot.gameObject);
            }
            else
            {
                // 向上查找直到找到根对象或MainCanvas
                while (petRoot.parent != null)
                {
                    petRoot = petRoot.parent;
                }
                Destroy(petRoot.gameObject);
            }
        }

        if (profile.petPrefab != null)
        {
            // MainCanvas prefab被实例化为场景的根对象（不设置parent）
            // 因为MainCanvas prefab本身就包含了完整的Canvas和Pet结构
            GameObject petInstance = Instantiate(profile.petPrefab);
            
            // MainCanvas prefab应该作为场景根对象，不设置为任何对象的子对象
            // 这样MainCanvas prefab和WallpaperCanvas（如果存在）是平行的
            petInstance.transform.SetParent(null);
            
            Debug.Log($"[PetManager] MainCanvas prefab已实例化为场景根对象: {petInstance.name}");

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
