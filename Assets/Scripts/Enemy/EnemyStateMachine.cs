using UnityEngine;

/// <summary>
/// Lightweight state machine for enemy AI.
/// Add this as a component on the enemy root GameObject.
/// EnemyBase auto-discovers it in Awake().
/// 
/// Provides:
/// - State definitions (Idle, Patrol, Chase, Attack, ReturnToPatrol, Dead)
/// - Clean transition method with enter/exit events
/// - Previous state tracking for context-aware behaviour
/// - Debug display in the Inspector
/// 
/// States:
/// - Idle:           enemy stands still, waiting
/// - Patrol:         follows waypoints
/// - Chase:          pursues detected player
/// - Attack:         performs attack on player
/// - ReturnToPatrol: walks back to patrol route after losing player
/// - Dead:           death state, no updates (final state)
/// </summary>
public class EnemyStateMachine : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        ReturnToPatrol,
        Dead
    }

    [Header("State Machine")]
    [SerializeField] private EnemyState currentState = EnemyState.Idle;
    [SerializeField] private EnemyState previousState = EnemyState.Idle;

    // Events fired when a state transition happens
    public System.Action<EnemyState> OnStateEnter;
    public System.Action<EnemyState> OnStateExit;

    public EnemyState CurrentState => currentState;
    public EnemyState PreviousState => previousState;

    /// <summary>
    /// Transition to a new state.
    /// Fires OnStateExit for the old state, then OnStateEnter for the new one.
    /// Dead is a final state — no transitions out of it.
    /// </summary>
    public void TransitionTo(EnemyState newState)
    {
        if (currentState == newState) return;
        if (currentState == EnemyState.Dead) return; // Dead is final

        previousState = currentState;

        OnStateExit?.Invoke(currentState);

        currentState = newState;

        OnStateEnter?.Invoke(currentState);
    }

    /// <summary>
    /// Returns true if the enemy is in an active (non-dead) state.
    /// </summary>
    public bool IsAlive => currentState != EnemyState.Dead;

    /// <summary>
    /// Returns true if the enemy is currently in the given state.
    /// </summary>
    public bool IsInState(EnemyState state) => currentState == state;

    /// <summary>
    /// Returns true if the enemy was in the given state before the last transition.
    /// </summary>
    public bool WasInState(EnemyState state) => previousState == state;
}
