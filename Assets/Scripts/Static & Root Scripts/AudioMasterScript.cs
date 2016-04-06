using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class AudioMasterScript : MonoBehaviour {

	public static AudioMasterScript instance;

	public AudioMixer masterMixer;


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
}
