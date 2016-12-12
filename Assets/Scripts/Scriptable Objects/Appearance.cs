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
	[Tooltip("Go Broad, Medium, Narrow head shapes")]
	public Vector3[] earPositions;
	public Sprite[] speakingMouthShapes;
	public Sprite[] shoutingMouthShapes;
	public Sprite[] tenseMouthSahpes;
	public Sprite[] eyes; //needs setup
	public Color[] eyeColours;
	[HideInInspector] public Sprite[] facialHairToUse;
	public Sprite[] facialHair_broad;
	public Sprite[] facialHair_medium;
	public Sprite[] facialHair_narrow;
	[HideInInspector] public Sprite[] hairToUse;
	public Sprite[] hair_broad;
	public Sprite[] hair_medium;
	public Sprite[] hair_narrow;
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
