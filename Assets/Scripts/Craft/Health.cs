using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class Health : MonoBehaviour {

	public bool dead = false;
	public int health = 5;
	public int maxHealth = 5; //formerly 100
	public int awareness = 0;
	public int maxAwareness = 5;

	public enum AwarenessMode {Recharge, SkillBased};
	[Tooltip("Only used with Player")]
	public AwarenessMode awarenessMode;

	[Tooltip ("If using Recharge mode only. Per bar, in seconds")]
	public float awarenessRechargeTime = 3;
	public int snapFocusAmount = 1;
	public int armourPoints = 0;

	//not used on basic fighters now
	public float healthRecoveryRate = 0; //formerly 2
	public bool hasFireExtinguisher = true;

	public Vector3 sixOclockPos = new Vector3(0, -6, 0);

	public bool temporarilyInvincible = false; //used instead of flashing the collider on and off. Prevents two shots close together both hitting													

	[Header("For Effects")]
	public ParticleSystem smoke;
	protected ParticleSystem.EmissionModule smokeEm;
	public ParticleSystem flames;
	protected ParticleSystem.EmissionModule flamesEm;
	public Slider healthSlider;
	public GameObject healthBarDividingBox;
	public Slider awarenessSlider;
	public GameObject awarenessBarDividingBox;
	Image healthSliderFill;
	public AudioClip playerHitSound;
	protected Image bloodSplashImage;
	public Color bloodSplashColour;
	public float flashFadeSpeed = 5;
	protected AudioSource myAudioSource;




	protected void AwakeBaseClass()
	{
		dead = false;
		health = Mathf.Clamp(health, 0, maxHealth);
		awareness = Mathf.Clamp(awareness, 0, maxAwareness);

		GameObject craftsSix = new GameObject();
		craftsSix.transform.parent = this.transform;
		craftsSix.transform.position = this.transform.position + sixOclockPos;
		craftsSix.name = "Craft's Six";

		bloodSplashImage = GameObject.FindGameObjectWithTag ("Manager Tools").transform.FindChild 
			("Canvas (Effects, screen)/Blood Splash").GetComponent<Image>();

		if(healthSlider != null)
		{
			healthSliderFill = healthSlider.GetComponentInChildren<Image> ();
			healthBarDividingBox.SetActive(false);
			for (int i = 1; i < maxHealth; i++)
			{
				GameObject newBox = Instantiate(healthBarDividingBox);
				newBox.transform.SetParent(healthBarDividingBox.transform.parent);
				newBox.transform.localScale = Vector3.one;
			}
			for(int i = 0; i < healthBarDividingBox.transform.parent.childCount; i++)
			{
				healthBarDividingBox.transform.parent.GetChild(i).gameObject.SetActive(true);
			}
		}
		if(awarenessSlider != null)
		{
			awarenessBarDividingBox.SetActive(false);
			for (int i = 1; i < maxAwareness; i++)
			{
				GameObject newBox = Instantiate(awarenessBarDividingBox);
				newBox.transform.SetParent(awarenessBarDividingBox.transform.parent);
				newBox.transform.localScale = Vector3.one;
			}
			for(int i = 0; i < awarenessBarDividingBox.transform.parent.childCount; i++)
			{
				awarenessBarDividingBox.transform.parent.GetChild(i).gameObject.SetActive(true);
			}

			awarenessSlider.value = (float)awareness/maxAwareness * 100/1;
		}

		myAudioSource = GetComponent<AudioSource> ();

		smokeEm = smoke.emission;
		flamesEm = flames.emission;
	}


	protected void UpdateBaseClass()
	{
		if(health < maxHealth && healthSliderFill != null)
		{
			healthSliderFill.color = Color.Lerp(Color.green, Color.red, 1 -(float)health /(float)maxHealth);
			healthBarDividingBox.transform.parent.gameObject.SetActive(true);
		}

		if(this.tag == "PlayerFighter")
		{
			healthSlider.value = (float)health/(float)maxHealth * 100f;
			awarenessSlider.value = (float)awareness/(float)maxAwareness * 100f;

			//FOR HEALTH BAR COLOUR
			/*float alpha = healthSliderFill.color.a;
			
			if(health < maxHealth)
			{
				alpha = 0.75f;
			}
			
			else*/ if(health >=maxHealth)
			{
				//alpha = 0f;
				//healthSliderFill.color = Color.green * alpha;
				smoke.gameObject.SetActive(false);
				flames.gameObject.SetActive(false);
			}
			
			//FOR BLOOD SPLASH
			if(bloodSplashImage.color != Color.clear)
			{
				bloodSplashImage.color = Color.Lerp (bloodSplashImage.color, Color.clear, flashFadeSpeed * Time.deltaTime);
			}
		}

		if(healthSlider && awarenessSlider)
		{
			healthSlider.value = (float)health/(float)maxHealth * 100;
			awarenessSlider.value = (float)awareness/(float)maxAwareness * 100;
		}
	}


	protected IEnumerator FlashOnInvincibility(int frameDelay)
	{
		while(frameDelay > 0) //a wait for 1 or 2 frames allows basically accurate shots to all hit instead of the 2nd bullet missing
		{
			frameDelay --;
			yield return new WaitForEndOfFrame(); yield return new WaitForEndOfFrame();
		}
		temporarilyInvincible = true;
		yield return new WaitForSeconds (0.25f);
		temporarilyInvincible = false;
	}


	protected void Deactivate()
	{
		transform.SetParent (GameObject.Find ("Dead Craft Bin (doesn't destroy)").transform);
		gameObject.SetActive (false);
	}

}//MONO
