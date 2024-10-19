using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace charactercontroller {

    [RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
    public class CharacterController : MonoBehaviour {

        [Header("Movement Settings")]
        [Tooltip("Movement speed of the character")]
        [SerializeField]
        private float _speed;

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

        private Rigidbody _rigidbody;
        private PlayerInput _playerInput;
        private Transform _cameraTransform;
        private bool _isGrounded;

        void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _playerInput = GetComponent<PlayerInput>();
            _cameraTransform = Camera.main.transform;
        }

        void Start() {
            if (!_characterModelTransform) {
                throw new MissingFieldException("characterModelTransform is required!");
            }
            Cursor.lockState = CursorLockMode.Locked;
        }

        void FixedUpdate() {
            GroundCheck();
            Vector2 move = _playerInput.actions["Move"].ReadValue<Vector2>();
            if (move == Vector2.zero) {
                // Stop movement when no input is detected
                _rigidbody.velocity = new Vector3(0f, _rigidbody.velocity.y, 0f);
                // If we won't return here then the camera keeps on rotating which will move the character too.
                return;
            }
            Vector3 movement = new Vector3(move.x, 0f, move.y);
            
            // For rotating the character to the camera facing side when moving forward.
            movement = movement.x * _cameraTransform.right.normalized + movement.z * _cameraTransform.forward.normalized;
            movement *= _speed;
            movement.y = _rigidbody.velocity.y;

            _rigidbody.velocity = movement;

            // Rotate the character to the forward moving direction.
            if (movement != Vector3.zero) {
                movement.y = 0f; 
                Quaternion rotation = Quaternion.LookRotation(movement);
                _characterModelTransform.rotation = Quaternion.Lerp(_characterModelTransform.rotation, rotation,
                        _rotationSpeed * Time.fixedDeltaTime);
            }
        }

        public void OnJump() {
            if (_isGrounded) {
                Debug.Log("Jump!");
                _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            }
        }

        private void GroundCheck() {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, _groundCheckDistance, _groundLayer);
        }
    }
}
