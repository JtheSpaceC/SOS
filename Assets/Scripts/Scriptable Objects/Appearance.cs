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

	/*public Sprite[] baseBody;
	public Sprite[] heads;
	public Color[] skinTones;
	public Sprite [] noses;
	public Sprite[] mouths;
	public Sprite[] speakingMouthShapes;
	public Sprite[] shoutingMouthShapes;
	public Sprite[] tenseMouthSahpes;
	public Sprite[] eyeWhitesMale;
	public Sprite[] eyeLidsMale;
	public Sprite[] eyeIrisesMale;
	public Sprite[] eyeBlinksMale;
	public Sprite[] eyeWhitesFemale;
	public Sprite[] eyeLidsFemale;
	public Sprite[] eyeIrisesFemale;
	public Sprite[] eyeBlinksFemale;
	public Sprite[] facialHair;
	public Sprite[] hairMale;
	public Sprite[] hairFemale;
	public Color[] hairColours;
	public Sprite[] eyesProp;
	public Sprite[] scars1;
	public Sprite[] scares2;
	public Sprite[] facialFeatures1; 
	public Sprite[] facialFeatures2; 
	public Sprite[] clothes;
	public Sprite[] helmets;
	public Sprite[] spaceSuits;
	public Color[] spaceSuitColours; 
	public Sprite[] unitNumbers;*/

	[HideInInspector] public int avatarWorldPositionModifier = 0; //used at start so avatars aren't on top of each other
}
