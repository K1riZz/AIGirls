using UnityEngine;
using UnityEngine.EventSystems;

public class PetInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PetController controller;

    void Awake()
    {
        // 由于此脚本现在位于子对象上，需要向上查找父对象来获取PetController
        controller = GetComponentInParent<PetController>();
        if (controller == null) {
            Debug.LogError("PetInteraction 错误: 未能在父级对象中找到 PetController! 请确认层级结构是否正确。", this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pet Clicked!");
        if (eventData.dragging) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键点击
            controller.OnClicked();
            Debug.Log("Left Button Clicked!");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 右键点击
            controller.OnRightClicked();
            Debug.Log("Right Button Clicked!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 只有当鼠标左键按下时，才识别为开始拖拽
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Pet Drag Started with Left Button!");
            controller.OnBeginDrag();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 在DraggedState中处理位置更新
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 在DraggedState中处理状态切换
    }
}