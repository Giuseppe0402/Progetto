using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateControllerX : MonoBehaviour
{

    Animator animator;
    int isWalkingHash;
    int isRunningHash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        //increases performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

    }

    // Update is called once per frame
    void Update()
    {
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);
        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");

        //se il giocatore preme w
        if (!isWalking && forwardPressed)
        {
            //allora setta il boolean isWalkign come vero
            animator.SetBool(isWalkingHash, true);
        }

        //se il giocatore non preme w
        if (isWalking && !forwardPressed)
        {
            //allora setta il boolean isWalking come falso
            animator.SetBool(isWalkingHash, false);
        }

        //se il giocatore sta camminando e non sta correndo e preme lo shift sinistro
        if (!isRunning && (forwardPressed && runPressed))
        {
            //allora setta il boolean isRunning come vero
            animator.SetBool(isRunningHash, true);

        }

        //se il giocatore sta correndo e si ferma o smette di camminare
        if (isRunning && (!forwardPressed || !runPressed))
        {
            //allora setta il booleano isRunning come falso
            animator.SetBool(isRunningHash, false);
        }

    }
}