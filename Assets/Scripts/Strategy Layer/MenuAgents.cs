using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuAgents : MonoBehaviour {

	public static MenuAgents instance;

	public GameObject newAgentIconPrefab;
	public Transform listParentForAgentIcon;
	public Color redColour;
	public Color greenColour;
	public Color amber;
	public Text descriptionText;
	public Sprite[] maleAvatars;
	public Sprite[] femaleAvatars;
	public string[] names;
	public GameObject[] agentScreenButtons;	
	public Scrollbar myTextScrollbar;
	public Image textScrollBarHandle;

	[HideInInspector]public GameObject selectedAgent;

	Spy newSpy;
	string startingText = "All of our agents are listed to the left. Select one to see their information or to give them orders.";


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

	void Start () 
	{
		//TODO: Remove these
		CreateAgent ();
		CreateAgent ();
		CreateAgent ();
		CreateAgent ();
	}

	void OnEnable()
	{
		descriptionText.text = startingText;
		foreach (GameObject button in agentScreenButtons)
			button.SetActive (false);
	}

	void CreateAgent()
	{
		GameObject newAgent = Instantiate (newAgentIconPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		newAgent.transform.SetParent (listParentForAgentIcon);
		newSpy = newAgent.GetComponent<Spy> ();
		newSpy.gender = Random.Range (0, 2) >= 1 ? Spy.Gender.male : Spy.Gender.female;
		newSpy.faceImage.sprite = newSpy.gender == Spy.Gender.male? maleAvatars[Random.Range(0, maleAvatars.Length)] : 
			femaleAvatars[Random.Range(0, femaleAvatars.Length)];
		newSpy.personName = "Cmdr. "+ names [Random.Range (0, names.Length)];
		newSpy.nameText.text = newSpy.personName;
		newSpy.lastReport = Random.Range (20, RTSDirector.instance.gameDay);
	}


}//Mono
