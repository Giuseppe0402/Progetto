using UnityEngine;
using Unity.Netcode;

public class animationStateController : NetworkBehaviour
{
    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;
    private int isJumpingHash;

    [SerializeField] private float rotationSpeed = 5f; // Velocità di rotazione graduale
    [SerializeField] private float walkSpeed = 0f; // Velocità di camminata
    [SerializeField] private float runSpeed = 0f; // Velocità di corsa

    private bool shiftPressedBeforeMovement = false; // Flag per determinare se Shift è stato premuto prima del movimento

    private NetworkVariable<Vector3> serverPosition = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> serverRotation = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> isWalking = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> isJumping = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

    void Start()
    {
        animator = GetComponent<Animator>();

        //increases performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        isWalking.OnValueChanged += (oldValue, newValue) => animator.SetBool(isWalkingHash, newValue);
        isRunning.OnValueChanged += (oldValue, newValue) => animator.SetBool(isRunningHash, newValue);
        isJumping.OnValueChanged += (oldValue, newValue) => animator.SetBool(isJumpingHash, newValue);
    }

    void Update()
    {
        if (IsOwner)
        {
            HandleInput();
            UpdateServerPositionAndRotation();
        }
        else
        {
            // Synchronize position and rotation for non-owner clients
            transform.position = serverPosition.Value;
            transform.rotation = serverRotation.Value;
        }
    }

    private void HandleInput()
    {
        // Input del giocatore
        bool forwardPressed = Input.GetKey("w");
        bool backPressed = Input.GetKey("s");
        bool moveLeftPressed = Input.GetKey("a");
        bool moveRightPressed = Input.GetKey("d");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");

        // Evita movimento se Shift è premuto senza direzione
        if (runPressed && !forwardPressed && !backPressed && !moveLeftPressed && !moveRightPressed)
        {
            shiftPressedBeforeMovement = true;
        }
        else if (forwardPressed || backPressed || moveLeftPressed || moveRightPressed)
        {
            shiftPressedBeforeMovement = false;
        }

        // Determina la velocità del movimento
        float currentSpeed = (runPressed && !shiftPressedBeforeMovement) ? runSpeed : walkSpeed;

        // Calcola la direzione del movimento
        Vector3 moveDirection = Vector3.zero;

        if (forwardPressed) moveDirection += Vector3.forward;
        if (backPressed) moveDirection += Vector3.back;
        if (moveLeftPressed) moveDirection += Vector3.left;
        if (moveRightPressed) moveDirection += Vector3.right;

        // Movimento e rotazione
        if (moveDirection != Vector3.zero && !shiftPressedBeforeMovement)
        {
            isWalking.Value = !runPressed;
            isRunning.Value = runPressed;

            // Ruota il personaggio nella direzione del movimento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Muove il personaggio nella direzione corrente
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }
        else
        {
            isWalking.Value = false;
            isRunning.Value = false;
        }

        // Logica per il salto
        if (jumpPressed)
        {
            isJumping.Value = true;
        }
        else
        {
            isJumping.Value = false;
        }
    }

    private void UpdateServerPositionAndRotation()
    {
        // Aggiorna la posizione e la rotazione sul server
        serverPosition.Value = transform.position;
        serverRotation.Value = transform.rotation;
    }
}