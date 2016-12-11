using UnityEngine;

[System.Serializable]
public class Appearance : ScriptableObject {

	public GameObject avatarPrefab;

	public Sprite[] body;
	public Sprite[] heads;
	public Color[] skinTones;
	public Sprite[] noses;
	public Sprite[] mouths;
	public Sprite[] cheeks; //needs setup
	public Sprite[] chins; //needs setup
	public Sprite[] ears; //needs setup
	public Sprite[] speakingMouthShapes;
	public Sprite[] shoutingMouthShapes;
	public Sprite[] tenseMouthSahpes;
	public Sprite[] eyes; //needs setup
	public Color[] eyeColours;
	public Sprite[] facialHair;
	public Sprite[] hair;
	public Color[] hairColours;
	public Sprite[] eyesProp;
	public Sprite[] scars1; //needs setup
	public Sprite[] scars2; //needs setup
	public Sprite[] facialFeatures1; //needs setup
	public Sprite[] facialFeatures2; //needs setup
	public Sprite[] eyebrows; //needs setup
	public Sprite[] clothes;
	public Sprite[] helmets;
	public Sprite[] spaceSuits;
	public Color[] spaceSuitColours1; 
	public Color[] spaceSuitColours2; 
	public Sprite[] unitNumbers;

	[HideInInspector] public static int avatarWorldPositionModifier = 0; //used at start so avatars aren't on top of each other
}
