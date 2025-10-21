using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] PlayerCharacter _player;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        _player.SetInput(h,v);
        SendMoveToServer();
    }

    private void SendMoveToServer()
    {
        _player.GetMoveInfo(out Vector3 position);
        Dictionary<string, object> data = new()
        {
            { "x", position.x },
            { "y", position.z }
        };
        MultiplayerManager.Instance.SendMessage("move", data);
    }
}
