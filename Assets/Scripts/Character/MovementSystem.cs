/*Created by Pawe³ Mularczyk*/

using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementSystem : MonoBehaviour
{
    [SerializeField] private bool _debug = false;

    [Space]
    [Header("MOVE SETTINGS")]
    [SerializeField] private float _moveSpeed = 12f;
    [SerializeField] private float _moveInertia = 8f;
    [SerializeField] private float _maxMoveSpeedBonus = 1.5f;
    [SerializeField] private float _oneTimeBonusAddition = 0.05f;
    [SerializeField] private float _bonusSpeedOffTime = 1.5f;
    [Space]
    [SerializeField] private float _dashForce = 5f;
    [Space]
    [SerializeField] private float _maxJumpForce = 15f;
    [SerializeField] private float _jumpLoadSpeed = 15f;
    [Space]
    [SerializeField] private float _wallJumpLoadSpeed = 18f;
    [SerializeField] private float _wallSlideSpeed = 2f;
    [SerializeField] private float _wallSlideTime = 2f;
    [SerializeField] private int _maxWallJumps = 3;
    [Space]
    [SerializeField] private float _launcherForce = 10f;

    [Space]
    [Header("GROUND CHECK SETTINGS")]
    [SerializeField] private bool _isGrounded = false;
    [SerializeField] private Vector2 _groundCheckerSize = new Vector2(1, 0.1f);
    [SerializeField] private Vector2 _groundCheckerPosition;
    [SerializeField] private LayerMask _groundMask;

    [Space]
    [Header("WALL CHECK SETTINGS")]
    [SerializeField] private bool _onWall = false;
    [SerializeField] private Vector2 _wallCheckerSize = new Vector2(0.1f, 1);
    [SerializeField] private Vector2 _wallCheckerPosition;

    [Space]
    [Header("LAUNCHER CHECK SETTINGS")]
    [SerializeField] private bool _onLauncher = false;
    [SerializeField] private Vector2 _launcherCheckerSize = new Vector2(1.2f, 1.2f);
    [SerializeField] private Vector2 _launcherCheckerPosition;
    [SerializeField] private LayerMask _launcherMask;

    [Space]
    [Header("OTHER SETTINGS")]
    [SerializeField] private float _gravityScale = 6f;
    [SerializeField] private float _maxFallingSpeed = 50f;
    [SerializeField] private ParticleSystem _runParticle;
    [SerializeField] private ParticleSystem _jumpParticle;

    [Space]
    [Header("AUDIO SETTINGS")]
    [SerializeField] private SoundType _soundType;
    [Space]
    [SerializeField] private AudioClip _stepsSound;
    [SerializeField] private Vector2 _stepPitchRange;
    [SerializeField] private float _stepSoundFrequency;
    [Space]
    [SerializeField] private AudioClip _jumpSound;
    [SerializeField] Vector2 _jumpPitchRange;
    [Space]
    [SerializeField] private AudioClip _dashSound;
    [SerializeField] private Vector2 _dashPitchRange;
    [Space]
    [SerializeField] private AudioClip _landingSound;
    [SerializeField] private Vector2 _landingPitchRange;

    private InputMaster _inputMaster;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;

    private Rigidbody2D _playerRB;

    private Animator _characterAnimator;

    private AudioSystem _audioSystem;

    private float _moveInput;
    private float _currentVel;
    private float _jumpForce;
    private float _wallSlideTimer;
    private float _moveSpeedBonus = 1f;
    private float _bonusSpeedTimer;
    private float _stepFrequencyTimer;

    private bool _facingRight = true;
    private bool _jumpInput;
    private bool _dashInput;
    private bool _wallDetached = false;
    private bool _isDashing = false;
    private bool _firedFromLauncher = false;
    private bool _isJumping = false;
    private bool _playJumpSound = false;
    private bool _dashSoundIsPlayed = false;
    private bool _landingSoundIsPlayed = false;

    private int _wallJumps;

    public bool _pausedGame { private get; set; }

    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    private void Start()
    {
        _playerRB = GetComponent<Rigidbody2D>();
        _characterAnimator = GetComponent<Animator>();
        _audioSystem = GetComponent<AudioSystem>();
    }

    private void OnEnable()
    {
        _moveAction = _inputMaster.Movement.Move;
        _jumpAction = _inputMaster.Movement.Jump;
        _dashAction = _inputMaster.Movement.Dash;
        _moveAction.Enable();
        _jumpAction.Enable();
        _dashAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _dashAction.Disable();
    }

    private void Update()
    {
        if (!_pausedGame)
            InputHandle();
        
        GroundChecker();
        WallChecker();
        LauncherChecker();
        GravityControl();
        AnimControl();
        AudioControl();

        if (_isGrounded)
        {
            _wallSlideTimer = 0;
            _wallDetached = false;
            _wallJumps = 0;
            _runParticle.Play();
        }
        else
        {
            _runParticle.Pause();
        }
        if (!_onWall)
        {
            _wallSlideTimer = 0;

            if (!_isGrounded)
                _jumpForce = 0;
        }
    }

    private void AudioControl()
    {
        _stepFrequencyTimer += Time.deltaTime;

        if (_moveInput != 0 && _stepFrequencyTimer > _stepSoundFrequency && _isGrounded)
        {
            _audioSystem.PlayAudio(_stepsSound, _soundType, _stepPitchRange);
            _stepFrequencyTimer = 0;
        }

        if (_playJumpSound)
        {
            _audioSystem.PlayAudio(_jumpSound, _soundType, _jumpPitchRange);
            _playJumpSound = false;
        }

        if(_isGrounded && _characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Landing") && !_landingSoundIsPlayed)
        {
            _audioSystem.PlayAudio(_landingSound, _soundType, _landingPitchRange);
            _landingSoundIsPlayed = true;
        }
        if (!_characterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Landing") && _landingSoundIsPlayed)
            _landingSoundIsPlayed = false;

        if(_dashInput && !_dashSoundIsPlayed && !_isDashing)
        {
            _audioSystem.PlayAudio(_dashSound, _soundType, _dashPitchRange);
            _dashSoundIsPlayed = true;
        }
        if (!_dashInput)
            _dashSoundIsPlayed = false;
    }

    private void AnimControl()
    {
        if (_moveInput != 0)
            _characterAnimator.SetBool("_run", true);
        else
            _characterAnimator.SetBool("_run", false);

        if (!_isGrounded)
            _characterAnimator.SetBool("_jump", true);
        else
            _characterAnimator.SetBool("_jump", false);

        if (_onWall)
            _characterAnimator.SetBool("_wallSlide", true);
        else
            _characterAnimator.SetBool("_wallSlide", false);
    }

    private void GravityControl()
    {
        _playerRB.gravityScale = _gravityScale;

        if (_playerRB.velocity.y < -_maxFallingSpeed)
            _playerRB.velocity = new Vector2(_playerRB.velocity.x, -_maxFallingSpeed);
    }

    private void FixedUpdate()
    {
        Move();
        Dash();
        Jump();
        WallJump();
        LauncherJump();
    }
    private void WallChecker()
    {
        float correctedCheckerPosX;
        if (transform.localScale.x < 0)
            correctedCheckerPosX = _wallCheckerPosition.x * -1f;
        else
            correctedCheckerPosX = _wallCheckerPosition.x;

        Vector2 checkerPos = new Vector2(transform.position.x + (transform.localScale.x - transform.localScale.x) + correctedCheckerPosX, transform.position.y + _wallCheckerPosition.y);
        _onWall = Physics2D.OverlapBox(checkerPos, _wallCheckerSize, 0, _groundMask);
    }


    private void GroundChecker()
    {
        Vector2 checkerPos = new Vector2(transform.position.x + _groundCheckerPosition.x, transform.position.y - (transform.localScale.y - transform.localScale.y) + _groundCheckerPosition.y);
        _isGrounded = Physics2D.OverlapBox(checkerPos, _groundCheckerSize, 0, _groundMask);
    }

    private void LauncherChecker()
    {
        Vector2 checkerPos = new Vector2(transform.position.x + _launcherCheckerPosition.x + (transform.localScale.x - transform.localScale.x), transform.position.y + _launcherCheckerPosition.y);
        _onLauncher = Physics2D.OverlapBox(checkerPos, _launcherCheckerSize, 0, _launcherMask);
    }

    private void Move()
    {
        Vector2 moveVel;

        if (!_onWall)
        {
            _currentVel = Mathf.Lerp(_currentVel, _moveInput * (_moveSpeed * SpeedBonus()), _moveInertia * Time.fixedDeltaTime);
            moveVel = new Vector2(_currentVel, _playerRB.velocity.y);
        }
        else
        {
            moveVel = WallSlide();
        }

        _playerRB.velocity = moveVel;

        if (_facingRight == false && _moveInput > 0)
            Flip();
        else if (_facingRight == true && _moveInput < 0)
            Flip();
    }

    private void Dash()
    {

        if (_dashInput && _moveInput != 0 && !_isDashing)
        {
            float dashVel = _dashForce * _moveInput * SpeedBonus();
            _currentVel += dashVel;
            _isDashing = true;
        }

        if (!_dashInput)
            if(_isGrounded || _onWall)
                _isDashing = false; 
    }

    private float SpeedBonus()
    {
        _bonusSpeedTimer += Time.fixedDeltaTime;

        if(_bonusSpeedTimer >= _bonusSpeedOffTime)
        {
            _moveSpeedBonus = Mathf.Lerp(_moveSpeedBonus, 1, 1.5f * Time.fixedDeltaTime);
        }

        return _moveSpeedBonus;
    }

    private void AddSpeedBonus()
    {
        _moveSpeedBonus = (_moveSpeedBonus + _oneTimeBonusAddition) > _maxMoveSpeedBonus ? _maxMoveSpeedBonus : (_moveSpeedBonus + _oneTimeBonusAddition);
        _bonusSpeedTimer = 0;
    }

    private void Jump()
    {
        if (!_isGrounded)
            return;  

        if (_jumpInput)
        {
            _isJumping = true;

            _jumpForce = Mathf.Lerp(_jumpForce, _maxJumpForce, _jumpLoadSpeed * Time.fixedDeltaTime);

            if (_jumpForce >= _maxJumpForce - 0.05f)
            {
                _playerRB.AddForce(Vector2.up * (_jumpForce * SpeedBonus()), ForceMode2D.Impulse);
                _jumpParticle.Play();
                _playJumpSound = true;
                _isJumping = false;
                _jumpForce = 0;
            }

        }
        else if (!_jumpInput && _isJumping)
        {
            _playerRB.AddForce(Vector2.up * (_jumpForce * SpeedBonus()), ForceMode2D.Impulse);
            _jumpParticle.Play();
            _playJumpSound = true;
            _jumpForce = 0;
            _isJumping = false;
        }
    }

    private void WallJump()
    {
        if (!_onWall || _wallJumps == _maxWallJumps || _isGrounded)
            return;

        if (_jumpInput)
        {
            _jumpForce = Mathf.Lerp(_jumpForce, _maxJumpForce, _wallJumpLoadSpeed * Time.fixedDeltaTime);
        }
        else if (!_jumpInput && _jumpForce != 0)
        {
            if (_facingRight)
                _currentVel = -12f;
            else
                _currentVel = 12f;

            _playerRB.AddForce(Vector2.up * 1.25f * (_jumpForce * SpeedBonus()), ForceMode2D.Impulse);
            _wallJumps++;
            AddSpeedBonus();
            _playJumpSound = true;
            Flip();
            _jumpForce = 0;
        }
    }

    private void LauncherJump()
    {
        if (!_onLauncher)
        {
            _firedFromLauncher = false;
            return;
        }

        if (_jumpInput && !_firedFromLauncher)
        {
            AddSpeedBonus();

            Vector2 launcherVel;

            if (_facingRight)
            {
                launcherVel = (Vector2.right + Vector2.up) * _launcherForce;
                _currentVel += _launcherForce;
            }
            else
            {
                launcherVel = (Vector2.left + Vector2.up) * _launcherForce;
                _currentVel -= _launcherForce;
            }

            _playerRB.AddForce(launcherVel, ForceMode2D.Impulse);

            _playJumpSound = true;
            _firedFromLauncher = true;
        }
    }

    private void Flip()
    {
        _facingRight = !_facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private Vector2 WallSlide()
    {
        if(_wallDetached)
        {
            _currentVel = 0;
            return new Vector2(0, _playerRB.velocity.y);
        }

        _wallSlideTimer += Time.fixedDeltaTime;

        if(_wallSlideTimer >= _wallSlideTime)
        {
            if (!_facingRight)
                _currentVel = 8f;
            else
                _currentVel = -8f;

            _wallDetached = true;

            return new Vector2(_currentVel, _playerRB.velocity.y);
        }
        else
        {
            _currentVel = 0;
            return new Vector2(0, -_wallSlideSpeed);
        }
    }

    private void InputHandle()
    {
        if(!_onWall || _isGrounded)
            _moveInput = _moveAction.ReadValue<float>();

        _jumpInput = _jumpAction.ReadValue<float>() > 0.1f ? true : false;
        _dashInput = _dashAction.ReadValue<float>() > 0.1f ? true : false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_debug)
        {
            //Ground Checker
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector2(transform.position.x + _groundCheckerPosition.x, transform.position.y - (transform.localScale.y - transform.localScale.y) + _groundCheckerPosition.y), _groundCheckerSize);

            //Wall Checker
            float correctedCheckerPosX;
            if (transform.localScale.x < 0)
                correctedCheckerPosX = _wallCheckerPosition.x * -1f;
            else
                correctedCheckerPosX = _wallCheckerPosition.x;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector2(transform.position.x + (transform.localScale.x - transform.localScale.x) + correctedCheckerPosX, transform.position.y + _wallCheckerPosition.y), _wallCheckerSize);

            //Launcher Checker
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector2(transform.position.x + _launcherCheckerPosition.x + (transform.localScale.x - transform.localScale.x), transform.position.y + _launcherCheckerPosition.y), _launcherCheckerSize);

            //Jump Force
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + (_jumpForce / 4)));

            //Wall Slide Timer
            if (_onWall)
            {
                UnityEditor.Handles.color = Color.black;
                UnityEditor.Handles.Label(new Vector2(transform.position.x, transform.position.y + transform.localScale.y), _wallSlideTimer.ToString());
            }
        }   
    }
#endif
}
