using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternate;

    private PlayerInputActions _playerInputActions;


    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        // to check what Action trigger(button e) or event is  
        _playerInputActions.Player.Interact.performed += Interact_performed;
        _playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
    }

    private void InteractAlternate_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (OnInteractAlternate != null)
        {
            OnInteractAlternate(this, EventArgs.Empty);
        }
        //OnInteractAlternate?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (OnInteractAction != null)
        {
            OnInteractAction(this, EventArgs.Empty);
        }
        //OnInteractAction?.Invoke(this, EventArgs.Empty);

    }

    public Vector2 GetMovementVectorNormalized()
    {
        //Vector2 inputVector = new Vector2(0, 0);
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();

        //if (Input.GetKey(KeyCode.W))
        //{
        //    inputVector.y = +1;
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    inputVector.y = -1;
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    inputVector.x = -1;
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    inputVector.x = +1;
        //}

        inputVector = inputVector.normalized;
        //Debug.Log(inputVector);
        return inputVector;
    }
}
