using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Personnel : MonoBehaviour {

	public string personName;
	public enum Gender {male, female};
	public Gender gender;
	public string appearanceSeed;

	[Header("UI stuff")]
	public Text nameText;
	public Image faceImage;
	public Text statusText;


	/*protected IEnumerator CheckScrollbarSize(Scrollbar givenScrollbar, Image scrollBarHandle)
	{
		//TODO: This not working. Always thinks it's 1 as size doesn't seem to update until next frame. This works on the planet info box though!
		yield return new WaitForEndOfFrame ();
		if (givenScrollbar.size == 1) 
		{
			givenScrollbar.gameObject.SetActive (false);
		}
		else
		{
			givenScrollbar.gameObject.GetComponent<Image> ().enabled = true;
			scrollBarHandle.enabled = true;
		}
	}*/
}
