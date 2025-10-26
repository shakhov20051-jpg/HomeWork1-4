using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    [SerializeField] private Transform _head;

    private Vector3 _targetPositon = Vector3.zero;
    private float _velocityMagnitude = 0;
    private Vector3 _velocity = Vector3.zero;

    public Vector3 Velocity => _velocity;

    private float _rotateX = 0;
    private float _rotateY = 0;

    public void SetStartRotate(float rotateX, float rotateY)
    {
        rotateX = NormalizeAngle(rotateX);
        _rotateX = Mathf.Clamp(rotateX, -90f, 90f);
        _rotateY = NormalizeAngle(rotateY);
        _head.transform.localRotation = Quaternion.Euler(_rotateX, 0, 0);
        transform.localEulerAngles = new Vector3(0, rotateY, 0);
    }


    private void Start()
    {
        _targetPositon = transform.position;
    }


    private void Update()
    {
        SmoothMove();
        RotateX();
        RotateY();
    }


    private void SmoothMove()
    {
        if (_velocityMagnitude > 0.1f)
        {
            float maxDistance = _velocityMagnitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetPositon, maxDistance);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _targetPositon, 20 * Time.deltaTime);
        }
    }
    private void RotateX()
    {
        Quaternion currentRotation = _head.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(_rotateX, 0, 0);
        _head.transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, 20f * Time.deltaTime);
    }
    private void RotateY()
    {
        Quaternion currentRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, _rotateY, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, 20f * Time.deltaTime);
    }


    // если оставить предсказание где должен быть через время то не редко заходит в коллейдеры, есть видео в тг general
    public void SetMovement(in Vector3 postion, in Vector3 velocity, in float averageInterval)
    {
        _targetPositon = postion;// + velocity * averageInterval;
        _velocity = velocity;
        _velocityMagnitude = _velocity.magnitude;
    }

    public void SetRotateX(float rotateX)
    {
        rotateX = NormalizeAngle(rotateX);
        _rotateX = Mathf.Clamp(rotateX, -90f, 90f);
    }
    public void SetRotateY(float rotateY)
    {
        _rotateY = NormalizeAngle(rotateY);
    }

    
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

}