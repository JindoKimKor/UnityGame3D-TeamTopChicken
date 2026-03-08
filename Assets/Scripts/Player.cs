using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    //Singleton Pattern
    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private GameInput _gameInput;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;


    //To keep interact even stop moving
    private bool isWalking;
    private Vector3 lastInteractDirection;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private void Awake()
    {
        // To prevent multiple player
        if (Instance != null)
        {
            Debug.LogError("There is more than one player instance");
        }
        Instance = this;
    }

    private void Start()
    {
        _gameInput.OnInteractAction += GameInput_OnInteractAction;
        _gameInput.OnInteractAlternate += GameInput_InteractAlternate;
    }

    private void GameInput_InteractAlternate(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if(selectedCounter != null) {
           selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector2 inputVectorTwo = _gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVectorTwo.x, 0f, inputVectorTwo.y);
       
        if (moveDirection != Vector3.zero )
        {
            lastInteractDirection = moveDirection;
        }


        float interactDistance = 2f;

        Debug.Log($"{transform.position}, {lastInteractDirection}, {interactDistance}, {countersLayerMask}");
        // By using Raycast, detecting collider  and interaction with prefabs
        // Racast return nomarlly bool
        if (Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            Debug.Log(raycastHit.transform);
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                Debug.Log(baseCounter);
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);
            //Debug.Log("-");
        }

        //Debug.Log(selectedCounter);
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleMovement() {
        Vector2 inputVectorTwo = _gameInput.GetMovementVectorNormalized();

        //transform.position += inputVector;

        //transform.position += (Vector3)inputVector;

        Vector3 moveDirection = new Vector3(inputVectorTwo.x, 0f, inputVectorTwo.y);

        float moveDistance = _moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirection, moveDistance);

        if (!canMove)
        {
            // Cannot move towards moveDirection

            // Attemp only X movement
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = moveDirection.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionX, moveDistance);

            if (canMove)
            {
                // Can move only on the X
                moveDirection = moveDirectionX;
            }
            else
            {
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove = moveDirection.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirectionZ, moveDistance);
                if (canMove)
                {
                    // Can move only on the Z
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    // Cannot move in any direction
                }
            }

        }
        else if (canMove)
        {
            transform.position += moveDirection * moveDistance;

        }
        isWalking = moveDirection != Vector3.zero;

        //transform.rotation
        //transform.eulerAngles
        //transform.LookAt
        //transform.up
        //transform.right useful for 2D Game
        //transform.forward = moveDirection;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }
    // Modifying position
    // To set changing SO into changed table and previous table make empty 
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    // method for checking whether counter has no kitchenObject
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
