using UnityEngine;

public abstract class EnemyState : MonoBehaviour
{
    public virtual void EnterState(GameObject enemy) { }
    public abstract void UpdateState(GameObject enemy);
    public virtual void ExitState(GameObject enemy) { }
}
