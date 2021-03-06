﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimTriggers
{
    public const string Jump = "Jump";
    public const string DiveWater = "DiveWater";
    public const string GettingOutOfWater = "GettingOutOfWater";

    public const string OnGround = "OnGround";
    public const string OnWater = "OnWater";
    public const string IsShooting = "IsShooting";
    public const string IsWalking = "IsWalking";
    public const string IsAimingUp = "IsAimingUp";
    public const string IsAimingDown = "IsAimingDown";

}

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] Sprites;

    private Animator _playerAnim;

    private bool _coolingDown = false;
    private Coroutine _coolingDownStraightRoutineReference = null;
    private Coroutine _currentShootingRoutineReference = null;
    private bool _triggeredDeath = false;

    private void Awake()
    {
        _playerAnim = GetComponent<Animator>();
    }

    private void Start()
    {
        _playerAnim.SetTrigger("Jump");
    }

    private void Update()
    {
        if (!PlayerManager.instance.PlayerDied)
        {
            _triggeredDeath = false;

            FlipSprites();

            if (PlayerManager.instance.IsPlayerGettingOutOfWater)
                _playerAnim.SetTrigger(AnimTriggers.GettingOutOfWater);
            // Player is touching the ground
            if (PlayerManager.instance.IsPlayerTouchingGround)
            {
                OnTheGroundAnimations();
            }
            // Player is on the water
            else if (PlayerManager.instance.IsPlayerTouchingWater)
            {
                if(!_playerAnim.GetBool(AnimTriggers.OnWater))
                    _playerAnim.SetTrigger(AnimTriggers.DiveWater);
                SetActiveSprite(0);
                OnTheWaterAnimations();
            }
            // Player on the air
            else
            {
                SetActiveSprite(0);
                OnTheAirAnimation();
            }

            // Shooting Animation
            if (PlayerManager.instance.IsPlayerShooting)
            {
                _playerAnim.SetBool(AnimTriggers.IsShooting, true);
                PlayerManager.instance.IsPlayerShooting = false;
            }
            else
                _playerAnim.SetBool(AnimTriggers.IsShooting, false);
        }
        else
        {
            if (!_triggeredDeath)
            {
                SetActiveSprite(0);
                _triggeredDeath = true;
                _playerAnim.SetTrigger("Dead");
            }
        }
    }

    private void FlipSprites()
    {
        // Set Sprites Dir
        if (PlayerManager.instance.PlayerDirection.x > 0)
            for (int i = 0; i < Sprites.Length; i++)
                Sprites[i].flipX = false;
        else if (PlayerManager.instance.PlayerDirection.x < 0)
            for (int i = 0; i < Sprites.Length; i++)
                    Sprites[i].flipX = true;
    }

    private void OnTheWaterAnimations()
    {
        _playerAnim.SetBool(AnimTriggers.OnWater, true);
        _playerAnim.SetBool(AnimTriggers.OnGround, false);

        // Set aim up and down triggers
        if (PlayerManager.instance.PlayerDirection.y > 0)
            _playerAnim.SetBool(AnimTriggers.IsAimingUp, true);
        else
            _playerAnim.SetBool(AnimTriggers.IsAimingUp, false);

        // Set walking anim
        if (PlayerManager.instance.IsPlayerWalking)
            _playerAnim.SetBool(AnimTriggers.IsWalking, true);
        else
            _playerAnim.SetBool(AnimTriggers.IsWalking, false);
    }

    private void OnTheGroundAnimations()
    {
        _playerAnim.SetBool(AnimTriggers.OnGround, true);
        _playerAnim.SetBool(AnimTriggers.OnWater, false);

        // Set aim up and down triggers
        if (PlayerManager.instance.PlayerDirection.y > 0 && !PlayerManager.instance.IsPlayerWalking)
            _playerAnim.SetBool(AnimTriggers.IsAimingUp, true);
        else if (PlayerManager.instance.PlayerDirection.y < 0 && !PlayerManager.instance.IsPlayerWalking)
            _playerAnim.SetBool(AnimTriggers.IsAimingDown, true);
        else
        {
            _playerAnim.SetBool(AnimTriggers.IsAimingUp, false);
            _playerAnim.SetBool(AnimTriggers.IsAimingDown, false);
        }
        // Set walking anim
        if (PlayerManager.instance.IsPlayerWalking)
        {
            SwitchRunningSprites();
            _playerAnim.SetBool(AnimTriggers.IsWalking, true);
        }
        else
        {
            SetActiveSprite(0);
            _playerAnim.SetBool(AnimTriggers.IsWalking, false);
        }
    }

    private void OnTheAirAnimation()
    {
        _playerAnim.SetBool(AnimTriggers.OnGround, false);
        if (PlayerManager.instance.PlayerJumped)
        {
            PlayerManager.instance.PlayerJumped = false;
            _playerAnim.SetTrigger(AnimTriggers.Jump);
        }
    }

    private void SwitchRunningSprites()
    {
        // Cancel current shooting routine before triggering another
        if (_currentShootingRoutineReference != null)
            StopCoroutine(_currentShootingRoutineReference);

        // Player is aiming up
        if (PlayerManager.instance.PlayerDirection.y > 0)
            if (PlayerManager.instance.IsPlayerShooting)
                StartCoroutine(SetShootingSprite(4, 3));
            else
                SetActiveSprite(3);
        // Player is aiming down
        else if(PlayerManager.instance.PlayerDirection.y < 0)
            if (PlayerManager.instance.IsPlayerShooting)
                StartCoroutine(SetShootingSprite(6, 5));
            else
                SetActiveSprite(5);
        // Running straight ahead
        else
            if (PlayerManager.instance.IsPlayerShooting)
            {
                _coolingDown = true;

                // Cancel cooling down routine before triggering another
                if (_coolingDownStraightRoutineReference!=null)
                    StopCoroutine(_coolingDownStraightRoutineReference);
                _coolingDownStraightRoutineReference = StartCoroutine(CoolingDownStraightRoutine());

                StartCoroutine(SetShootingSprite(2,1));
            }
            else if(!_coolingDown)
                SetActiveSprite(0);
            else if(_coolingDown)
                SetActiveSprite(1);
    }

    private void SetActiveSprite(int p_index)
    {
        Sprites[p_index].enabled = true;
        // Disable all other sprites
        for (int i = 0; i < Sprites.Length; i++)
            if (i != p_index)
                Sprites[i].enabled = false;
    }

    private IEnumerator CoolingDownStraightRoutine()
    {
        SetActiveSprite(1);

        yield return new WaitForSeconds(1.5f);
         _coolingDown = false;
    }

    private IEnumerator SetShootingSprite(int p_indexShootingSprite, int p_indexRegulargSprite)
    {
        SetActiveSprite(p_indexShootingSprite);

        yield return new WaitForSeconds(0.6f);

        if (Sprites[p_indexShootingSprite].enabled)
            SetActiveSprite(p_indexRegulargSprite);
    }
}
