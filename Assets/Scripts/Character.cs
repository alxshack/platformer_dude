using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private int maxHealthCost = 100;

    public HealthBar healthBar;
    public Text moneyText;
    
    private Rigidbody2D _rigidbody;
    
    private int _moneyCost, _manaCost, _healthCost;
    
    private CollisionState _collisionState;
    private MoveState _moveState;
    private float _overlapCircleRadius = 0.05f;
    private float _defaultDragValue;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _defaultDragValue = _rigidbody.drag;
        _moneyCost = 0;
        _healthCost = maxHealthCost;
        healthBar.SetMaxHealth(maxHealthCost);
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
        else if (rightWallColliders.Any(elem => elem.tag == "SlidedWall"))
        {
            _collisionState = CollisionState.RightWalled;
        }
        else if (leftWallColliders.Any(elem => elem.tag == "SlidedWall"))
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

            if(Input.GetKeyDown(KeyCode.D))
            {
                TakeDamage(10);
            }

            BetterJumpModifier();
        }
        catch (Exception exception)
        {
            print(exception.Message);
        }
    }

    private void BetterJumpModifier()
    {
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime; // @todo код добавлен из урока, почему тут -1, хотя можно задать меньшее значение поля?
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
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string collisionTag = collision.gameObject.tag;
        GameObject colObj = collision.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Collider2D obTrigger = trigger;
        
        switch (obTrigger.tag)
        {
            case "Money":
                int costValue = obTrigger.GetComponent<Money>().CostValue;
                AddMoney(costValue);
                break;
            case "Mana":
                int manaValue = obTrigger.GetComponent<Mana>().CostValue;
                AddMana(manaValue);
                break;
            case "Health":
                int healthValue = obTrigger.GetComponent<Health>().CostValue;
                AddHealth(healthValue);
                break;
            default:
                break;
        }
        
        if (obTrigger.GetComponent<Catchable>())
        {
            Destroy(obTrigger.gameObject);
        }
        
    }

    private void AddMoney(int cost)
    {
        _moneyCost += cost;
        moneyText.text = _moneyCost.ToString();
        print($"moneyCost = {_moneyCost}");
    }
    
    private void AddMana(int cost)
    {
        _manaCost += cost;
        print($"manaCost = {_manaCost}");
    }
    
    private void AddHealth(int cost)
    {
        _healthCost += cost;
        print($"healthCost = {_manaCost}");
    }
    
    private void TakeDamage(int damage)
    {
        _healthCost -= damage;
        healthBar.SetHealth(_healthCost);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f + Vector3.right * 0.5f, 0.05f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f - Vector3.right * 0.5f, 0.05f);
    }
    
}
