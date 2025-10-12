// --- DialogueBridge.cs ---
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class DialogueBridge : MonoBehaviour
{
    void OnEnable()
    {
        // 注册C#函数到Lua环境中，让Dialogue System可以调用
        Lua.RegisterFunction("ChangeAffection", this, SymbolExtensions.GetMethodInfo(() => ChangeAffection(0)));
        Lua.RegisterFunction("UnlockBehavior", this, SymbolExtensions.GetMethodInfo(() => UnlockBehavior("")));
    }

    void OnDisable()
    {
        // 在对象销毁时注销，防止内存泄漏
        Lua.UnregisterFunction("ChangeAffection");
        Lua.UnregisterFunction("UnlockBehavior");
    }

    // --- 以下是可以在对话脚本中调用的函数 ---

    /// <summary>
    /// 在对话中改变宠物好感度。用法: ChangeAffection(10)
    /// </summary>
    public void ChangeAffection(double amount)
    {
        if (PetManager.Instance.ActivePet != null)
        {
            PetManager.Instance.ActivePet.Affection += (float)amount;
            Debug.Log($"好感度变化: {amount}. 当前: {PetManager.Instance.ActivePet.Affection}");
            
            // 使用Dialogue System的变量系统来记录好感度，以便存档和在对话中判断
            DialogueLua.SetVariable("Affection", PetManager.Instance.ActivePet.Affection);
        }
    }

    /// <summary>
    /// 在对话中解锁宠物的新行为。用法: UnlockBehavior("Dance")
    /// </summary>
    public void UnlockBehavior(string behaviorName)
    {
        if (PetManager.Instance.ActivePet != null)
        {
            Debug.Log($"尝试解锁行为: {behaviorName}");
            // 在这里实现具体的行为解锁逻辑
            // 例如: PetManager.Instance.ActivePet.BehaviorRegistry.Unlock(behaviorName);
        }
    }
}