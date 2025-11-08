// 文件路径: d:\UnityProject\AIGirl\Assets\Scripts\Pet\PetInputHandler.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class PetInputHandler : MonoBehaviour
{
    private TMP_InputField inputField;
    private PetController petController;
    private bool wasFocused = false;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        // 从父级或根对象找到PetController
        petController = GetComponentInParent<PetController>();

        if (petController == null)
        {
            Debug.LogError("PetInputHandler 无法找到 PetController!", this);
            enabled = false;
        }

        // 添加对onSubmit事件的监听
        inputField.onSubmit.AddListener(OnSubmit);
    }

    void Update()
    {
        // 检查输入框是否刚刚获得焦点
        if (inputField.isFocused && !wasFocused)
        {
            // 如果是，则立即停止宠物的移动
            if (petController != null)
            {
                petController.StopMovement();
            }
        }
        wasFocused = inputField.isFocused;

        // 持续更新宠物的状态，告知其玩家是否正在输入
        if (petController != null)
        {
            petController.IsPlayerTyping = inputField.isFocused;
        }
    }

    private void SendMessage()
    {
        string message = inputField.text;

        // 如果消息不为空，则发送
        if (!string.IsNullOrWhiteSpace(message))
        {
            petController.ShowPlayerBark(message);
        }

        // 清空输入框并取消焦点
        inputField.text = "";
    }

    /// <summary>
    /// 公共方法，用于从UI按钮触发发送消息。
    /// </summary>
    public void TriggerSend()
    {
        SendMessage();
        // 发送后重新激活输入框，方便连续输入
        inputField.ActivateInputField();
    }

    /// <summary>
    /// 当输入框提交时（例如按下回车键）调用此方法。
    /// </summary>
    /// <param name="text">输入框中的文本</param>
    private void OnSubmit(string text)
    {
        // 确保只有在输入框仍然有焦点时才发送消息
        // 这可以防止在取消焦点时（例如点击其他地方）也触发发送
        if (inputField.isFocused)
        {
            SendMessage();
        }
    }

    /// <summary>
    /// 当输入框被激活时，确保它被选中
    /// </summary>
    void OnEnable()
    {
        if (inputField != null)
        {
            inputField.ActivateInputField();
            inputField.Select();
        }
    }
}
