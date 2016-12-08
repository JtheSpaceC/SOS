using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class ButtonSelectAuto : MonoBehaviour {

	EventSystem myEventSystem;

	public Button myButton;
	[Tooltip("Should work better with this on, but delays the visual selection.")] public bool waitOneFrame = true;

	[Tooltip("Choose a key to activate this button.")] 
	public KeyCode selectButtonWith;
	public KeyCode alternateSelectButton;

	/*[Tooltip("Only select if using Gamepad?")]
	public bool dependsOnInputType = false;*/


	void Awake()
	{
		myEventSystem = FindObjectOfType<EventSystem>();
	}

	void OnEnable()
	{
		myEventSystem.SetSelectedGameObject(null);

		if(!waitOneFrame)
			myButton.Select();
		else
			StartCoroutine("SelectAfterOneFrame");
	}

	IEnumerator SelectAfterOneFrame()
	{
		yield return new WaitForEndOfFrame();

		if(myButton != null)
			myButton.Select();
	}

	void Update()
	{
		if(Input.GetKeyDown(selectButtonWith) || Input.GetKeyDown(alternateSelectButton))
		{
			DoTheClick();
		}
	}

	public void DoTheClick()
	{
		if(myButton == null)
			GetComponent<Button>().onClick.Invoke();
		else
			myButton.onClick.Invoke();
	}
}
