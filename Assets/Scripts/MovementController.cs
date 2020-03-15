using UnityEngine;

namespace Mirror
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class MovementController : NetworkBehaviour
    {
        public const float ACCELERATION = 25f;

        // The base speed at which the given character can move
        [SyncVar]
        public float runSpeed = 5f;
        // The base speed for the character when walking. To disable walkSpeed, set it to runSpeed
        [SyncVar]
        public float walkSpeed = 2f;

        [SyncVar]
        public Vector3 gravity;

        private Animator characterAnimator;
        private CharacterController characterController;
        private new Camera camera;

        // Current movement the player is making.
        private Vector2 currentMovement = new Vector2();

        private Vector2 previousInput;
        private bool isWalking;
        public bool canMove;

        private void Start()
        {
            canMove = true;
            isWalking = false;
            gravity = new Vector3();

            characterController = GetComponent<CharacterController>();
            characterAnimator = GetComponent<Animator>();
            camera = Camera.main;
            if (isLocalPlayer)
            {
                camera.GetComponent<CameraFollow>().target = gameObject;
            }
        }

        void Update()
        {
            // Must be the local player, or they cannot move
            if (!isLocalPlayer)
            {
                return;
            }

            characterController.SimpleMove(gravity);

            if (Input.GetButtonDown("Toggle Run"))
            {
                isWalking = !isWalking;
            }
            

            float x = Mathf.Lerp(previousInput.x, Input.GetAxisRaw("Horizontal"), Time.deltaTime * .3f);
            float y = Mathf.Lerp(previousInput.y, Input.GetAxisRaw("Vertical"), Time.deltaTime * .3f);

            previousInput.x = Input.GetAxisRaw("Horizontal");
            previousInput.y = Input.GetAxisRaw("Vertical");

            // Smoothly transition to next intended movement
            Vector2 intendedMovement = new Vector2(x, y) * (isWalking ? walkSpeed : runSpeed);
            currentMovement = Vector2.MoveTowards(currentMovement, intendedMovement, Time.deltaTime * ACCELERATION);

            // Move the player
            if (currentMovement.magnitude > 0)
            {
                // Determine the absolute movement by aligning input to the camera's looking direction
                Vector3 absoluteMovement = currentMovement.y * Vector3.Cross(camera.transform.right, Vector3.up).normalized + currentMovement.x * Vector3.Cross(Vector3.up, camera.transform.forward).normalized;
                // Move (without gravity). Whenever we move we also readjust the player's direction to the direction they are running in.
                characterController.Move((absoluteMovement) * Time.deltaTime);

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(absoluteMovement), Time.deltaTime * 8);
            }
            // TODO: Might eventually want more animation options. E.g. when in 0-gravity and 'clambering' via a surface
            //characterAnimator.SetBool("Floating", !hasGravity); // Note: Player can be floating and still move
            characterAnimator.SetFloat("speed", currentMovement.magnitude / runSpeed); // animation Speed is a proportion of maximum runSpee
        }
    }
}