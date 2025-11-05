using Colyseus.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private int _maxHP = 10;
    public float Speed => _speed;
    public int Health => _maxHP;

    [SerializeField] private GameObject _dieObject;
    [SerializeField] private Image _imageHP;
    [SerializeField] PlayerCharacter _player;
    [SerializeField] private float _sensativeMouse = 5;
    [SerializeField] private GunController _gunController;

    private TargetSpawnController _targetSpawnController;
    private bool _isActiveCursor = false;
    private bool _isSquat = false;
    private MultiplayerManager _multiplayerManager;
    private string _idSession;


    private void Awake()
    {
        _player.Init(_speed);
        SwithActivateCursor();
        _multiplayerManager = MultiplayerManager.Instance;  // это имеет какой-то прктичкский эффект кроме удобства написани€/понимани€? 
        UpdateHealth(_maxHP);
    }


    public void Init(string idSession, TargetSpawnController targetSpawnController)
    {
        _targetSpawnController = targetSpawnController;
        _idSession = idSession;
        
        // SendMessageSpawn(spawnPosition);
    }


    // вызывает MultyplayerManager
    public void RespawnAfterDie(CharacterPosition[] players) 
    {
        List<Transform> spawnPoint = GetPointToRespawn(players);

        int positionRandom = UnityEngine.Random.Range(0, spawnPoint.Count);
        Vector3 spawnPosition = spawnPoint[positionRandom].position;
        StartCoroutine(SpawnPlayer(spawnPosition));
    }


    public List<Transform> GetPointToRespawn(CharacterPosition[] players)
    {
        List<Vector3> enemyPosition = new();
        
        foreach (var enemy in players)
        {
            if (enemy.id == _idSession) continue;
            enemyPosition.Add(new Vector3(enemy.px, enemy.py, enemy.pz));
        }

        return _targetSpawnController.GetSpawnPositionsSelfEnemy(enemyPosition);
    }


    private IEnumerator SpawnPlayer(Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(3f);
        SendMessageSpawn(spawnPosition);
    }

    private void SendMessageSpawn(Vector3 spawnPosition)
    {
        CharacterPosition positon = new()
        {
            id = _idSession,
            px = spawnPosition.x,
            py = spawnPosition.y,
            pz = spawnPosition.z,
        };

        string json = JsonUtility.ToJson(positon);

        Dictionary<string, object> data = new()
        {
            { "id", _idSession },
            { "json", json },
        };

        _multiplayerManager.SendMessageToServer("revive", data);

        _dieObject.SetActive(false);
        _player.Respawn(spawnPosition);
        UpdateHealth(_maxHP);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) SwithActivateCursor();

        // передвижение 
        float acceleration = 1;
        if (Input.GetKey(KeyCode.LeftShift)) acceleration = 2;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _player.SetInput(h, v, acceleration);

        // поворот камеры
        float mouseX = Input.GetAxis("Mouse X") * _sensativeMouse;
        float mouseY = Input.GetAxis("Mouse Y") * _sensativeMouse;
        _player.RotateX(-mouseY);
        _player.RotateY(mouseX);

        // прыжок 
        if (Input.GetKeyDown(KeyCode.Space)) _player.Jump();

        // приседание
        _isSquat = Input.GetKey(KeyCode.LeftControl);
        _player.Squat(_isSquat);

        // стрельба
        if (Input.GetMouseButton(0)) _gunController.TryShoot(SendMessageShoot);

        SendMoveToServer();
    }


    public void OnChange(List<DataChange> changes)
    {
        foreach (var dataChange in changes)
            switch (dataChange.Field)
            {
                case "hpCurrent":
                    UpdateHealth((sbyte)dataChange.Value);
                    if ((sbyte)dataChange.Value <= 0) _dieObject.SetActive(true);
                    break;
                default:
                    break;
            }
    }

    private void UpdateHealth(int currentHP)
    {
        _imageHP.fillAmount = (float)currentHP / _maxHP;
    }


    private void SendMessageShoot(ShootInfo info)
    {
        info.key = _multiplayerManager.GetSessionId();
        string json = JsonUtility.ToJson(info);

        Dictionary<string, object> data = new()
        {
            { "id", _idSession },
            { "json", json },
        };

        _multiplayerManager.SendMessageToServer("shoot", data);
    }


    private void SendMoveToServer() // а что если отправл€ть на сервер сериализованную структуру? или это плохой вариант? 
    {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out bool isFly);

        Dictionary<string, object> data = new()
        {
            { "id", _idSession },
            { "px", position.x },
            { "py", position.y },
            { "pz", position.z },
            { "vx", velocity.x },
            { "vy", velocity.y },
            { "vz", velocity.z },
            { "rx", rotateX },
            { "ry", rotateY },
            { "fly", isFly },
            { "sq", _isSquat }
        };
        _multiplayerManager.SendMessageMove("move", data);
    }


    // вкл/выкл курсора
    private void SwithActivateCursor()
    {
        if (_isActiveCursor)
        {
            _isActiveCursor = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            _isActiveCursor = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

}


// что если использовать class? в каких случа€х на практике используетс€ класс, а не структура? 
//[Serializable]
//public class ShootInfo
//{
//public string key;
//    public float px;
//    public float py;
//    public float pz;
//    public float dx;
//    public float dy;
//    public float dz;
//}

[Serializable]
public struct ShootInfo
{
    public string key;
    public float px;
    public float py;
    public float pz;
    public float dx;
    public float dy;
    public float dz;
}


[Serializable]
public class CharacterPosition
{
    public string id;
    public float px;
    public float py;
    public float pz;

    public CharacterPosition() { }
}