using UnityEngine;

public class LimbPart : MonoBehaviour
{
    [SerializeField] private int _multiplier = 1;
    [SerializeField] private Health _health ;

    public void TakeDamager(int damager)
    {
        _health.ApplyDamage(damager * _multiplier);
    }
}
