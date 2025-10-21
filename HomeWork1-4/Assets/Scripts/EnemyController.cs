using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector3 previousPosition;
    private Vector3 targetPosition;
    private float lastUpdateTime;
    private float interpolationDuration = 0.1f; // в дальнейшем получить пинг от сервера и устанавливаться точное значение 


    private void Start()
    {
        previousPosition = transform.position;
        targetPosition = transform.position;
        lastUpdateTime = Time.time;
    }

    internal void OnChange(List<DataChange> changes)
    {
        Vector3 newPosition = targetPosition;
        foreach (var dataChange in changes)
        {
            switch (dataChange.Field)
            {
                case "x":
                    newPosition.x = (float)dataChange.Value;
                    previousPosition.x = (float)dataChange.PreviousValue;
                    break;

                case "y":
                    newPosition.z = (float)dataChange.Value;
                    previousPosition.z = (float)dataChange.PreviousValue;
                    break;

                default:
                    Debug.Log("не обработалось поле " + dataChange.Field);
                    break;
            }
        }

        targetPosition = newPosition;
        lastUpdateTime = Time.time;
    }

    private void Update()
    {
        float t = (Time.time - lastUpdateTime) / interpolationDuration;
        t = Mathf.Clamp01(t);
        transform.position = Vector3.Lerp(previousPosition, targetPosition, t);
    }

}
