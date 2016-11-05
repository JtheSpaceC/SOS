using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class _voiceManager : MonoBehaviour {

	SquadronLeader playerSquadLeadScript;
	AudioSource[] myAudioSources;

	public AudioClip[] wingmanFirstClash;
	public AudioClip[] wingmanGotAKill;
	public AudioClip[] wingmanInTrouble;
	public AudioClip[] wingmanBackInAction;
	public AudioClip[] wingmanAcknowledge;
	public AudioClip[] wingmanDied;

	public AudioClip[] pickupOnTheWay;
	public AudioClip[] pickupNegative;

	public AudioClip[] playerLeaving;
	public AudioClip[] playerShotDown;

	public bool haveAcceptedAudioThisFrame = false;

	void Start()
	{
		myAudioSources = GetComponentsInChildren<AudioSource> ();
		if (GameObject.FindGameObjectWithTag ("PlayerFighter") != null)
			playerSquadLeadScript = GameObject.FindGameObjectWithTag ("PlayerFighter").GetComponentInChildren<SquadronLeader> ();
	}

	void OnEnable()
	{
		_battleEventManager.wingmanFirstClash += WingmenDeclareFirstClash;
		_battleEventManager.wingmanGotAKill += WingmanDeclareGotAKill;
		_battleEventManager.wingmanInTrouble += WingmanDeclareInTrouble;
		_battleEventManager.wingmanBack += WingmanDeclareBack;
		_battleEventManager.wingmanAcknowledgeOrder += WingmanAcknowledgeOrder;
		_battleEventManager.wingmanDied += WingmanDied;

		_battleEventManager.pickupOnTheWay += PickupOnTheWay;
		_battleEventManager.pickupNegative += PickupNegative;

		_battleEventManager.ordersCoverMe += OrdersCoverMe;

		_battleEventManager.missionComplete += MissionComplete;

		_battleEventManager.playerLeavingByWarp += PlayerLeaving;
		_battleEventManager.playerShotDown += PlayerShotDown;
	}
	void OnDisable()
	{
		_battleEventManager.wingmanFirstClash -= WingmenDeclareFirstClash;		
		_battleEventManager.wingmanGotAKill -= WingmanDeclareGotAKill;
		_battleEventManager.wingmanInTrouble -= WingmanDeclareInTrouble;
		_battleEventManager.wingmanBack -= WingmanDeclareBack;
		_battleEventManager.wingmanAcknowledgeOrder -= WingmanAcknowledgeOrder;
		_battleEventManager.wingmanDied -= WingmanDied;

		_battleEventManager.pickupOnTheWay -= PickupOnTheWay;
		_battleEventManager.pickupNegative -= PickupNegative;

		_battleEventManager.ordersCoverMe -= OrdersCoverMe;

		_battleEventManager.missionComplete -= MissionComplete;

		_battleEventManager.playerLeavingByWarp -= PlayerLeaving;
		_battleEventManager.playerShotDown -= PlayerShotDown;
	}

	void LoadClipAndPlay(AudioClip[] ac)
	{
		/*try{
		if (ac.Length != 0) {
			myAudioSources.clip = ac [Random.Range (0, ac.Length)];
			myAudioSources.Play ();
		}
		}catch{Debug.LogError("Voice Manager reports it didn't work?!");
		}*/
		try{
		if (ac.Length != 0 && !haveAcceptedAudioThisFrame) {
				for(int i = 0; i < myAudioSources.Length; i++)
				{
					if(!myAudioSources[i].isPlaying)
					{
						myAudioSources[i].clip = ac [Random.Range (0, ac.Length)];
						myAudioSources[i].Play ();
						haveAcceptedAudioThisFrame = true;
						StartCoroutine(TurnOffBoolNextFrame());
						break;
					}
				}
		}
		}catch{//Debug.LogError("Voice Manager reports it didn't work?!");
		}
	}
	IEnumerator TurnOffBoolNextFrame()
	{
		yield return new WaitForEndOfFrame ();
		haveAcceptedAudioThisFrame = false;
	}

	void WingmenDeclareFirstClash()
	{
		if(playerSquadLeadScript == null || playerSquadLeadScript.activeWingmen.Count >= 1)
		{
			LoadClipAndPlay(wingmanFirstClash);
			if(playerSquadLeadScript)
				playerSquadLeadScript.activeWingmen[0].GetComponent<AIFighter>().myCharacterAvatarScript.StartCoroutine("Speaking");
		}
	}

	void WingmanDeclareGotAKill()
	{
		LoadClipAndPlay (wingmanGotAKill);
	}

	void WingmanDeclareInTrouble()
	{
		LoadClipAndPlay (wingmanInTrouble);
	}

	void WingmanDeclareBack()
	{
		LoadClipAndPlay (wingmanBackInAction);
	}

	void WingmanAcknowledgeOrder()
	{
		LoadClipAndPlay (wingmanAcknowledge);
	}

	void WingmanDied()
	{
		LoadClipAndPlay (wingmanDied);
	}

	void PickupOnTheWay()
	{
		LoadClipAndPlay (pickupOnTheWay);
	}

	void PickupNegative()
	{
		LoadClipAndPlay (pickupNegative);
	}

	void OrdersCoverMe()
	{
		
	}

	void MissionComplete()
	{
		
	}

	void PlayerLeaving()
	{
		LoadClipAndPlay (playerLeaving);
	}

	void PlayerShotDown()
	{
		
	}



}//Mono
