using UnityEngine;
using System;

/// <summary>
/// 小游戏基类，所有小游戏都应该继承此类
/// </summary>
public abstract class MiniGameBase : MonoBehaviour
{
    [Header("小游戏基础配置")]
    [Tooltip("小游戏名称")]
    [SerializeField] protected string gameName = "MiniGame";

    protected Transform uiContainer;
    protected GameObject uiInstance;
    protected PetController petController;
    
    public bool IsActive { get; protected set; }
    public bool IsInitialized { get; protected set; }

    /// <summary>
    /// 初始化小游戏
    /// </summary>
    public virtual void Initialize(Transform uiContainer)
    {
        this.uiContainer = uiContainer;
        IsInitialized = true;
        OnInitialize();
    }

    /// <summary>
    /// 启动小游戏
    /// </summary>
    public virtual void StartGame(PetController petController)
    {
        if (petController == null)
        {
            Debug.LogError($"[{gameName}] PetController为null，无法启动小游戏");
            return;
        }

        this.petController = petController;
        IsActive = true;
        OnStartGame();
    }

    /// <summary>
    /// 结束小游戏
    /// </summary>
    public virtual void EndGame()
    {
        if (!IsActive) return;

        IsActive = false;
        OnEndGame();

        // 清理UI实例
        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }

        petController = null;
    }

    // 抽象方法，由子类实现
    protected abstract void OnInitialize();
    protected abstract void OnStartGame();
    protected abstract void OnEndGame();

    // 虚拟方法，子类可以重写
    protected virtual void Update()
    {
        if (IsActive)
        {
            OnGameUpdate();
        }
    }

    protected virtual void OnGameUpdate() { }
}

