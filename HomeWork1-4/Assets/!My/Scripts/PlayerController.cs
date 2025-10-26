using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerCharacter _player;
    [SerializeField] private float _sensativeMouse = 5;
    [SerializeField] private GunController _gunController;
    private bool _isActiveCursor = false;
    private bool _isSquat = false;
    private MultiplayerManager _multiplayerManager;


    private void Start()
    {
        SwithActivateCursor();
        _multiplayerManager = MultiplayerManager.Instance;  // это имеет какой-то прктичкский эффект кроме удобства написания/понимания? 
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) SwithActivateCursor();

        // передвижение 
        float acceleration = 1;
        if (Input.GetKey(KeyCode.LeftShift)) acceleration = 2;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _player.SetInput(h,v, acceleration);

        // поворот камеры
        float mouseX  = Input.GetAxis("Mouse X") * _sensativeMouse;
        float mouseY  = Input.GetAxis("Mouse Y") * _sensativeMouse;
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




    private void SendMessageShoot(ShootInfo info)
    {
        info.key = _multiplayerManager.GetSessionId();
        string json = JsonUtility.ToJson(info);
        _multiplayerManager.SendMessageShoot("shoot", json);
    }


    private void SendMoveToServer() // а что если отправлять на сервер сериализованную структуру? или это плохой вариант? 
    {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out bool isFly);

        Dictionary<string, object> data = new()
        {
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
        if(_isActiveCursor)
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


// что если использовать class? в каких случаях на практике используется класс, а не структура? 
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


