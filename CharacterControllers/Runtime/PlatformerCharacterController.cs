using System;
using UnityEngine;

public class PlatformerCharacterController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    private FrameInputs _inputs;

    private bool _facingLeft;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 4;
    [SerializeField] private float _acceleration = 2;
    [SerializeField] private float _currentMovementLerpSpeed = 100;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 15;
    [SerializeField] private float _fallMultiplier = 7;
    [SerializeField] private float _jumpVelocityFalloff = 8;
    [SerializeField] private float _wallJumpLock = 0.25f;
    [SerializeField] private float _wallJumpMovementLerp = 5;
    [SerializeField] private float _coyoteTime = 0.2f;
    [SerializeField] private bool _enableDoubleJump = true;

    [Header("Wall Interaction")]
    [SerializeField] private float _wallCheckOffset = 0.5f, _wallCheckRadius = 0.05f;
    private bool _isAgainstLeftWall, _isAgainstRightWall, _pushingLeftWall, _pushingRightWall;

    private bool _hasJumped;
    private bool _hasDoubleJumped;
    private bool _hasDashed;
    private bool _dashing;
    private bool _grabbing;
    private bool _wallSliding;

    private float _timeStartedDash;

    private float _timeLeftGrounded = -10;
    private float _timeLastWallJumped;
    private Vector3 _dashDir;

    public bool IsGrounded;

    private void Update()
    {
        GatherInputs();

        HandleGrounding();

        HandleWalking();

        HandleJumping();

        HandleWallSlide();

        HandleWallGrab();

        HandleDashing();
    }

    #region Inputs

    private void GatherInputs()
    {
        _inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
        _inputs.RawY = (int)Input.GetAxisRaw("Vertical");
        _inputs.X = Input.GetAxis("Horizontal");
        _inputs.Y = Input.GetAxis("Vertical");

        _facingLeft = _inputs.RawX != 1 && (_inputs.RawX == -1 || _facingLeft);
        if (!_grabbing) SetFacingDirection(_facingLeft); // Don't turn while grabbing the wall
    }

    private void SetFacingDirection(bool left)
    {
        transform.rotation = left ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
    }

    #endregion

    #region Detection

    [Header("Detection")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;

    private readonly Collider[] _ground = new Collider[1];
    private readonly Collider[] _leftWall = new Collider[1];
    private readonly Collider[] _rightWall = new Collider[1];

    private void HandleGrounding()
    {
        // Ground detection
        var grounded = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) > 0;

        if (!IsGrounded && grounded)
        {
            IsGrounded = true;
            _hasDashed = false;
            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            transform.SetParent(_ground[0].transform);
        }
        else if (IsGrounded && !grounded)
        {
            IsGrounded = false;
            _timeLeftGrounded = Time.time;
            transform.SetParent(null);
        }

        // Wall detection
        _isAgainstLeftWall = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(-_wallCheckOffset, 0), _wallCheckRadius, _leftWall, _groundMask) > 0;
        _isAgainstRightWall = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(_wallCheckOffset, 0), _wallCheckRadius, _rightWall, _groundMask) > 0;
        _pushingLeftWall = _isAgainstLeftWall && _inputs.X < 0;
        _pushingRightWall = _isAgainstRightWall && _inputs.X > 0;
    }

    #endregion

    #region Walking

    private void HandleWalking()
    {
        var acceleration = IsGrounded ? _acceleration : _acceleration * 0.5f;

        if (_dashing) return;
        // Walking movement
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (_rb.linearVelocity.x > 0) _inputs.X = 0; // Immediate stop and turn
            _inputs.X = Mathf.MoveTowards(_inputs.X, -1, acceleration * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (_rb.linearVelocity.x < 0) _inputs.X = 0;
            _inputs.X = Mathf.MoveTowards(_inputs.X, 1, acceleration * Time.deltaTime);
        }
        else
        {
            _inputs.X = Mathf.MoveTowards(_inputs.X, 0, acceleration * 2 * Time.deltaTime);
        }

        var idealVel = new Vector3(_inputs.X * _walkSpeed, _rb.linearVelocity.y);
        _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);
    }

    #endregion

    #region Jumping

    private void HandleJumping()
    {
        if (_dashing) return;
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_grabbing || !IsGrounded && (_isAgainstLeftWall || _isAgainstRightWall))
            {
                _timeLastWallJumped = Time.time;
                _currentMovementLerpSpeed = _wallJumpMovementLerp;
                ExecuteJump(new Vector2(_isAgainstLeftWall ? _jumpForce : -_jumpForce, _jumpForce)); // Wall jump
            }
            else if (IsGrounded || Time.time < _timeLeftGrounded + _coyoteTime || _enableDoubleJump && !_hasDoubleJumped)
            {
                if (!_hasJumped || _hasJumped && !_hasDoubleJumped) ExecuteJump(new Vector2(_rb.linearVelocity.x, _jumpForce), _hasJumped); // Ground jump
            }
        }

        void ExecuteJump(Vector3 dir, bool doubleJump = false)
        {
            _rb.linearVelocity = dir;
            _hasDoubleJumped = doubleJump;
            _hasJumped = true;
        }

        // Fall faster and allow small jumps
        if (_rb.linearVelocity.y < _jumpVelocityFalloff || _rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.C))
            _rb.linearVelocity += _fallMultiplier * Physics.gravity.y * Vector3.up * Time.deltaTime;
    }

    #endregion

    #region Wall Slide

    private void HandleWallSlide()
    {
        var sliding = _pushingLeftWall || _pushingRightWall;

        if (sliding && !_wallSliding)
        {
            transform.SetParent(_pushingLeftWall ? _leftWall[0].transform : _rightWall[0].transform);
            _wallSliding = true;
            if (_rb.linearVelocity.y < 0) _rb.linearVelocity = new Vector3(0, -1); // Slide down
        }
        else if (!sliding && _wallSliding && !_grabbing)
        {
            transform.SetParent(null);
            _wallSliding = false;
        }
    }

    #endregion

    #region Wall Grab

    private void HandleWallGrab()
    {
        var grabbing = (_isAgainstLeftWall || _isAgainstRightWall) && Input.GetKey(KeyCode.Z) && Time.time > _timeLastWallJumped + _wallJumpLock;

        _rb.useGravity = !_grabbing;
        if (grabbing && !_grabbing)
        {
            _grabbing = true;
            SetFacingDirection(_isAgainstLeftWall);
        }
        else if (!grabbing && _grabbing)
        {
            _grabbing = false;
        }

        if (_grabbing) _rb.linearVelocity = new Vector3(0, _inputs.RawY * 1 * (_inputs.RawY < 0 ? 1 : 0.8f));
    }

    #endregion

    #region Dash

    private void HandleDashing()
    {
        if (Input.GetKeyDown(KeyCode.X) && !_hasDashed)
        {
            _dashDir = new Vector3(_inputs.RawX, _inputs.RawY).normalized;
            if (_dashDir == Vector3.zero) _dashDir = _facingLeft ? Vector3.left : Vector3.right;
            _dashing = true;
            _hasDashed = true;
            _timeStartedDash = Time.time;
            _rb.useGravity = false;
        }

        if (_dashing)
        {
            _rb.linearVelocity = _dashDir * 15;

            if (Time.time >= _timeStartedDash + 1)
            {
                _dashing = false;
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, _rb.linearVelocity.y > 3 ? 3 : _rb.linearVelocity.y);
                _rb.useGravity = true;
            }
        }
    }

    #endregion

    #region Impacts

    [Header("Collisions")][SerializeField] private float _minImpactForce = 2;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > _minImpactForce && IsGrounded)
        {
            // Handle impact effects here (e.g. audio or visual feedback)
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death"))
        {
            Destroy(gameObject); // Kill the player
        }

        _hasDashed = false;
    }

    #endregion

    private struct FrameInputs
    {
        public float X, Y;
        public int RawX, RawY;
    }
}
