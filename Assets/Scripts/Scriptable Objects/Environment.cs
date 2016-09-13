using UnityEngine;

[System.Serializable]
public class Environment : ScriptableObject {

	public Sprite[] asteroidsSmall;
	public Sprite[] asteroidsMedium;
	public Sprite[] asteroidsLarge;

	public Color[] BackgroundColours;


	public Sprite GetRandomSprite(Sprite[] array)
	{
		return array[Random.Range(0, array.Length)];
	}

	public Color GetASceneColour()
	{
		return BackgroundColours[Random.Range(0, BackgroundColours.Length)];
	}
}
