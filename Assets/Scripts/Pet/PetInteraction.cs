using UnityEngine;
using UnityEngine.EventSystems;

public class PetInteraction : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PetController controller;

    void Awake()
    {
        controller = GetComponent<PetController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pet Clicked!");
        // 只在非拖拽状态下响应点击
        if (!eventData.dragging)
        {
            controller.OnClicked();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Pet Draged!");
        controller.OnBeginDrag();
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