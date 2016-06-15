using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class ButtonSelectAuto : MonoBehaviour {

	EventSystem myEventSystem;

	public Button myButton;
	[Tooltip("Should work better with this on, but delays the visual selection.")] public bool waitOneFrame = true;


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

		myButton.Select();
	}
}
