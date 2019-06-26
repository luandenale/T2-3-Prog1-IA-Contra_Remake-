﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weapon
{
    REGULAR,
    RAPID,
    MACHINEGUN,
    SPREAD,
    FIRE,
    LASER
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance = null;

    public Weapon CurrentWeapon;
    public Vector2 PlayerDirection;

    public bool PlayerJumped = false;
    public bool PlayerJumpingDown = false;

    public bool IsPlayerTouchingGround;
    public bool IsPlayerTouchingWater;
    public bool IsPlayerGettingOutOfWater;
    public bool IsPlayerShooting;
    public bool IsPlayerWalking { get; private set; }
    public bool IsAimingUp { get; private set; }
    public bool IsAimingDown { get; private set; }

    public float ShotSpeedModificator = 1f;

    public bool PlayerDied;

    private Rigidbody2D _playerRigidBody;
    private SpriteRenderer _playerSprite;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this);

        _playerRigidBody = GetComponent<Rigidbody2D>();
        PlayerDied = false;
    }

    private void Update()
    {
        if (_playerRigidBody.velocity.y != 0)
        {
            IsPlayerTouchingGround = false;
        }

        if (_playerRigidBody.velocity.x != 0)
            IsPlayerWalking = true;
        else
            IsPlayerWalking = false;
    }

    private void OnCollisionStay2D(Collision2D p_collision)
    {
        if (p_collision.gameObject.tag == "Ground")
        {
            IsPlayerTouchingWater = false;

            // Check if its firmly on the floor
            if(_playerRigidBody.velocity.y == 0)
            {
                if (!IsPlayerTouchingGround)
                    AudioManager.instance.PlayHitFloor();
                IsPlayerTouchingGround = true;
            }

            // Check to jump down through
            if (PlayerJumpingDown)
            {
                BoxCollider2D __platformCollider = p_collision.gameObject.GetComponent<BoxCollider2D>();

                PlayerJumpingDown = false;
                __platformCollider.isTrigger = true;
                _playerRigidBody.AddForce(new Vector3(0f, -1f, 0f), ForceMode2D.Impulse);
                StartCoroutine(DisableGroundPeriodically(__platformCollider));
            }
        }
        else if (p_collision.gameObject.tag == "Water")
        {
            if (!IsPlayerTouchingWater && !IsPlayerGettingOutOfWater)
                AudioManager.instance.PlayHitFloor();
            IsPlayerTouchingWater = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            PlayerDied = true;
            AudioManager.instance.PlayDie();
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        if (collision.gameObject.tag == "Ground" && IsPlayerTouchingWater)
        {
            IsPlayerGettingOutOfWater = true;
            StartCoroutine(GetOutOfWater());
        }
    }

    private IEnumerator GetOutOfWater()
    {
        _playerRigidBody.velocity = new Vector2(0f,0f);
        yield return new WaitForSeconds(0.1f);
        transform.position = new Vector3(transform.position.x + 0.25f, -4.16f, 0f);
        IsPlayerGettingOutOfWater = false;
    }

    private IEnumerator DisableGroundPeriodically(BoxCollider2D p_collider)
    {
        yield return new WaitForSeconds(0.5f);
        p_collider.isTrigger = false;
    }

    public void ResetPlayer()
    {
        GetComponent<Animator>().SetTrigger("Jump");

        float __xPosition = transform.position.x;

        if (__xPosition > Camera.main.transform.position.x)
            __xPosition = Camera.main.transform.position.x - 1f;

        transform.position = new Vector3(__xPosition, 2f, 0f);

        gameObject.layer = LayerMask.NameToLayer("Default");

        CurrentWeapon = Weapon.REGULAR;
        PlayerDied = false;
        PlayerDirection = new Vector2(1f, 0f);
        ShotSpeedModificator = 1f;
    }
}