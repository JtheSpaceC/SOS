//NOTE: There are two ways that facial features are picked in here.
//GenerateRandomNewAppearance() and  GenerateAppearanceBySeed(string[] seed)
//It is also changed in CharacterPool with the Next option in the Character Creation screen.
//Any change to any sprites may require changes in 3 places.
//The seed is also referenced in 4 places with "APPEARANCE_SEED"


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Character : MonoBehaviour {

	int nextSeedChoice;

	public bool showShipInsteadOfAvatar = false;

	[HideInInspector] public Heartbeat heartbeatScript;
	[HideInInspector] public BGScroller bgScrollerScript;
	[HideInInspector] public AIFighter myAIFighterScript;

	[Tooltip("Usually okay to leave blank if on a Fighter. Gets set by SquadronLeader script normally.")] 
	public GameObject avatarOutput;

	[Tooltip("For Character Pool screen")] public bool selected = false;

	public enum Gender {Male, Female};
	public Gender gender;

	public string characterID;
	public string firstName;
	public string lastName;
	public string callsign;
	public string characterBio;
	public string appearanceSeed;

	public bool inSpace = true;

	[HideInInspector] public Appearance myAppearance;
	public Appearance maleAppearances;
	public Appearance femaleAppearances;
	public Names names;
	public GameObject avatarOutputPrefab;
	RenderTexture myRenderTexture;

	[Header("Appearance")]
	public SpriteRenderer body;
	public SpriteRenderer clothes;
	public SpriteRenderer[] ears;
	public SpriteRenderer head;
	public SpriteRenderer chin;
	public SpriteRenderer eyeLids;
	public Image eyeWhites;
	public Image eyeIrises;
	public SpriteRenderer eyeShine;
	public SpriteRenderer eyesBlinking;
	public Transform eyeballs;
	public SpriteRenderer cheeks;
	public SpriteRenderer mouth;
	[Range(0,100)]
	public int chanceOfFacialFeatures = 25;
	public SpriteRenderer facialFeatures1;
	public SpriteRenderer facialFeatures2;
	public SpriteRenderer facialHair;
	public SpriteRenderer nose;
	[Range(0,100)]
	public int chanceOfScarring = 25;
	public SpriteRenderer scars1;
	public SpriteRenderer scars2;
	[Range(0,100)]
	public int chanceOfEyesProp = 25;
	public SpriteRenderer eyesProp;
	public SpriteRenderer eyebrows;
	public SpriteRenderer hair;
	public SpriteRenderer helmet;
	public SpriteRenderer spaceSuit;
	public SpriteRenderer cockpit;
	public SpriteRenderer warpBG;

	[Header("Eye Positions")]
	public Vector2 neutral;
	public Vector2 up;
	public Vector2 upperRight;
	public Vector2 right;
	public Vector2 lowerRight;
	public Vector2 down;
	public Vector2 lowerLeft;
	public Vector2 left;
	public Vector2 upperLeft;
	Vector2[] eyePositions;

	//for moving eyes around
	Vector2 startPos;
	float startTime = 0;
	float timeToMove = 0.1f;
	public Vector2 nextPosition;

	//for speaking & mouth behaviour
	Sprite originalMouth;
	//bool speaking = false;

	int callsignChecks;

	void Awake()
	{
		myRenderTexture = new RenderTexture(256, 256, 24);
		myRenderTexture.name = "Avatar RT "+ transform.root.gameObject.name;
		bgScrollerScript = GetComponentInChildren<BGScroller>();

		//Appearance.avatarWorldPositionModifier = 0;
	}

	void Start () 
	{
		Appearance.avatarWorldPositionModifier++;

		transform.position = new Vector3 (Appearance.avatarWorldPositionModifier * 5, 1000, -1000); //stops avatars lining up on each other

		transform.SetParent(null);

		heartbeatScript = FindObjectOfType<Heartbeat>();
		eyePositions = new Vector2[] {neutral, up, upperRight, right, lowerRight, down, lowerLeft, left, upperLeft};

		//if we're not loading in an appearance (if we are this happens but is overridden, currently)
		GenerateRandomNewAppearance();
		GenerateName();

		originalMouth = mouth.sprite;

		StartCoroutine("AlertEyes");
		StartCoroutine("Blinking");

		if(!inSpace)
		{
			helmet.enabled = false;
			spaceSuit.enabled = false;
			cockpit.enabled = false;
			bgScrollerScript.gameObject.SetActive(false);
		}
		else
		{
			hair.enabled = false;
			clothes.enabled = false;
			helmet.enabled = true;
			spaceSuit.enabled = true;
			cockpit.enabled = true;
			bgScrollerScript.gameObject.SetActive(true);
		}
	}

	public void SetUpAvatar (int mySquadUnitNumber)
	{
		if(mySquadUnitNumber > 3)
			return;
		GetComponentInChildren<Camera>().enabled = true;
		GetComponentInChildren<Camera>().targetTexture = myRenderTexture;
		avatarOutput = Instantiate (avatarOutputPrefab) as GameObject;
		avatarOutput.transform.SetParent (Tools.instance.NextFreeAvatarsPanelUI());
		avatarOutput.GetComponent<RawImage> ().texture = myRenderTexture;
		avatarOutput.transform.localScale = Vector3.one;
		avatarOutput.GetComponent<RectTransform>().offsetMax = Vector2.zero;
		avatarOutput.GetComponent<RectTransform>().offsetMin = Vector2.zero;		
		avatarOutput.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;	

		if(mySquadUnitNumber != 0)
		{
			avatarOutput.transform.FindChild("Flash Image/Unit Number").GetComponent<Image>().sprite = myAppearance.unitNumbers[mySquadUnitNumber];
			//TODO: Remove this line below when unit numbers are brought back. 
			avatarOutput.transform.FindChild("Flash Image/Unit Number").GetComponent<Image>().color = Color.clear;
		}

		if(showShipInsteadOfAvatar)
		{
			Transform myCam = GetComponentInChildren<Camera>().transform;
			myCam.parent = myAIFighterScript.transform;
			myCam.localPosition = new Vector3(0, 0, -10);
			myCam.gameObject.AddComponent<UI_NonRotate>();
			myCam.localRotation = Quaternion.Euler(Vector3.zero);
			myCam.GetComponent<Camera>().cullingMask = Tools.instance.normalCameraViewingLayers;
		}
	}

	[ContextMenu("Generate Random Appearance")]
	public void GenerateRandomNewAppearance()
	{
		GenerateRandomNewAppearance(999);
		GenerateName();
	}

	public void GenerateRandomNewAppearance(int genderInt) //where 0 is male, 1 is female, otherwise random
	{
		//APPEARANCE_SEED//SAVED HERE
		//FOR SEED (string): ORDER IS: 
		//OLD(Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour)

		//0-Gender, 1-Body, 2-Clothes, 3-Ears, 4-Head, 5-Chin, 6-Eyes, 7-Cheeks, 8-Mouth, 9-Facial 1, 10-Facial 2, 11-Scar 1, 12-Scar 2,
		//13-Facial Hair, 14-Nose,  15-Eye Prop, 16-Eyebrows, 17-Hair, 18-Space suit, 19-helmet,
		//20-Skin Colour, 21-Hair Colour, 22-Eye colour, 23-Spacesuit Colour 1, 24-Spacesuit Colour 2

		if(genderInt == 0)
		{
			gender = Gender.Male;
			myAppearance = maleAppearances;
			appearanceSeed = "0,";
		}
		else if(genderInt == 1)
		{
			gender = Gender.Female;
			myAppearance = femaleAppearances;
			appearanceSeed = "1,";
		}
		else //random choice of male or female
		{
			if(UnityEngine.Random.Range(0f, 2f) > -1) //TODO: re-enable females
			{
				gender = Gender.Male;
				myAppearance = maleAppearances;
				appearanceSeed = "0,";
			}
			else 
			{
				gender = Gender.Female;
				myAppearance = femaleAppearances;
				appearanceSeed = "1,";
			}
		}

		body.sprite = myAppearance.body[NewSeed(myAppearance.body.Length, false)];
		clothes.sprite = myAppearance.clothes[NewSeed(myAppearance.clothes.Length, false)];
		ears[0].sprite = myAppearance.ears[NewSeed(myAppearance.ears.Length, false)];
		ears[1].sprite = ears[0].sprite;

		int headShape = NewSeed(myAppearance.heads.Length, false);
		head.sprite = myAppearance.heads[headShape];
		//then ear positions have to change
		AdjustEarsAndHair(headShape);

		chin.sprite = myAppearance.chins[NewSeed(myAppearance.chins.Length, false)];

		//Generate Eyes
		int numSetsOfEyes = (myAppearance.eyes.Length /5)-1;
		int eyeSetChoice = NewSeed(numSetsOfEyes, false) * 5;
		eyeLids.sprite = myAppearance.eyes[eyeSetChoice];
		eyeWhites.sprite = myAppearance.eyes[eyeSetChoice+1];
		eyeIrises.sprite = myAppearance.eyes[eyeSetChoice+2];
		eyeShine.sprite = myAppearance.eyes[eyeSetChoice+3];
		eyesBlinking.sprite = myAppearance.eyes[eyeSetChoice+4];
		//eye iris colour is set lower down

		cheeks.sprite = myAppearance.cheeks[NewSeed(myAppearance.cheeks.Length, false)];
		mouth.sprite = myAppearance.mouths[NewSeed(myAppearance.mouths.Length, false)];

		//chance of having FACIAL FEATURE
		int result = UnityEngine.Random.Range(0, 4);

		if(result == 0) //just one facial feature
		{
			nextSeedChoice = NewSeed(myAppearance.facialFeatures1.Length, true);
			if(nextSeedChoice == 0)
				facialFeatures1.sprite = null;
			else
				facialFeatures1.sprite = myAppearance.facialFeatures1[nextSeedChoice-1];

			facialFeatures2.sprite = null;
			NewSeed(0, false); //for ff2

			AdjustFacialFeatureColour();
		}
		else if(result == 2) //two facial features
		{
			nextSeedChoice = NewSeed(myAppearance.facialFeatures1.Length, true);
			if(nextSeedChoice == 0)
				facialFeatures1.sprite = null;
			else
				facialFeatures1.sprite = myAppearance.facialFeatures1[nextSeedChoice-1];
			
			nextSeedChoice = NewSeed(myAppearance.facialFeatures2.Length, true);
			if(nextSeedChoice == 0)
				facialFeatures2.sprite = null;
			else
				facialFeatures2.sprite = myAppearance.facialFeatures2[nextSeedChoice-1];	

			AdjustFacialFeatureColour();
		}
		else
		{
			facialFeatures1.sprite = null;
			facialFeatures2.sprite = null;
			NewSeed(0, false); //for ff1
			NewSeed(0, false); //for ff2
		}

		//chance of having SCARS
		int result2 = UnityEngine.Random.Range(0, 3);

		if(result2 == 0) //just one scar
		{
			nextSeedChoice = NewSeed(myAppearance.scars1.Length, true);
			if(nextSeedChoice == 0)
				scars1.sprite = null;
			else
				scars1.sprite = myAppearance.scars1[nextSeedChoice-1];

			scars2.sprite = null;
			NewSeed(0, false); //for scars2

			AdjustFacialFeatureColour();
		}
		else
		{
			scars1.sprite = null;
			scars2.sprite = null;
			NewSeed(0, false); //for scars1
			NewSeed(0, false); //for scars2
		}

		//FACIAL HAIR
		if(gender == Gender.Male)
		{
			//50:50 chance to be a clean shaven male
			if(UnityEngine.Random.Range(0,10) >=5)
			{
				nextSeedChoice = NewSeed(myAppearance.facialHairToUse.Length, true);
				if(nextSeedChoice == 0)
					facialHair.sprite = null;
				else
					facialHair.sprite = myAppearance.facialHairToUse[nextSeedChoice-1]; 
			}
			else
			{
				facialHair.sprite = null;
				NewSeed(0, false);
			}
		}
		else 
		{
			facialHair.sprite = null;
			NewSeed(0, false);
		}

		nose.sprite = myAppearance.noses[NewSeed(myAppearance.noses.Length, false)];

		//chance of having EYE PROP
		if(UnityEngine.Random.Range(0,100) >= chanceOfEyesProp)
		{
			nextSeedChoice = NewSeed(myAppearance.eyesProp.Length, true);
			if(nextSeedChoice == 0)
				eyesProp.sprite = null;
			else
				eyesProp.sprite = myAppearance.eyesProp[nextSeedChoice-1];
		}
		else
		{
			eyesProp.sprite = null;
			NewSeed(0, false);
		}

		eyebrows.sprite = myAppearance.eyebrows[NewSeed(myAppearance.eyebrows.Length, false)];

		//HAIR
		nextSeedChoice = NewSeed(myAppearance.hairToUse.Length, true);
		if(nextSeedChoice == 0)
			hair.sprite = null;
		else
			hair.sprite = myAppearance.hairToUse[nextSeedChoice-1];

		spaceSuit.sprite = myAppearance.spaceSuits[NewSeed(myAppearance.spaceSuits.Length, false)];
		helmet.sprite = myAppearance.helmets[NewSeed(myAppearance.helmets.Length, false)];

		//SKIN COLOUR
		Color skinColor = myAppearance.skinTones[NewSeed(myAppearance.skinTones.Length, false)];
		head.color = skinColor;
		chin.color = skinColor;
		eyeLids.color = skinColor;
		eyesBlinking.color = skinColor;
		cheeks.color = skinColor;
		mouth.color = Color.Lerp(Color.white, skinColor, 0.5f);
		nose.color = skinColor;
		ears[0].color = skinColor;
		ears[1].color = skinColor;
		AdjustFacialFeatureColour();

		//HAIR COLOUR
		hair.color = myAppearance.hairColours[NewSeed(myAppearance.hairColours.Length, false)];
		facialHair.color = hair.color;
		eyebrows.color = hair.color;

		//EYE COLOUR
		eyeIrises.color = myAppearance.eyeColours[NewSeed(myAppearance.eyeColours.Length, false)];

		//SPACE SUIT COLOURS
		helmet.color = myAppearance.spaceSuitColours1[NewSeed(myAppearance.spaceSuitColours1.Length, false)];
		spaceSuit.color = myAppearance.spaceSuitColours2[NewSeed(myAppearance.spaceSuitColours1.Length, false)];

	}//end of GenerateRandomNewAppearance


	public void GenerateAppearanceBySeed(string[] seed)
	{
		//APPEARANCE_SEED
		//FOR SEED (string): ORDER IS: 
		//OLD(Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, SpacesuitColour)

		//0-Gender, 1-Body, 2-Clothes, 3-Ears, 4-Head, 5-Chin, 6-Eyes, 7-Cheeks, 8-Mouth, 9-Facial 1, 10-Facial 2, 11-Scar 1, 12-Scar 2,
		//13-Facial Hair, 14-Nose,  15-Eye Prop, 16-Eyebrows, 17-Hair, 18-Space suit, 19-helmet,
		//20-Skin Colour, 21-Hair Colour, 22-Eye colour, 23-Spacesuit Colour 1, 24-Spacesuit Colour 2

		if(seed[0] == "0") //male
		{
			gender = Gender.Male;
			myAppearance = maleAppearances;
		}
		else if(seed[0] == "1") //female
		{
			gender = Gender.Female;
			myAppearance = femaleAppearances;
		}
		else
			Debug.Log("Something Went Wrong");

		body.sprite = myAppearance.body[Int32.Parse(seed[1].ToString())];


		clothes.sprite = myAppearance.clothes[Int32.Parse(seed[2].ToString())];
		ears[0].sprite = myAppearance.ears[Int32.Parse(seed[3].ToString())];
		ears[1].sprite = ears[0].sprite;

		int headShape = Int32.Parse(seed[4].ToString());
		head.sprite = myAppearance.heads[headShape];
		//then ear positions have to change
		AdjustEarsAndHair(headShape);

		chin.sprite = myAppearance.chins[Int32.Parse(seed[5].ToString())];

		int eyeSetChoice = Int32.Parse(seed[6].ToString())* 5;
		eyeLids.sprite = myAppearance.eyes[eyeSetChoice];
		eyeWhites.sprite = myAppearance.eyes[eyeSetChoice+1];
		eyeIrises.sprite = myAppearance.eyes[eyeSetChoice+2];
		eyeShine.sprite = myAppearance.eyes[eyeSetChoice+3];
		eyesBlinking.sprite = myAppearance.eyes[eyeSetChoice+4];
		//eye iris colour is set lower down

		cheeks.sprite = myAppearance.cheeks[Int32.Parse(seed[7].ToString())];
		mouth.sprite = myAppearance.mouths[Int32.Parse(seed[8].ToString())];

		//FACIAL FEATURES
		int facialFeature = Int32.Parse(seed[9].ToString());
		if(facialFeature == 0)
			facialFeatures1.sprite = null;
		else
			facialFeatures1.sprite = myAppearance.facialFeatures1[facialFeature-1];
		
		facialFeature = Int32.Parse(seed[10].ToString());
		if(facialFeature == 0)
			facialFeatures2.sprite = null;
		else
			facialFeatures2.sprite = myAppearance.facialFeatures2[facialFeature-1];

		//SCARS
		facialFeature = Int32.Parse(seed[11].ToString());
		if(facialFeature == 0)
			scars1.sprite = null;
		else
			scars1.sprite = myAppearance.scars1[facialFeature-1];

		facialFeature = Int32.Parse(seed[12].ToString());
		if(facialFeature == 0)
			scars2.sprite = null;
		else
			scars2.sprite = myAppearance.scars2[facialFeature-1];

		//FACIAL HAIR
		if(gender == Gender.Male)
		{
			facialFeature = Int32.Parse(seed[13].ToString());
			if(facialFeature == 0)
				facialHair.sprite = null;
			else
				facialHair.sprite = myAppearance.facialHairToUse[facialFeature-1];
		}
		else 
		{
			facialHair.sprite = null; 
		}

		nose.sprite = myAppearance.noses[Int32.Parse(seed[14].ToString())];

		//EYE PROP
		facialFeature = Int32.Parse(seed[15].ToString());
		if(facialFeature == 0)
			eyesProp.sprite = null;
		else 
			eyesProp.sprite = myAppearance.eyesProp[facialFeature-1];

		eyebrows.sprite = myAppearance.eyebrows[Int32.Parse(seed[16].ToString())];

		//HAIR
		facialFeature = Int32.Parse(seed[17].ToString());
		if(facialFeature == 0)
			hair.sprite = null;
		else
			hair.sprite = myAppearance.hairToUse[facialFeature-1];

		spaceSuit.sprite = myAppearance.spaceSuits[Int32.Parse(seed[18].ToString())];
		helmet.sprite = myAppearance.helmets[Int32.Parse(seed[19].ToString())];

		//SKIN COLOUR
		Color newColour = myAppearance.skinTones[Int32.Parse(seed[20].ToString())];
		head.color = newColour;
		chin.color = newColour;
		eyeLids.color = newColour;
		eyesBlinking.color = newColour;
		cheeks.color = newColour;
		mouth.color = Color.Lerp(Color.white, newColour, 0.5f);
		nose.color = newColour;
		ears[0].color = newColour;
		ears[1].color = newColour;
		AdjustFacialFeatureColour();

		//HAIR COLOUR
		newColour = myAppearance.hairColours[Int32.Parse(seed[21].ToString())];
		hair.color = newColour;
		facialHair.color = newColour;
		eyebrows.color = newColour;

		eyeIrises.color = myAppearance.eyeColours[Int32.Parse(seed[22].ToString())];

		//SPACESUIT COLOUR 1 & 2
		newColour = myAppearance.spaceSuitColours1[Int32.Parse(seed[23].ToString())];
		spaceSuit.color = newColour;
		newColour = myAppearance.spaceSuitColours2[Int32.Parse(seed[24].ToString())];
		helmet.color = newColour;

	}//end of GenerateAppearanceBySeed()

	public void GenerateName()
	{
		//Get a first and last name. We may check this against names in use later, so we don't have the same names in a campaign
		GetFirstAndLastName();
		string fullName = firstName+lastName;

		//Do the same for callsigns
		callsignChecks = 0;
		GetCallsign ();

		//next we check for duplicated names, if appropriate
		if(Tools.instance)
		{
			while(Tools.instance.fullNamesInUse.Contains(fullName))
			{
				print("Contained full names. Getting new one.");
				GetFirstAndLastName();
			}

			Tools.instance.fullNamesInUse.Add(fullName);

			while(Tools.instance.callsignsInUse.Contains(callsign))
			{
				callsignChecks ++;
				print("Check " + callsignChecks + ". Pilot already exists with Callsign " + callsign);

				if(callsignChecks == Tools.instance.callsignsInUse.Count)
				{
					Debug.Log("Clearing out Callsigns list. May reuse.");
					Tools.instance.callsignsInUse.Clear(); //so we don't get stuck on an infinite loop
				}

				GetCallsign();	
			}
			Tools.instance.callsignsInUse.Add(callsign);
		}
		//OLD WAY
		//else callsign = NameGenerator.Instance.getRandomCallsign(); 
		//OR callsign = StaticTools.PopRandomElement(Tools.instance.availableCallsigns);

		if(myAIFighterScript != null)
		{
			myAIFighterScript.nameHUDText.text = callsign;
		}
	}

	void GetFirstAndLastName()
	{
		//firstName = NameGenerator.Instance.getRandomFirstName(gender.ToString().ToCharArray()[0]);
		//lastName = NameGenerator.Instance.getRandomLastName();

		if(gender == Gender.Male)	
		{
			firstName = names.maleNames[UnityEngine.Random.Range(0, names.maleNames.Count)];
		}
		else if(gender == Gender.Female)
		{
			firstName = names.femaleNames[UnityEngine.Random.Range(0, names.femaleNames.Count)];
		}

		lastName = names.lastNames[UnityEngine.Random.Range(0, names.lastNames.Count)];
	}

	void GetCallsign ()
	{
		List<string> possibleCallsigns = new List<string>();
		possibleCallsigns.AddRange(names.callsigns);

		if (gender == Gender.Male) 
		{
			possibleCallsigns.AddRange (names.callsignsMaleOnly);
		}
		else if (gender == Gender.Female) 
		{
			possibleCallsigns.AddRange (names.callsignsFemaleOnly);
		}
		callsign = possibleCallsigns [UnityEngine.Random.Range (0, possibleCallsigns.Count)];
	}

	int NewSeed(int arrayLength, bool zeroIsEmpty) //this function adds its result to the string recording the seed of this appearance
	{
		int choice = 0;
		if(zeroIsEmpty) //this is because a choice of zero is meant to be nothing on scars, facial features, etc, not the first selection
			choice = UnityEngine.Random.Range(0, arrayLength+1);
		else
			choice = UnityEngine.Random.Range(0, arrayLength);
		
		appearanceSeed += choice.ToString() + ",";
		return choice;
	}

	public Sprite GetARandomSprite(Sprite[] whatArray)
	{
		return whatArray[UnityEngine.Random.Range(0, whatArray.Length)];
	}

	public void AdjustFacialFeatureColour() //mostly for scar colour matching skin tone
	{
		facialFeatures1.color = Color.Lerp(body.color, Color.black, 0.4f);
		facialFeatures2.color = Color.Lerp(body.color, Color.black, 0.4f);
	}

	public void AdjustEarsAndHair(int shape)
	{
		//ear positions and hair types have to change
		if(shape == 0) //broad
		{
			ears[0].transform.localPosition = myAppearance.earPositions[0];
			ears[1].transform.localPosition = myAppearance.earPositions[0] * (-2);

			myAppearance.hairToUse = myAppearance.hair_broad;
			myAppearance.facialHairToUse = myAppearance.facialHair_broad;
		}
		else if(shape == 1) //medium
		{
			ears[0].transform.localPosition = myAppearance.earPositions[1];
			ears[1].transform.localPosition = myAppearance.earPositions[1] * (-2);

			myAppearance.hairToUse = myAppearance.hair_medium;
			myAppearance.facialHairToUse = myAppearance.facialHair_medium;
		}
		else if(shape == 2) //narrow
		{
			ears[0].transform.localPosition = myAppearance.earPositions[2];
			ears[1].transform.localPosition = myAppearance.earPositions[2] * (-2);

			myAppearance.hairToUse = myAppearance.hair_narrow;
			myAppearance.facialHairToUse = myAppearance.facialHair_narrow;
		}
		else 
			Debug.LogWarning("You've used more than 3 head types haven't you?...");
	}


	#region Animations

	public IEnumerator GuiltyEyes()
	{
		startPos = eyeballs.localPosition;
		startTime = Time.time;
		RandomNewEyesPosition();

		while(Time.time < startTime + timeToMove)
		{
			eyeballs.localPosition = Vector2.Lerp(startPos, nextPosition, (Time.time - startTime)/timeToMove);
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 2f));
		StartCoroutine("GuiltyEyes");
	}

	public IEnumerator AlertEyes()
	{
		startPos = eyeballs.localPosition;
		startTime = Time.time;

		if(nextPosition != neutral || UnityEngine.Random.Range(0f, 10f) > 2.5f) //if we were not previously in neutral, go there, or if we roll, go there
			nextPosition = neutral;
		else
			RandomNewEyesPosition();

		while(Time.time < startTime + timeToMove)
		{
			eyeballs.localPosition = Vector2.Lerp(startPos, nextPosition, (Time.time - startTime)/timeToMove);
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(UnityEngine.Random.Range(0.01f, 1f));
		StartCoroutine("AlertEyes");
	}

	public IEnumerator Blinking()
	{		
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 5f));

		eyesBlinking.enabled = true;

		yield return new WaitForSeconds(0.15f);

		eyesBlinking.enabled = false;

		StartCoroutine("Blinking");
	}

	public IEnumerator Speaking()
	{
		//speaking = true;
		Invoke ("StopSpeaking", 1);
		mouth.sprite = GetARandomSprite(myAppearance.speakingMouthShapes);

		yield return new WaitForSeconds(0.1f);

		mouth.sprite = originalMouth;

		yield return new WaitForSeconds(0.1f);

		StartCoroutine("Speaking");
	}

	void StopSpeaking()
	{
		StopCoroutine("Speaking");
		mouth.sprite = originalMouth;
		//speaking = false;
	}

	void RandomNewEyesPosition()
	{
		nextPosition = eyePositions[UnityEngine.Random.Range(0, eyePositions.Length)];
	}

	public IEnumerator TenseMouth()
	{
		mouth.sprite = GetARandomSprite(myAppearance.tenseMouthSahpes);
		yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 1f));
		mouth.sprite = originalMouth;
	}

	public void SetBPM(int newBpm)
	{
		heartbeatScript.bpm = newBpm;
	}
	#endregion

	void OnDisable()
	{
		StopAllCoroutines();
	}
}
