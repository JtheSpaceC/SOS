using UnityEngine;

[System.Serializable]
public class Environment : ScriptableObject {

	public GameObject smokeTrailPrefab;
	public GameObject flamesTrailPrefab;

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
