using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Character : MonoBehaviour {

	[HideInInspector] public Heartbeat heartbeatScript;
	[HideInInspector] public BGScroller bgScrollerScript;
	[HideInInspector] public GameObject avatarOutput;

	public enum Gender{Male, Female};
	public Gender gender;

	public bool inSpace = true;

	public Appearance appearances;
	public GameObject avatarOutputPrefab;
	RenderTexture myRenderTexture;

	[Header("Appearance")]
	public SpriteRenderer body;
	public SpriteRenderer eyes;
	public SpriteRenderer glasses;
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
	Sprite originalEyes;

	//for speaking & mouth behaviour
	Sprite originalMouth;
	bool speaking = false;



	void Awake()
	{
		myRenderTexture = new RenderTexture(256, 256, 24);
		myRenderTexture.name = "Avatar RT "+ transform.root.gameObject.name;
		bgScrollerScript = GetComponentInChildren<BGScroller>();

		appearances.avatarWorldPositionModifier = 0;
	}

	void Start () 
	{
		appearances.avatarWorldPositionModifier++;

		transform.position = new Vector3 (appearances.avatarWorldPositionModifier * 5, 1000, 1000); //stops avatars lining up on each other

		transform.SetParent(null);

		heartbeatScript = FindObjectOfType<Heartbeat>();
		eyePositions = new Vector2[] {neutral, up, upperRight, right, lowerRight, down, lowerLeft, left, upperLeft};

		GenerateNewAppearance();

		originalEyes = eyes.sprite;
		originalMouth = mouth.sprite;

		StartCoroutine("AlertEyes");
		StartCoroutine("Blinking");

		if(!inSpace)
		{
			helmet.enabled = false;
			spaceSuit.enabled = false;
			cockpit.enabled = false;
		}
		else
		{
			hair.enabled = false;
			clothes.enabled = false;
		}

	}

	public void SetUpAvatar (int mySquadUnitNumber)
	{
		GetComponentInChildren<Camera>().targetTexture = myRenderTexture;
		avatarOutput = Instantiate (avatarOutputPrefab) as GameObject;
		avatarOutput.transform.SetParent (Tools.instance.avatarsPanelUI.transform);
		avatarOutput.GetComponent<RawImage> ().texture = myRenderTexture;
		avatarOutput.transform.localScale = Vector3.one;
		avatarOutput.transform.FindChild("Flash Image/Unit Number").GetComponent<Image>().sprite = appearances.unitNumbers[mySquadUnitNumber];
	}

	#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.O))
			StartCoroutine("TenseMouth");
	}
	#endif

	[ContextMenu("Generate Appearance")]
	void GenerateNewAppearance()
	{ 
		if(Random.Range(0f, 2f) > 1)
		{
			gender = Gender.Male;
		}
		else 
		{
			gender = Gender.Female;
		}

		body.sprite = GetASprite(appearances.baseBody);
		body.color = appearances.skinTones[Random.Range(0, appearances.skinTones.Length)];
		nose.sprite = GetASprite(appearances.noses);
		eyes.sprite = gender == Gender.Male? GetASprite(appearances.eyesMale) : GetASprite(appearances.eyesFemale);
		mouth.sprite = GetASprite(appearances.mouths);
		clothes.sprite = GetASprite(appearances.clothes);
		helmet.sprite = GetASprite(appearances.helmets);
		spaceSuit.sprite = GetASprite(appearances.spaceSuits);

		if(gender == Gender.Male)
		{
			facialHair.sprite = Random.Range(0,10) >=5? GetASprite(appearances.facialHair): null; //50:50 chance to be a clean shaven male
		}
		else facialHair.sprite = null;

		hair.sprite = gender == Gender.Male? GetASprite(appearances.hairMale) : GetASprite(appearances.hairFemale);
		hair.color = appearances.hairColours[Random.Range(0, appearances.hairColours.Length)];
		facialHair.color = hair.color;

		if(Random.Range(0f, 2f) > 1.33f)
		{
			glasses.sprite = GetASprite(appearances.glasses);
		}
		else glasses.sprite = null;
	}

	public Sprite GetASprite(Sprite[] whatArray)
	{
		return whatArray[Random.Range(0, whatArray.Length)];
	}

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

		yield return new WaitForSeconds(Random.Range(0.01f, 2f));
		StartCoroutine("GuiltyEyes");
	}

	public IEnumerator AlertEyes()
	{
		startPos = eyeballs.localPosition;
		startTime = Time.time;

		if(nextPosition != neutral || Random.Range(0f, 10f) > 2.5f) //if we were not previously in neutral, go there, or if we roll, go there
			nextPosition = neutral;
		else
			RandomNewEyesPosition();

		while(Time.time < startTime + timeToMove)
		{
			eyeballs.localPosition = Vector2.Lerp(startPos, nextPosition, (Time.time - startTime)/timeToMove);
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(Random.Range(0.01f, 1f));
		StartCoroutine("AlertEyes");
	}

	public IEnumerator Blinking()
	{		
		yield return new WaitForSeconds(Random.Range(0.5f, 5f));

		eyes.sprite = GetASprite(appearances.eyesBlinking);
		eyeballs.gameObject.SetActive(false);
		yield return new WaitForSeconds(0.15f);

		eyes.sprite = originalEyes;
		eyeballs.gameObject.SetActive(true);
		StartCoroutine("Blinking");
	}

	public IEnumerator Speaking()
	{
		speaking = true;
		Invoke ("StopSpeaking", 1);
		mouth.sprite = GetASprite(appearances.speakingMouthShapes);

		yield return new WaitForSeconds(0.1f);

		mouth.sprite = originalMouth;

		yield return new WaitForSeconds(0.1f);

		StartCoroutine("Speaking");
	}

	void StopSpeaking()
	{
		StopCoroutine("Speaking");
		mouth.sprite = originalMouth;
		speaking = false;
	}

	void RandomNewEyesPosition()
	{
		nextPosition = eyePositions[Random.Range(0, eyePositions.Length)];
	}

	public IEnumerator TenseMouth()
	{
		mouth.sprite = GetASprite(appearances.tenseMouthSahpes);
		yield return new WaitForSeconds(Random.Range(0.3f, 1f));
		mouth.sprite = originalMouth;
	}

	public void SetBPM(int newBpm)
	{
		heartbeatScript.bpm = newBpm;
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}
}
