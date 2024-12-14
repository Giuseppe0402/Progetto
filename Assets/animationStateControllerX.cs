using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateControllerX : MonoBehaviour
{
    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    public float rotationSpeed = 10f; // Velocità di rotazione graduale
    public float walkSpeed = 5f; // Velocità di camminata
    public float runSpeed = 10f; // Velocità di corsa

    bool shiftPressedBeforeMovement = false; // Flag per determinare se Shift è stato premuto prima del movimento

    void Start()
    {
        animator = GetComponent<Animator>();

        //increases performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
    }

    void Update()
    {
        // Input del giocatore
        bool forwardPressed = Input.GetKey("w");
        bool backPressed = Input.GetKey("s");
        bool moveLeftPressed = Input.GetKey("a");
        bool moveRightPressed = Input.GetKey("d");
        bool runPressed = Input.GetKey("left shift");
        bool jumpPressed = Input.GetKey("space");

        // Se Shift è premuto senza direzione di movimento (WASD), non fare nulla
        if (runPressed && !forwardPressed && !backPressed && !moveLeftPressed && !moveRightPressed)
        {
            shiftPressedBeforeMovement = true; // Impedisce movimento se Shift è premuto senza WASD
        }
        else if (forwardPressed || backPressed || moveLeftPressed || moveRightPressed)
        {
            // Se un tasto WASD è premuto, resetta il flag
            if (shiftPressedBeforeMovement)
            {
                shiftPressedBeforeMovement = false; // Reset del flag
            }
        }

        // Determina la velocità del movimento
        float currentSpeed = (runPressed && !shiftPressedBeforeMovement) ? runSpeed : walkSpeed;

        // Calcola la direzione del movimento
        Vector3 moveDirection = Vector3.zero;

        if (forwardPressed) moveDirection += Vector3.forward;
        if (backPressed) moveDirection += Vector3.back;
        if (moveLeftPressed) moveDirection += Vector3.left;
        if (moveRightPressed) moveDirection += Vector3.right;

        // Se c'è una direzione di movimento e Shift non è stato premuto prima
        if (moveDirection != Vector3.zero && !shiftPressedBeforeMovement)
        {
            animator.SetBool(isWalkingHash, !runPressed); // Cammina se non stai correndo
            animator.SetBool(isRunningHash, runPressed); // Corri se stai correndo

            // Ruota il personaggio nella direzione del movimento
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Muove il personaggio nella direzione corrente
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
        }

        // Logica per il salto
        if (jumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
        }
        else
        {
            animator.SetBool(isJumpingHash, false);
        }
    }
}
