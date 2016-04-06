using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuRequisitions : MonoBehaviour {

	public static MenuRequisitions instance;

	public Text descriptionText;
	public Text priceText;
	public Scrollbar myTextScrollbar;

	public GameObject[] purchaseButtons;
	[HideInInspector]public GameObject selectedItem;

	string startingText = "Available items are listed on the left. Select them to view the cost and time to delivery.";


	void Awake()
	{
		if (instance == null) 
		{
			instance = this;
		}
		else 
		{
			Debug.LogError("Duplicate instances " + gameObject.name);
			Destroy (gameObject);
		}
	}


	void OnEnable()
	{
		descriptionText.text = startingText;
		foreach (GameObject button in purchaseButtons)
			button.SetActive (false);
	}

}//Mono
