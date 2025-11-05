using System;
using System.Collections;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _head;
    [SerializeField] private Animator _animatorFoot;
    [SerializeField] private Animator _animatorSquat; 
    [SerializeField] private Transform _pointCamera;

    // скорость 
    private float _speed = 10f;
 //   private int _health = 10;

    // движение 
    private float _inputH;
    private float _inputV;
    private float _acceleration;
    private Vector3 _fall;

    //гравитация и прыжок 
    private float _timeFly = 0;
    private float _coyoteTime = 0.2f;

    private float _gravity;
    private float _ySpeed;
    private readonly float _jumpHeightGrounded = 1f;
    private readonly float _currentGravityMultiplier = 5;
    private readonly float _acelerationJump = 1.05f;
    private readonly float _maxFallSpeed = -10f;

    // поворот 
    private float _xEulerHeadAngle;
    private readonly float _minHeadAngle = -45;
    private readonly float _maxHeadAngle = 65;

    // приседание 
    private bool _isCurrentSquat = false;

    private Coroutine _coroutine;

    public void Init(float speed)
    {
        _speed = speed;
    }

    private void Start()
    {
        _gravity = Physics.gravity.y * _currentGravityMultiplier;
        Transform camera = Camera.main.transform;
        camera.parent = _pointCamera;
        camera.localPosition = Vector3.zero;
        camera.localRotation = Quaternion.identity;
    }
    void Update()
    {
        if (_characterController.enabled == false) return;
        CalculationTimeCoyote();
        CalculationGravity();
        Move();
        ChangeAnimation();
    }


    private void CalculationTimeCoyote()
    {
        if (_characterController.isGrounded == false)
        {
            _timeFly += Time.deltaTime;
        }
        else if (_timeFly > 0) _timeFly = 0;
    }


    public void Respawn(Vector3 spawnPosition)
    {

        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(SetPlayerNewPosition(spawnPosition));
    }

    private IEnumerator SetPlayerNewPosition(Vector3 spawnPosition)
    {
        _characterController.enabled = false;
        transform.position = spawnPosition;
        yield return new WaitForSeconds (0.3f);
        _characterController.enabled = true;
        _coroutine = null;
    }


    private void CalculationGravity()
    {
        _ySpeed += _gravity * Time.deltaTime;
        _ySpeed = Mathf.Clamp(_ySpeed, _maxFallSpeed, float.MaxValue);
        _fall.y = _ySpeed;
    }
    private void Move()
    {
        Vector3 movePlayer = new Vector3(_inputH, 0, _inputV).normalized * _speed * _acceleration;
        movePlayer = transform.TransformVector(movePlayer);
        _characterController.Move((movePlayer + _fall) * Time.deltaTime);
    }
    private void ChangeAnimation()
    {
        _animatorFoot.SetBool("Grounded", _characterController.isGrounded);
        _animatorFoot.SetFloat("Speed", _characterController.velocity.magnitude / _speed);
    }


    public void SetInput(float h, float v, float acceleration)
    {
        _inputH = h;
        _inputV = v;
        _acceleration = acceleration;
    }
    public void Jump()
    {
        if (_timeFly > _coyoteTime) return;
        _ySpeed = Mathf.Sqrt(_jumpHeightGrounded * _acelerationJump * -4 * _gravity);
    }
    public void Squat(bool isSquat)
    {
        if (_isCurrentSquat == isSquat) return;
        _isCurrentSquat = isSquat;
        _animatorSquat.SetBool("IsSquat", _isCurrentSquat);
    }

 
    internal void RotateX(float value)
    {
        _xEulerHeadAngle += value;
        _xEulerHeadAngle = Mathf.Clamp(_xEulerHeadAngle, _minHeadAngle, _maxHeadAngle);
        _head.localEulerAngles = new Vector3(_xEulerHeadAngle, 0, 0);
    }
    public void RotateY(float value)
    {
        transform.Rotate(0, value, 0);
    }


    public void GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out bool isFly)
    {
        velocity = _characterController.velocity;
        position = transform.position;
        rotateX = _head.localEulerAngles.x;
        rotateY = transform.localEulerAngles.y;
        isFly = !_characterController.isGrounded;
    }
}
