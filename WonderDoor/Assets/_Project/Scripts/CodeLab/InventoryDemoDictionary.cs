using UnityEngine;
using System.Collections.Generic;

public class InventoryDemoDictionary : MonoBehaviour 
{
    [SerializeField] private string itemName;
    [SerializeField] private int itemCountInput;
    private Dictionary<string, int> inventory;

    private void Start()
    {
        UnityEngine.Debug.Log("InventoryDemoDictionary started");
        inventory = new Dictionary<string, int>(System.StringComparer.InvariantCultureIgnoreCase);
        AddItem(itemName, itemCountInput);
        AddItem("КИРАСА", 10);
        AddItem("Щит", 3);
        AddItem("Меч", 1);
        OutputItem();
        UnityEngine.Debug.Log("InventoryDemoDictionary finished");
    }
    
    private void AddItem(string itemName,int itemCountInput)
    {
            string key = itemName;
            if(inventory.TryGetValue(itemName, out int currentCount)){
                inventory [itemName] = currentCount + itemCountInput;
            }
            else{
             inventory .Add(key, itemCountInput);
            }
        
    }
    private void OutputItem()
    {
        foreach (var item in inventory ){
            UnityEngine.Debug.Log(item.Key + " " + item.Value);
        }
    }
}