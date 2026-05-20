using UnityEngine;

public interface ISpellTarget
{
    void OnPushSpell(Vector3 force, float pushForce);
    void OnFreezeSpell(float duration);
    void OnBounceSpell(float bounceHeight);
}
