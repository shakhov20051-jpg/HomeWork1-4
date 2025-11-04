using UnityEngine;
using System;

public class GunController : MonoBehaviour
{
    [SerializeField] private Bullet _bulletPlayer;
    [SerializeField] private Transform _bulletCreatePosition;
    [SerializeField] private Animator _animator;
    [SerializeField] private int _damage = 1;
    private float _speedBullet = 50f;
    private bool _isCanShoot = true;

    private ShootInfo _infoShoot = new();

    public void TryShoot(Action<ShootInfo> shoot)
    {
        if (_isCanShoot == false) return;

        _isCanShoot = false;
        _animator.SetTrigger("Shoot");
        Vector3 velocity = _bulletCreatePosition.forward * _speedBullet;
        Instantiate(_bulletPlayer).Init(_bulletCreatePosition.position, velocity, _damage);

        _infoShoot.px = _bulletCreatePosition.position.x;
        _infoShoot.py = _bulletCreatePosition.position.y;
        _infoShoot.pz = _bulletCreatePosition.position.z;
        _infoShoot.dx = velocity.x;
        _infoShoot.dy = velocity.y;
        _infoShoot.dz = velocity.z;

        shoot?.Invoke(_infoShoot);
    }

    public void EndAnimationShoot()
    {
        _isCanShoot = true;
    }


}



