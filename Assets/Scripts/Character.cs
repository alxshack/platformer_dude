using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

enum MoveState
{
    Idle, Run, Jump, RearJump // @todo пересмотреть варианты состояний
}

enum CollisionState
{
    Grounded, LeftWalled, RightWalled, NoCollision
}

public class Character : MonoBehaviour
{

    [SerializeField] private float speed = 4.0f;
    [SerializeField] private float jumpForce = 3.0f;
    [SerializeField] private float wallSlideDragValue = 15.0f;
    [SerializeField] private float rearJumpForce = 3.0f;
    [SerializeField] private float rearJumpHoldTime = 0.5f;

    private Rigidbody2D _rigidbody;
    
    private CollisionState _collisionState;
    private MoveState _moveState;
    private bool _isDebugMode = true;
    private float _overlapCircleRadius = 0.05f;
    private float _defaultDragValue;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _defaultDragValue = _rigidbody.drag;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        CheckCollisionState();
        ModifyGravityScale();
    }

    private void CheckCollisionState()
    {
        // @todo слишком в лоб. нужен рефакторинг
        Vector3 position = transform.position;
        Collider2D[] groundColliders = Physics2D.OverlapCircleAll(position, _overlapCircleRadius);
        Collider2D[] rightWallColliders = Physics2D.OverlapCircleAll(position + Vector3.up * 0.5f + Vector3.right * 0.5f, 0.05f);
        Collider2D[] leftWallColliders = Physics2D.OverlapCircleAll(position + Vector3.up * 0.5f - Vector3.right * 0.5f, _overlapCircleRadius);

        if (groundColliders.Length > 1)
        {
            _collisionState = CollisionState.Grounded;
        }
        else if (rightWallColliders.Length > 1)
        {
            _collisionState = CollisionState.RightWalled;
        }
        else if (leftWallColliders.Length > 1)
        {
            _collisionState = CollisionState.LeftWalled;
        }
        else
        {
            _collisionState = CollisionState.NoCollision;
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (Input.GetButton("Horizontal") &&
                ((_collisionState != CollisionState.LeftWalled && Input.GetAxis("Horizontal") < 0) ||
                 (_collisionState != CollisionState.RightWalled && Input.GetAxis("Horizontal") > 0)))
            {
                Run();
            }

            if (Input.GetButtonDown("Jump") && (_collisionState != CollisionState.NoCollision))
            {
                Jump();
            }
            
            if (Input.GetButton("Fire1"))
            {
                Fight();
            }

            if (_isDebugMode)
            {
                DebugState();
            }
            
        }
        catch (Exception exception)
        {
            print(exception.Message);
        }
    }

    private void ModifyGravityScale()
    {
        if (_collisionState == CollisionState.LeftWalled || _collisionState == CollisionState.RightWalled)
        {
            _rigidbody.drag = wallSlideDragValue;
        }
        else
        {
            _rigidbody.drag = _defaultDragValue;
        }
    }

    private void Fight()
    {
        
    }

    private void Jump()
    {
        Vector2 localJumpForce;
        if (_collisionState == CollisionState.LeftWalled || _collisionState == CollisionState.RightWalled)
        {
            float localDirection = _collisionState == CollisionState.LeftWalled ? 1.0f : -1.0f;
            localJumpForce = transform.up * jumpForce + transform.right * rearJumpForce * localDirection;
            _moveState = MoveState.RearJump;
            Invoke(nameof(StateToLinearJump), rearJumpHoldTime);
        }
        else
        {
            localJumpForce = transform.up * jumpForce;
        }
        _rigidbody.AddForce(localJumpForce, ForceMode2D.Impulse);
    }
    
    private void StateToLinearJump()
    {
        _moveState = MoveState.Jump;
    }

    private void Run()
    {
        if (_moveState == MoveState.RearJump)
        {
            return;
        }
        if (_moveState == MoveState.Jump)
        {
            _rigidbody.velocity = Vector2.zero;
        }
        Vector3 direction = transform.right * Input.GetAxis("Horizontal");
        var position = transform.position;
        position = Vector3.MoveTowards(position, position + direction, speed * Time.deltaTime);
        transform.position = position;
        _moveState = MoveState.Run;
        
        print($"direction = {direction}");
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f + Vector3.right * 0.5f, 0.05f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f - Vector3.right * 0.5f, 0.05f);
    }

    private void DebugState()
    {
        print($"_collisionState = {_collisionState}");
        print($"_moveState = {_moveState}");
    }
    
}
