using Colyseus.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyCharacter _enemyCharacter;
    [SerializeField] private Animator _animatorFoot;
    [SerializeField] private Animator _animatorSquat;
    [SerializeField] private Animator _animatorGun;
    [SerializeField] private Bullet _bulletPrefab;

    private List<float> _receiveTimeInterval = new();
    private float _lastReceiveTime = 0;
    private bool _isFly = true;
    private Player _player;
    private bool _isCurrentSquat = false;
    private Vector3 _velocity = Vector3.zero;
    private float _maxSpeed = 10f;


    public void Init(Player player)
    {
        _player = player;
        _maxSpeed = _player.speed;

        _isFly = _player.fly;
        _animatorFoot.SetBool("Grounded", !_isFly);

        _isCurrentSquat = _player.sq; 
        _animatorSquat.SetBool("IsSquat", _isCurrentSquat);

        _enemyCharacter.SetStartRotate(_player.rx, _player.ry);

        _player.OnChange += OnChange;
    }


    private void Start()
    {
        SaveReceiveTime();
    }

    private void Update()
    {
        _animatorFoot.SetFloat("Speed", _velocity.magnitude / _maxSpeed); 
    }

    public void Shoot(in ShootInfo info)
    {
        _animatorGun.SetTrigger("Shoot");
        Instantiate(_bulletPrefab).Init(new Vector3(info.px, info.py, info.pz), new Vector3(info.dx, info.dy, info.dz));
    }

    public void OnChange(List<DataChange> changes)
    {
        SaveReceiveTime();

        Vector3 newPosition = transform.position;
        _velocity = _enemyCharacter.Velocity; 

        _isFly = true; 

        foreach (var dataChange in changes)
        {
            switch (dataChange.Field)
            {
                case "px":
                    newPosition.x = (float)dataChange.Value;
                    break;
                case "py":
                    newPosition.y = (float)dataChange.Value;
                    break;
                case "pz":
                    newPosition.z = (float)dataChange.Value;
                    break;

                case "vx":
                    _velocity.x = (float)dataChange.Value;
                    break;
                case "vy":
                    _velocity.y = (float)dataChange.Value;
                    break;
                case "vz":
                    _velocity.z = (float)dataChange.Value;
                    break;

                case "rx":
                    _enemyCharacter.SetRotateX((float)dataChange.Value);
                    break;
                case "ry":
                    _enemyCharacter.SetRotateY((float)dataChange.Value);
                    break;

                case "fly":
                    _animatorFoot.SetBool("Grounded", !(bool)dataChange.Value);
                    break;

                case "sq":
                    if (_isCurrentSquat == (bool)dataChange.Value) break;
                    _isCurrentSquat = (bool)dataChange.Value;
                    _animatorSquat.SetBool("IsSquat", _isCurrentSquat);
                    break;

                default:
                    Debug.Log("не обработалось поле " + dataChange.Field);
                    break;
            }
        }

        _enemyCharacter.SetMovement(newPosition, _velocity, _receiveTimeInterval.Average());
    }

    private void SaveReceiveTime()
    {
        float interval = Time.time - _lastReceiveTime;
        _lastReceiveTime = Time.time;
        _receiveTimeInterval.Add(interval);
        if (_receiveTimeInterval.Count > 5) _receiveTimeInterval.RemoveAt(0);
    }


    public void Destroy()
    {
        _player.OnChange -= OnChange;
        Destroy(gameObject);
    }
}
