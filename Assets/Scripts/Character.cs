using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Character : MonoBehaviour {

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
	public string appearanceSeed; //APPEARANCE_SEED //NB!! FOR SEED (string): ORDER IS: 
	//Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp

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
	public SpriteRenderer ears;
	public SpriteRenderer head;
	public SpriteRenderer chin;
	public SpriteRenderer eyeLids;
	public SpriteRenderer eyeWhites;
	public SpriteRenderer eyeIrises;
	public SpriteRenderer eyesBlinking;
	public Transform eyeballs;
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
	[HideInInspector] public Sprite[] eyeSet; //0 is lids, 1 is whites, 2 is irises, 3 is blinking

	//for speaking & mouth behaviour
	Sprite originalMouth;
	//bool speaking = false;

	int callsignChecks;

	void Awake()
	{
		myRenderTexture = new RenderTexture(256, 256, 24);
		myRenderTexture.name = "Avatar RT "+ transform.root.gameObject.name;
		bgScrollerScript = GetComponentInChildren<BGScroller>();

		eyeSet = new Sprite[4];

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
		//APPEARANCE_SEED
		//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, HelmetColour

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

		body.sprite = myAppearance.baseBody[NewSeed(myAppearance.baseBody.Length)];
		body.color = myAppearance.skinTones[NewSeed(myAppearance.skinTones.Length)];
		nose.sprite = myAppearance.noses[NewSeed(myAppearance.noses.Length)];
		int numSetsOfEyes = (myAppearance.eyes.Length /3)-1; //TODO: 4 when we add lids 
		int eyeSetChoice = NewSeed(numSetsOfEyes) * 3;
		eyeLids.sprite = myAppearance.eyes[eyeSetChoice];
		eyeWhites.sprite = myAppearance.eyes[eyeSetChoice+1];
		eyeIrises.sprite = myAppearance.eyes[eyeSetChoice+2];
		//eyesBlinking.sprite = myAppearance.eyes[eyeSetChoice+3];
		eyeSet[0] = eyeLids.sprite;
		eyeSet[1] = eyeWhites.sprite;
		eyeSet[2] = eyeIrises.sprite;
		eyeSet[3] = eyesBlinking.sprite;
		hair.sprite = myAppearance.hair[NewSeed(myAppearance.hair.Length)];

		//facial hair
		if(gender == Gender.Male)
		{
			//50:50 chance to be a clean shaven male
			if(UnityEngine.Random.Range(0,10) >=5)
			{
				facialHair.sprite = myAppearance.facialHair[NewSeed(myAppearance.facialHair.Length)]; 
			}
			else
				NewSeed(0);
		}
		else 
		{
			facialHair.sprite = null;
			NewSeed(0);
		}

		hair.color = myAppearance.hairColours[NewSeed(myAppearance.hairColours.Length)];
		facialHair.color = hair.color;
		eyebrows.color = hair.color;

		//chance of having a prop for the eyes
		if(UnityEngine.Random.Range(0,100) >= chanceOfEyesProp)
		{
			eyesProp.sprite = myAppearance.eyesProp[NewSeed(myAppearance.eyesProp.Length)];
		}
		else
		{
			eyesProp.sprite = null;
			NewSeed(0);
		}

		//chance of having a facial feature like a scar
		if(UnityEngine.Random.Range(0f, 2f) > 1.33f)
		{
			facialFeatures1.sprite = myAppearance.facialFeatures1[NewSeed(myAppearance.facialFeatures1.Length)];
			AdjustFacialFeatureColour();
			//TODO: FF1 & 2, Scars 1 & 2
		}
		else
		{
			facialFeatures1.sprite = null;
			NewSeed(0);
		}

		helmet.sprite = myAppearance.helmets[NewSeed(myAppearance.helmets.Length)];
		helmet.color = myAppearance.spaceSuitColours[NewSeed(myAppearance.spaceSuitColours.Length)];
		spaceSuit.color = helmet.color;

		//SEED OVER. REST IS IRRELEVANT FOR SEED AT THE MOMENT
		//currently only one mouth. Ignore for seed
		mouth.sprite = GetARandomSprite(myAppearance.mouths);

		clothes.sprite = GetARandomSprite(myAppearance.clothes);
		helmet.sprite = GetARandomSprite(myAppearance.helmets);
		spaceSuit.sprite = GetARandomSprite(myAppearance.spaceSuits);

	}//end of GenerateRandomNewAppearance

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

	int NewSeed(int arrayLength) //this function adds its result to the string recording the seed of this appearance
	{
		int choice = UnityEngine.Random.Range(0, arrayLength);
		appearanceSeed += choice.ToString() + ",";
		return choice;
	}

	public Sprite GetARandomSprite(Sprite[] whatArray)
	{
		return whatArray[UnityEngine.Random.Range(0, whatArray.Length)];
	}

	public void GenerateAppearanceBySeed(string[] seed)
	{
		//APPEARANCE_SEED
		//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature

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

		body.sprite = myAppearance.baseBody[Int32.Parse(seed[1].ToString())];
		body.color = myAppearance.skinTones[Int32.Parse(seed[2].ToString())];
		nose.sprite = myAppearance.noses[Int32.Parse(seed[3].ToString())];
		eyeLids.sprite = myAppearance.eyes[Int32.Parse(seed[4].ToString())];
		eyeSet[0] = eyeLids.sprite;
		eyeSet[1] = eyeWhites.sprite;
		eyeSet[2] = eyeIrises.sprite;
		eyeSet[3] = eyesBlinking.sprite;
		hair.sprite = myAppearance.hair[Int32.Parse(seed[5].ToString())];

		//facial hair
		if(gender == Gender.Male)
		{
			facialHair.sprite = myAppearance.facialHair[Int32.Parse(seed[6].ToString())];
		}
		else 
		{
			facialHair.sprite = null; 
		}

		hair.color = myAppearance.hairColours[Int32.Parse(seed[7].ToString())];
		facialHair.color = hair.color;

		eyesProp.sprite = myAppearance.eyesProp[Int32.Parse(seed[8].ToString())];

		facialFeatures1.sprite = myAppearance.facialFeatures1[Int32.Parse(seed[9].ToString())];
		//TODO: FF 1 & 2, Scars 1 & 2

		AdjustFacialFeatureColour();

		helmet.sprite = myAppearance.helmets[Int32.Parse(seed[10].ToString())];
		helmet.color = myAppearance.spaceSuitColours[Int32.Parse(seed[11].ToString())];
		spaceSuit.color = helmet.color;
	}

	public void AdjustFacialFeatureColour() //mostly for scar colour matching skin tone
	{
		Debug.Log("Accessing AdjustFacialFeatureColour(), but it's commented out.");
		//facialFeatures1.color = Color.Lerp(body.color, Color.black, 0.4f);
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
