using UnityEngine;

[System.Serializable]
public class Appearance : ScriptableObject {

	public GameObject avatarPrefab;

	public Sprite[] baseBody;
	public Color[] skinTones;
	public Sprite [] noses;
	public Sprite[] mouths;
	public Sprite[] speakingMouthShapes;
	public Sprite[] shoutingMouthShapes;
	public Sprite[] tenseMouthSahpes;
	public Sprite[] eyesMale;
	public Sprite[] eyesFemale;
	public Sprite[] eyesBlinking;
	public Sprite[] facialHair;
	public Sprite[] hairMale;
	public Sprite[] hairFemale;
	public Color[] hairColours;
	public Sprite[] eyesProp;
	public Sprite[] facialFeatures; 
	public Sprite[] clothes;
	public Sprite[] helmets;
	public Sprite[] spaceSuits;
	public Color[] spaceSuitColours; 
	public Sprite[] unitNumbers;

	[HideInInspector] public int avatarWorldPositionModifier = 0; //used at start so avatars aren't on top of each other
}
