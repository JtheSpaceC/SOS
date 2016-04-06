using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuNemesis : MonoBehaviour {

	public static MenuNemesis instance;
	
	public GameObject newNemesisIconPrefab;
	public Transform listParentForNemesisIcon;
	public Color redColour;
	public Color greenColour;
	public Color amber;
	public Text descriptionText;
	public Sprite[] maleAvatars;
	public Sprite[] femaleAvatars;
	public GameObject[] nemesisScreenButtons;
	public Scrollbar myTextScrollbar;
	public Image textScrollBarHandle;
	
	[HideInInspector]public GameObject selectedNemesis;

	string startingText = "All known hostile commanders are listed to the left. Select one for our report or to post a bounty.";
	
	
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
		foreach (GameObject button in nemesisScreenButtons)
			button.SetActive (false);
	}


}//Mono
