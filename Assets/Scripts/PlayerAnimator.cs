using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{

    private const string IS_WALKING = "IsWalking";

    [SerializeField] private Player player;

    private Animator animator;

    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Checks if the player is moving based on the IsWalking Player Method
        // If false then run player's Idle animation state
        // If true then run player's Walk animation state
        animator.SetBool(IS_WALKING, player.IsWalking());
    }
}
