using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IGameManager 
{   
    public ManagerStatus status { get; private set; }

    private List<string> _items;

    public void Startup() 
    {
        Debug.Log("Inventory manager starting... on frame" + Time.frameCount);

        _items = new List<string>();

        status = ManagerStatus.Started;
	}

    public void AddItem(string item) {
        if(_items==null) { _items = new List<string>(); }

        _items.Add(item);
        DisplayItems();
	}

    private void DisplayItems() {
        string itemDisplay = "Items: ";
        foreach(string item in _items) 
        {
            itemDisplay += item + " ";
		}
        Debug.Log(itemDisplay);
	}
}
