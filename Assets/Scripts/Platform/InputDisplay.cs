using UnityEngine;
using TMPro;

/// <summary>
/// 更新UI以显示总的输入次数。
/// </summary>
public class InputDisplay : MonoBehaviour
{
    [Tooltip("用于显示总输入次数的TextMeshPro-Text组件")]
    public TextMeshProUGUI totalInputsText;

    private float updateInterval = 0.1f; // 每0.5秒更新一次UI，避免过于频繁
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (totalInputsText != null)
        {
            totalInputsText.text = $"{DesktopInputTracker.TotalInputs}";
        }
    }
}
