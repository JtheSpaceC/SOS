using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RequisitionableItem : MonoBehaviour {

	public string itemName;
	[TextArea(10,20)]
	public string itemDescription;
	public int itemCost;
	public int deliveryTime = 3;

	public Text nameText;


	void Awake()
	{
		nameText.text = itemName;
	}


	public void SelectItem()
	{
		foreach(GameObject button in MenuRequisitions.instance.purchaseButtons)
		{
			button.SetActive(true);
		}
		MenuRequisitions.instance.selectedItem = this.gameObject;
		MenuRequisitions.instance.myTextScrollbar.value = 1;
		GetItemDescription ();
	}


	void GetItemDescription()
	{
		MenuRequisitions.instance.descriptionText.text = itemDescription;
		MenuRequisitions.instance.descriptionText.text += "\n\n" + "Delivery Time: " + deliveryTime + " days\n" + "Cost: c" + itemCost;
		MenuRequisitions.instance.priceText.text = "c" + itemCost;
	}

}
