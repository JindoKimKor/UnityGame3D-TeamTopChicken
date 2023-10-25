using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f;

    private bool isWalking;
    private void Update()
    {
        Vector2 inputVector = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y = +1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x = +1;
        }

        inputVector = inputVector.normalized;

        //transform.position += inputVector;

        //transform.position += (Vector3)inputVector;

        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        transform.position += moveDirection * Time.deltaTime * _moveSpeed;

        isWalking = moveDirection != Vector3.zero;

        //transform.rotation
        //transform.eulerAngles
        //transform.LookAt
        //transform.up
        //transform.right useful for 2D Game
        //transform.forward = moveDirection;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);



        Debug.Log(Time.deltaTime);
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
