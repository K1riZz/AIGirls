// --- PetStateMachine.cs ---
using UnityEngine;

public class PetStateMachine : MonoBehaviour
{
    public PetBaseState CurrentState { get; private set; }
    private PetController controller;

    public void Initialize(PetController petController)
    {
        this.controller = petController;
        SwitchState(new IdleState(controller));
    }

    void Update()
    {
        CurrentState?.Update();
    }

    public void SwitchState(PetBaseState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}

// --- PetBaseState.cs ---
public abstract class PetBaseState
{
    protected PetController controller;
    protected PetStateMachine stateMachine;

    public PetBaseState(PetController controller)
    {
        this.controller = controller;
        this.stateMachine = controller.StateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}