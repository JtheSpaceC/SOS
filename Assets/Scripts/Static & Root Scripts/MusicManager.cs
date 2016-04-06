using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {

	public static MusicManager instance;

	private GameObject player;

	public bool persistAfterLoad = true;

	public AudioSource[] musicClips;
	private AudioSource turntableA;
	public float turntableAMaxVolume = 1.0f;
	private AudioSource turntableB;
	public float turntableBMaxVolume = 0.5f;

	public float fadeSpeed = 0.25f;

	public float dangerZone = 75f;
	public LayerMask mask1;
	public bool InAction = false;

	public bool muteMusic = false;


	
	void Awake()
	{
		if(instance == null && persistAfterLoad)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else if(instance == null && !persistAfterLoad)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}

		turntableA = musicClips [0];
		turntableB = musicClips [1];

		InvokeRepeating ("CheckForAPlayer", 0, 5);
	}

	void CheckForAPlayer()
	{
		if(GameObject.FindGameObjectWithTag ("PlayerFighter") != null)
		{
			player = GameObject.FindGameObjectWithTag ("PlayerFighter");
		}
		else 
			player = null;
	}

	void Update()
	{
		if(!muteMusic)
		{
			if(player != null)
			{				
				Collider2D[] enemyFighterArray = Physics2D.OverlapCircleAll (player.transform.position, dangerZone, mask1);
				if(enemyFighterArray.Length <= 0)
				{
					Invoke("SwitchInAction", 2f);
				}
				else if (enemyFighterArray.Length > 0)
				{
					InAction = true;
				}
				
				
				if(InAction == false)
				{
					FadeIn(turntableA);
					FadeOut(turntableB);
				}
				
				else if (InAction == true) 
				{
					FadeIn(turntableB);
					FadeOut(turntableA);
				}
			}
			else
			{
				FadeIn(turntableA);
				FadeOut(turntableB);
			}

		}

		else if(muteMusic)
		{
			FadeOut(turntableA);
			FadeOut(turntableB);
		}
	}

	void SwitchInAction()
	{
		InAction = false;
	}
	
	public void FadeIn(AudioSource whichTrack)
	{
		if(!turntableA.isPlaying && !turntableB.isPlaying)
		{
			turntableA.Play();
			turntableB.Play();
		}
		if(whichTrack == turntableA && turntableA.volume < turntableAMaxVolume)
		{
			whichTrack.volume += fadeSpeed * Time.deltaTime; 
		}

		else if(whichTrack == turntableB && turntableB.volume < turntableBMaxVolume)
		{
			whichTrack.volume += fadeSpeed * Time.deltaTime; 
		}
	}
	
	public void FadeOut(AudioSource whichTrack)
	{
		if(whichTrack == turntableA && turntableA.volume > 0)
		{
			whichTrack.volume -= fadeSpeed * Time.deltaTime;
		}
		else if(whichTrack == turntableB && turntableB.volume >0)
		{
			whichTrack.volume -= fadeSpeed * Time.deltaTime;
		}

		if(turntableA.volume == 0 && turntableB.volume == 0)
		{
			turntableA.Stop();
			turntableB.Stop();
		}
	}

	public void BecomeDontDestroyOnLoad()
	{
		DontDestroyOnLoad (gameObject);
		instance = this;
		persistAfterLoad = true;
	}
	public void BecomeDestroyOnLoad()
	{
		DontDestroyOnLoad (gameObject);
		persistAfterLoad = false;
	}
	public void DestroyThisNow()
	{
		DestroyImmediate (gameObject);
	}
	
}//Mono