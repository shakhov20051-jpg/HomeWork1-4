using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetSpawnController : MonoBehaviour
{
    [SerializeField] private Transform[] _targetPositions;
    [SerializeField] private float _safeSpawnDistance;


    // выбираем те точки которые дальше безопасного расстояния если нето то беру одну где дальше всего игрок 
    public List<Transform> GetSpawnPositionsSelfEnemy(List<Vector3> enemyPositions)
    {
        List<Transform> validSpawns = _targetPositions.ToList();

        foreach (var enemy in enemyPositions)
        {
            foreach (var spawnPoint in _targetPositions)
            {
                if (Vector3.Distance(enemy, spawnPoint.position) < _safeSpawnDistance)
                {
                    validSpawns.Remove(spawnPoint);
                }
            }
        }

        if(validSpawns.Count == 0)
        {
            Transform bestSpawn = null;
            float bestDistance = float.MinValue;

            foreach (var spawnPoint in _targetPositions)
            {
                float minDistanceToEnemy = float.MaxValue;

                foreach (var enemy in enemyPositions)
                {
                    float dist = Vector3.Distance(enemy, spawnPoint.position);
                    if (dist < minDistanceToEnemy) minDistanceToEnemy = dist;
                }

                if (minDistanceToEnemy > bestDistance)
                {
                    bestDistance = minDistanceToEnemy;
                    bestSpawn = spawnPoint;
                }
            }
            validSpawns.Add(bestSpawn);
        }

        return validSpawns;
    }


    public Transform GetSpawnPositions()
    {
        return _targetPositions[Random.Range(0, _targetPositions.Length)];
    }
    

    private void OnDrawGizmos()
    {
        if (_targetPositions == null) return;

        Gizmos.color = new Color(0, 1, 0, 0.8f); // полупрозрачный зелёный

        foreach (var pos in _targetPositions)
        {
            if (pos == null) continue;
            Gizmos.DrawWireSphere(pos.position, _safeSpawnDistance);
        }
    }


}
