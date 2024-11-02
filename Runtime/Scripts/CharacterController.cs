using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace charactercontroller
{
    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Walk speed of the character")]
        [SerializeField]
        private float _walkSpeed = 5f;

        [Tooltip("Running speed of the character")]
        [SerializeField]
        private float _runSpeed = 10f;

        [Tooltip("Rotation speed to the character")]
        [SerializeField]
        private float _rotationSpeed;

        [Tooltip("This will be used for rotation")]
        [SerializeField]
        private Transform _characterModelTransform;

        [Header("Jump Settings")]
        [Tooltip("Jump force applied when jumping")]
        [SerializeField]
        private float _jumpForce = 5f;

        [Tooltip("Distance to check if the character is grounded")]
        [SerializeField]
        private float _groundCheckDistance = 0.1f;

        [Tooltip("Layer for ground detection")]
        [SerializeField]
        private LayerMask _groundLayer;

        // Components
        private Rigidbody _rigidbody;
        private PlayerInput _playerInput;
        private Transform _cameraTransform;
        private Animator _animator;

        private bool _isGrounded;
        private bool _isRunning;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInput = GetComponent<PlayerInput>();
            _animator = GetComponent<Animator>();
            _cameraTransform = Camera.main.transform;
        }

        void Start()
        {
            if (!_characterModelTransform)
            {
                throw new MissingFieldException("characterModelTransform is required!");
            }
            Cursor.lockState = CursorLockMode.Locked;
        }

        void FixedUpdate()
        {
            Vector2 move = _playerInput.actions["Move"].ReadValue<Vector2>();

            GroundCheck();
            Animate(move);

            if (move == Vector2.zero)
            {
                // Stop movement when no input is detected
                _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
                // If we won't return here then the camera keeps on rotating which will move the character too.
                return;
            }
            Vector3 movement = new Vector3(move.x, 0f, move.y);

            // For rotating the character to the camera facing side when moving forward.
            movement =
                movement.x * _cameraTransform.right.normalized
                + movement.z * _cameraTransform.forward.normalized;
            movement *= _isRunning ? _runSpeed : _walkSpeed;
            movement.y = _rigidbody.velocity.y;

            _rigidbody.velocity = movement;

            // Rotate the character to the forward moving direction.
            if (movement != Vector3.zero)
            {
                movement.y = 0f;
                Quaternion rotation = Quaternion.LookRotation(movement);
                _characterModelTransform.rotation = Quaternion.Lerp(
                    _characterModelTransform.rotation,
                    rotation,
                    _rotationSpeed * Time.fixedDeltaTime
                );
            }
        }

        // Method that will be called by the unity input system
        public void OnJump()
        {
            if (_isGrounded)
            {
                Debug.Log("Jump!");
                _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            }
        }

        public void OnRun(InputValue inputValue)
        {
            _isRunning = inputValue.isPressed;
        }

        // Utility methods
        private void GroundCheck()
        {
            _isGrounded = Physics.Raycast(
                transform.position,
                Vector3.down,
                _groundCheckDistance,
                _groundLayer
            );
        }

        private void Animate(Vector2 movement)
        {
            // Animation speed shouldn't be inlfluenced by the actual movement speed, So having separate separate multiplier for
            // animation.
            float walkAnimSpeed = 2f;
            float runAnimSpeed = 5f;
            _animator.SetFloat(
                "x",
                _isRunning ? runAnimSpeed * movement.x : walkAnimSpeed * movement.x
            );
            _animator.SetFloat(
                "y",
                _isRunning ? runAnimSpeed * movement.y : walkAnimSpeed * movement.y
            );
        }
    }
}
