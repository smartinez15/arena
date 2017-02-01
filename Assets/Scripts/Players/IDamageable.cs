using UnityEngine;

public interface IDamageable
{
    void TakeHit(int damage, RaycastHit hit);
}
