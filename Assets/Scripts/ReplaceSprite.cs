using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//USED TO SEARCH THROUGH HIERARCHY AND MASS CHANGE SPRITES OR THEIR COLOURS

[ExecuteInEditMode]
public class ReplaceSprite : MonoBehaviour {

	Image[] images;

	public List<Sprite> imagesFound = new List<Sprite>();

	public string replaceThisSprite;
	public Sprite withThisSprite;

	public List<Image> imagesRecentlyReplaced;


	[ContextMenu("Get List Of Items")]
	public void GetListOfItems()
	{
		images = GetComponentsInChildren<Image>(true);
		print("Found " + images.Length + " iamges.");
		imagesFound.Clear();

		foreach(Image im in images)
		{
			if(!imagesFound.Contains(im.sprite))
			{
				print("Adding " + im.sprite);
				imagesFound.Add(im.sprite);

				if(im.sprite == null)
					print(im.name + " was null");
			}
		}
	}

	[ContextMenu("Do Replacement")]
	public void DoReplacement()
	{
		if(replaceThisSprite == null || replaceThisSprite == "")
		{
			Debug.Log("replaceThisSprite string was empty");
			return;
		}
		else if(withThisSprite == null)
		{
			Debug.Log("withThisSprite was null");
			return;
		}
		else
		{
			imagesRecentlyReplaced.Clear();

			Image[] allImages = GetComponentsInChildren<Image>(true);
			foreach(Image image in allImages)
			{
				if(image.sprite.name == replaceThisSprite)
				{
					Debug.Log("Replacing " + image.sprite.name + " on " + image.name);
					image.sprite = withThisSprite;
					imagesRecentlyReplaced.Add(image);
				}
			}
		}
	}

	[ContextMenu("Do Additional Treatment")]
	public void DoAdditionalTreatment()
	{
		Debug.LogWarning("Have you definitely written the custom behaviour here?");

		foreach(Image image in imagesRecentlyReplaced)
		{
			//WRITE CUSTOM CHANGES IN HERE
			image.color = Color.white;
		}
	}
}
