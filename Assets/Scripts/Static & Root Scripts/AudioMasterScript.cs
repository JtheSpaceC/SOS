using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioMasterScript : MonoBehaviour {

	public static AudioMasterScript instance;

	public AudioMixer masterMixer;

	[Tooltip("Mostly for Debug")]
	public AudioClip chime;


	void Awake () 
	{
		if (instance == null)
			instance = this;
		else 
		{
			Debug.Log("There were 2 AudioMasterScripts. Destroyed 1");
			Destroy (this.gameObject);
		}
	}

	public void MuteSFX()
	{
		masterMixer.SetFloat ("SFX vol", -80);
	}
	public void ZeroSFX()
	{
		masterMixer.SetFloat ("SFX vol", 0);
	}

	public void MuteMusic()
	{
		masterMixer.SetFloat("Music vol", -80);
	}

	public void ClearAll()
	{
		masterMixer.ClearFloat ("Master vol");
		masterMixer.ClearFloat ("SFX vol");
		masterMixer.ClearFloat ("UI vol");
		masterMixer.ClearFloat ("Voice vol");
		masterMixer.ClearFloat ("Music vol");
	}

	public void FadeChannel(string whichChannel, float toVol, float startDelay, float fadeTime)
	{
		StartCoroutine(StartFadeChannel(whichChannel, toVol, startDelay, fadeTime));
	}


	IEnumerator StartFadeChannel(string whichChannel, float toVol, float startDelay, float fadeTime)
	{
		yield return new WaitForSeconds(startDelay);

		float startTime = Time.time;
		float currentVol;
		masterMixer.GetFloat(whichChannel, out currentVol);
		float startVol = currentVol;

		while(currentVol != toVol)
		{
			masterMixer.SetFloat(whichChannel, Mathf.SmoothStep(startVol, toVol, (Time.time - startTime)/fadeTime));
			masterMixer.GetFloat(whichChannel, out currentVol);	
			yield return new WaitForEndOfFrame();
		}

	}

	public void PlayChime()
	{
		AudioSource.PlayClipAtPoint(chime, Camera.main.transform.position);
	}

}
