using UnityEngine;

public class PlayerPrefsManager: MonoBehaviour {

	static bool doDebugs = false;

	const string CONTROLLER_VIBRATE_KEY = "Controller_Vibrate_Key";
	const string CONTROLLER_STICK_BEHAVIOUR_KEY = "Controller_Stick_Behaviour_Key";

	public enum LeftGamepadStickBehaviour {stickRotates, stickPoints};
	public static LeftGamepadStickBehaviour leftGamepadStickBehaviour;


	public static void GetSomething()
	{
		
	}

	public static void SetSomething()
	{

	}


	public static string GetVibrateKey()
	{
		return PlayerPrefs.GetString(CONTROLLER_VIBRATE_KEY);
	}
	public static void SetVibrateKey(string trueOrFalse)
	{
		if(doDebugs)
		{
			Debug.Log("PLAYERPREFSMANAGER Setting Vibrate to " + trueOrFalse);
		}
		PlayerPrefs.SetString(CONTROLLER_VIBRATE_KEY, trueOrFalse);

		PlayerPrefs.Save();
	}


	public static string GetControllerStickBehaviourKey()
	{
		return PlayerPrefs.GetString(CONTROLLER_STICK_BEHAVIOUR_KEY);
	}
	public static void SetControllerStickBehaviourKey(string whichScheme)
	{
		if (whichScheme == "StickPoints")
		{
			leftGamepadStickBehaviour = LeftGamepadStickBehaviour.stickPoints;
			PlayerPrefs.SetString(CONTROLLER_STICK_BEHAVIOUR_KEY, "StickPoints");
		}
		else if(whichScheme == "StickRotates")
		{
			leftGamepadStickBehaviour = LeftGamepadStickBehaviour.stickRotates;
			PlayerPrefs.SetString(CONTROLLER_STICK_BEHAVIOUR_KEY, "StickRotates");
		}
		PlayerPrefs.Save ();
	}
}
