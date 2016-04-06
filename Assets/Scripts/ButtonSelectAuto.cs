using UnityEngine;
using UnityEngine.UI;

public class ButtonSelectAuto : MonoBehaviour {

	public Button myButton;

	void OnEnable()
	{
		myButton.Select();
	}
}
