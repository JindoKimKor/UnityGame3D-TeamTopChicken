using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;



    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCounter.Interact();");
    }    
    public virtual void InteractAlternate(Player player)
    {
       //Debug.LogError("BaseCounter.InteractAlternate();");
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
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
