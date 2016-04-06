using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class _logoScene : MonoBehaviour {

	public float fadeInTime = 1;
	public float showTime = 3;
	public float fadeOutTime = 3;

	public string levelToLoad = "";

	public float growRate = 0;

	Image blackoutImage;

	Color newColour;


	void Start()
	{
		blackoutImage = FindObjectOfType<Image>();
		blackoutImage.color = Color.black;
		newColour = blackoutImage.color;

		StartCoroutine(CommenceFadeIn());
	}

	void Update () 
	{
		Camera.main.orthographicSize -= growRate * Time.deltaTime;

		if(Input.anyKey)
		{
			LoadLevel();
		}
	}


	IEnumerator CommenceFadeIn()
	{
		while(blackoutImage.color.a >0)
		{
			newColour.a -= Time.deltaTime / fadeInTime;
			blackoutImage.color = newColour;
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(showTime);

		while(blackoutImage.color.a <1)
		{
			newColour.a += Time.deltaTime / fadeOutTime;
			blackoutImage.color = newColour;
			yield return new WaitForEndOfFrame();
		}

		LoadLevel();
	}

	public void LoadLevel()
	{
		StopAllCoroutines();
		SceneManager.LoadScene (levelToLoad);
	}
}
