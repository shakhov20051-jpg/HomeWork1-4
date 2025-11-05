using UnityEngine;
using Colyseus;
using System;
using System.Collections.Generic;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [SerializeField] private TargetSpawnController _targetSpawnController;
    [SerializeField] private PlayerController _player;
    [SerializeField] private EnemyController _enemy;
    private Dictionary<string, EnemyController> _enemies = new();
    private ColyseusRoom<State> _room;
    private PlayerController playerPrefab;

    protected  override void  Awake()
    {
        base.Awake();

        Instance.InitializeClient();
        Connect();
    }

    private async void Connect()
    {
        Dictionary<string, object> data = new()
        {
            { "speed", _player.Speed },
            { "hp", _player.Health}
        };

        _room = await Instance.client.JoinOrCreate<State>("state_handler", data);

        _room.OnStateChange += OnChange;
        _room.OnMessage<string>("Shoot", ApplyShoot);
        _room.OnMessage<string>("Revive", SpawnPlayer);
        
        _room.OnMessage<CharacterPosition[]>("PlayersPositions", players => PlayerDie(players));
    }

    private void SpawnPlayer(string jsonShootInfo)
    {
        CharacterPosition newPosition = JsonUtility.FromJson<CharacterPosition>(jsonShootInfo);

        if (_enemies.ContainsKey(newPosition.id) == false) return;
        _enemies[newPosition.id].Respawn(newPosition);
    }

    private void PlayerDie(CharacterPosition[] players)
    {
        playerPrefab.RespawnAfterDie(players);
    }

    private void ApplyShoot(string jsonShootInfo)
    {
        if (jsonShootInfo == null) return;
        ShootInfo shootInfo = JsonUtility.FromJson<ShootInfo>(jsonShootInfo);
        if (_enemies.ContainsKey(shootInfo.key) == false)
        {
            Debug.LogError("Врага нет но он стреляет");
            return;
        }
        _enemies[shootInfo.key].Shoot(shootInfo);
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (isFirstState == false) return;

        state.players.ForEach((key, player) => 
            {
                if (key == _room.SessionId)
                {
                    CreatePlayer(key, player);
                }
                else CreateEnemy(key, player);
            }
        );

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;
    }


    private void CreatePlayer(string key, Player player)
    {
        Vector3 spawnPosition = _targetSpawnController.GetSpawnPositions().position;
        playerPrefab = Instantiate(_player, spawnPosition, Quaternion.identity);
        playerPrefab.Init(key, _targetSpawnController);
        player.OnChange += playerPrefab.OnChange;
    }

    private void CreateEnemy(string key, Player player)
    {
        var position = new Vector3(player.px, player.py, player.pz);
        EnemyController enemy = Instantiate(_enemy, position, Quaternion.identity);
        enemy.Init(key, player);        
        _enemies.Add(key, enemy);
    }

    internal string GetSessionId()
    {
        return _room.SessionId;
    }

    private void RemoveEnemy(string key, Player value)
    {
        if (_enemies.ContainsKey(key) == false) return;
        var enemy = _enemies[key];
        enemy.Destroy();
        _enemies.Remove(key);
    }

    public void SendMessageMove(string key, Dictionary<string, object> data)
    {
        _room.Send(key, data);
    }

    public void SendMessageToServer(string key, object data)
    {
        _room.Send(key, data);
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(_room != null)  _room.Leave();
    } 

}



