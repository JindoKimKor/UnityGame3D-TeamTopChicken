using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class KitchenObjectSO :ScriptableObject
{
    // kind of prefab is selected 
    public Transform prefab;
    public Sprite sprite;
    public string objectName;
   
}
