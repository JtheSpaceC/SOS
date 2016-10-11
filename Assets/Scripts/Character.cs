using AssemblyCSharp;
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
	public string appearanceSeed; //NB!! Do in this order: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp

	public bool inSpace = true;

	public Appearance appearances;
	public Names names;
	public GameObject avatarOutputPrefab;
	RenderTexture myRenderTexture;

	[Header("Appearance")]
	public SpriteRenderer body;
	public SpriteRenderer eyes;
	public SpriteRenderer eyesProp;
	public SpriteRenderer facialFeature;
	public SpriteRenderer nose;
	public SpriteRenderer mouth;
	public Transform eyeballs;
	public SpriteRenderer facialHair;
	public SpriteRenderer hair;
	public SpriteRenderer clothes;
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
	[HideInInspector] public Sprite originalEyes;

	//for speaking & mouth behaviour
	Sprite originalMouth;
	//bool speaking = false;




	void Awake()
	{
		myRenderTexture = new RenderTexture(256, 256, 24);
		myRenderTexture.name = "Avatar RT "+ transform.root.gameObject.name;
		bgScrollerScript = GetComponentInChildren<BGScroller>();

		//appearances.avatarWorldPositionModifier = 0;
	}

	void Start () 
	{
		appearances.avatarWorldPositionModifier++;

		transform.position = new Vector3 (appearances.avatarWorldPositionModifier * 5, 1000, 1000); //stops avatars lining up on each other

		transform.SetParent(null);

		heartbeatScript = FindObjectOfType<Heartbeat>();
		eyePositions = new Vector2[] {neutral, up, upperRight, right, lowerRight, down, lowerLeft, left, upperLeft};

		//if we're not loading in an appearance (if we are this happens but is overridden, currently)
		GenerateRandomNewAppearance();
		GenerateName();

		originalEyes = eyes.sprite;
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
			avatarOutput.transform.FindChild("Flash Image/Unit Number").GetComponent<Image>().sprite = appearances.unitNumbers[mySquadUnitNumber];
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
		//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature, Helmet, HelmetColour

		if(UnityEngine.Random.Range(0f, 2f) > 1)
		{
			gender = Gender.Male;
			appearanceSeed = "0";
		}
		else 
		{
			gender = Gender.Female;
			appearanceSeed = "1";
		}

		body.sprite = appearances.baseBody[NewSeed(appearances.baseBody.Length)];
		body.color = appearances.skinTones[NewSeed(appearances.skinTones.Length)];
		nose.sprite = appearances.noses[NewSeed(appearances.noses.Length)];
		eyes.sprite = gender == Gender.Male? appearances.eyesMale[NewSeed(appearances.eyesMale.Length)] : 
			appearances.eyesFemale[NewSeed(appearances.eyesFemale.Length)];
		originalEyes = eyes.sprite;
		hair.sprite = gender == Gender.Male? appearances.hairMale[NewSeed(appearances.hairMale.Length)] : 
			appearances.hairFemale[NewSeed(appearances.hairFemale.Length)];

		//facial hair
		if(gender == Gender.Male)
		{
			//50:50 chance to be a clean shaven male
			if(UnityEngine.Random.Range(0,10) >=5)
			{
				facialHair.sprite = appearances.facialHair[NewSeed(appearances.facialHair.Length)]; 
			}
			else
				NewSeed(0);
		}
		else 
		{
			facialHair.sprite = null;
			NewSeed(0);
		}

		hair.color = appearances.hairColours[NewSeed(appearances.hairColours.Length)];
		facialHair.color = hair.color;

		//chance of having a prop for the eyes
		if(UnityEngine.Random.Range(0f, 2f) > 1.33f)
		{
			eyesProp.sprite = appearances.eyesProp[NewSeed(appearances.eyesProp.Length)];
		}
		else
		{
			eyesProp.sprite = null;
			NewSeed(0);
		}

		//chance of having a facial feature like a scar
		if(UnityEngine.Random.Range(0f, 2f) > 1.33f)
		{
			facialFeature.sprite = appearances.facialFeatures[NewSeed(appearances.facialFeatures.Length)];
			AdjustFacialFeatureColour();
		}
		else
		{
			facialFeature.sprite = null;
			NewSeed(0);
		}

		helmet.sprite = appearances.helmets[NewSeed(appearances.helmets.Length)];
		helmet.color = appearances.spaceSuitColours[NewSeed(appearances.spaceSuitColours.Length)];
		spaceSuit.color = helmet.color;

		//SEED OVER. REST IS IRRELEVANT FOR SEED AT THE MOMENT
		//currently only one mouth. Ignore for seed
		mouth.sprite = GetARandomSprite(appearances.mouths);

		clothes.sprite = GetARandomSprite(appearances.clothes);
		helmet.sprite = GetARandomSprite(appearances.helmets);
		spaceSuit.sprite = GetARandomSprite(appearances.spaceSuits);

	}//end of GenerateRandomNewAppearance

	public void GenerateName()
	{
		//Get a first and last name. We may check this against names in use later, so we don't have the same names in a campaign
		GetFirstAndLastName();
		string fullName = firstName+lastName;

		//Do the same for callsigns
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
				print("Contained Callsign " + callsign);
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
		appearanceSeed += choice.ToString();
		return choice;
	}

	public Sprite GetARandomSprite(Sprite[] whatArray)
	{
		return whatArray[UnityEngine.Random.Range(0, whatArray.Length)];
	}

	public void GenerateAppearanceBySeed(char[] seed)
	{
		//FOR SEED (string): ORDER IS: Gender, Body, Skin Colour, Nose, Eyes, Hair, FacialHair, HairColour, EyesProp, FacialFeature

		if(seed[0] == '0') //male
			gender = Gender.Male;
		else if(seed[0] == '1') //female
			gender = Gender.Female;
		else
			Debug.Log("Something Went Wrong");

		body.sprite = appearances.baseBody[Int32.Parse(seed[1].ToString())];
		body.color = appearances.skinTones[Int32.Parse(seed[2].ToString())];
		nose.sprite = appearances.noses[Int32.Parse(seed[3].ToString())];
		eyes.sprite = gender == Gender.Male? appearances.eyesMale[Int32.Parse(seed[4].ToString())] : 
			appearances.eyesFemale[Int32.Parse(seed[4].ToString())];
		originalEyes = eyes.sprite;
		hair.sprite = gender == Gender.Male? appearances.hairMale[Int32.Parse(seed[5].ToString())] : 
			appearances.hairFemale[Int32.Parse(seed[5].ToString())];

		//facial hair
		if(gender == Gender.Male)
		{
			facialHair.sprite = appearances.facialHair[Int32.Parse(seed[6].ToString())];
		}
		else 
		{
			facialHair.sprite = null;
		}

		hair.color = appearances.hairColours[Int32.Parse(seed[7].ToString())];
		facialHair.color = hair.color;

		eyesProp.sprite = appearances.eyesProp[Int32.Parse(seed[8].ToString())];

		facialFeature.sprite = appearances.facialFeatures[Int32.Parse(seed[9].ToString())];
		AdjustFacialFeatureColour();

		helmet.sprite = appearances.helmets[Int32.Parse(seed[10].ToString())];
		helmet.color = appearances.spaceSuitColours[Int32.Parse(seed[11].ToString())];
		spaceSuit.color = helmet.color;
	}

	public void AdjustFacialFeatureColour() //mostly for scar colour matching skin tone
	{
		facialFeature.color = Color.Lerp(body.color, Color.black, 0.4f);
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

		eyes.sprite = GetARandomSprite(appearances.eyesBlinking);
		eyeballs.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.15f);

		eyes.sprite = originalEyes;
		eyeballs.gameObject.SetActive(true);
		StartCoroutine("Blinking");
	}

	public IEnumerator Speaking()
	{
		//speaking = true;
		Invoke ("StopSpeaking", 1);
		mouth.sprite = GetARandomSprite(appearances.speakingMouthShapes);

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
		mouth.sprite = GetARandomSprite(appearances.tenseMouthSahpes);
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
