using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private Image _imageHP;
    [SerializeField] private EnemyController _enemyController;
    private int _max;
    private int _current;

    public void SetMax(int max)
    {
        _max = max;
    }

    public void SetCurrent(int current)
    {
        _current = current;
        if (_current < 0) _current = 0;
        UpdateHP();
    }

    public void ApplyDamage(int damage)
    {
        _enemyController.SendDamage(damage);
    }

    private void UpdateHP()
    {
        _imageHP.fillAmount = (float)_current / _max;
    }




}
