using Colyseus.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyCharacter _enemyCharacter;
    [SerializeField] private Health _health;
    [SerializeField] private Animator _animatorFoot;
    [SerializeField] private Animator _animatorSquat;
    [SerializeField] private Animator _animatorGun;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _damage = 1;
    [SerializeField] private GameObject _dieObject;

    private List<float> _receiveTimeInterval = new();
    private float _lastReceiveTime = 0;
    private bool _isFly = true;
    private Player _player;
    private bool _isCurrentSquat = false;
    private Vector3 _velocity = Vector3.zero;
    private float _maxSpeed = 10f;
    private string _sessionId;


    public void Init(string sessionId, Player player)
    {
        _sessionId = sessionId;
        _player = player;
        _maxSpeed = _player.speed;
        _health.SetMax(_player.hpMax);
        _health.SetCurrent(_player.hpCurrent);

        _isFly = _player.fly;
        _animatorFoot.SetBool("Grounded", !_isFly);

        _isCurrentSquat = _player.sq; 
        _animatorSquat.SetBool("IsSquat", _isCurrentSquat);

        _enemyCharacter.SetStartRotate(_player.rx, _player.ry);

        _player.OnChange += OnChange;
    }

    
    public void SendDamage(int damage)
    {
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "id", _sessionId },
            { "value", damage },
        };
        MultiplayerManager.Instance.SendMessageToServer("damage", data);
    }
   

    private void Start()
    {
        SaveReceiveTime();
    }

    private void Update()
    {
        _animatorFoot.SetFloat("Speed", _velocity.magnitude / _maxSpeed); 
    }


    public void Respawn(CharacterPosition newPosition)
    {
        Vector3 position = new Vector3(newPosition.px, newPosition.py, newPosition.pz);
        _enemyCharacter.Respawn(position);
    }


    public void Shoot(in ShootInfo info)
    {
        _animatorGun.SetTrigger("Shoot");
        Instantiate(_bulletPrefab).Init(new Vector3(info.px, info.py, info.pz), new Vector3(info.dx, info.dy, info.dz), _damage);
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
                case "hpCurrent":
                        _health.SetCurrent((sbyte)dataChange.Value);
                        // if ((sbyte)dataChange.Value <= 0) _dieObject.SetActive(true); // temp for test
                        return;
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
