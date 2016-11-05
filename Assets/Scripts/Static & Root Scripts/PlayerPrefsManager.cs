using UnityEngine;

public class PlayerPrefsManager: MonoBehaviour {

	static bool doDebugs = false;

	const string CONTROLLER_VIBRATE_KEY = "Controller_Vibrate_Key";
	const string CONTROLLER_STICK_BEHAVIOUR_KEY = "Controller_Stick_Behaviour_Key";
	const string HINTS_ON_OFF_KEY = "Hints_On_Off_Key";
	const string CHARACTER_POOL_USAGE_KEY = "Character_Pool_Usage_Key";

	public enum LeftGamepadStickBehaviour {stickRotates, stickPoints};
	public static LeftGamepadStickBehaviour leftGamepadStickBehaviour;



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
		else
			Debug.LogError("Player Prefs Error");

		PlayerPrefs.Save ();
	}

	public static string GetHintsKey()
	{
		return PlayerPrefs.GetString(HINTS_ON_OFF_KEY, "On");
	}
	public static void SetHintsKey(string trueOrFalse)
	{
		if(trueOrFalse == "true")
		{
			PlayerPrefs.SetString(HINTS_ON_OFF_KEY, "On");
		}
		else if(trueOrFalse == "false")
		{
			PlayerPrefs.SetString(HINTS_ON_OFF_KEY, "Off");
		}
		else
			Debug.LogError("Player Prefs Error");

		PlayerPrefs.Save();
	}


	public static string GetCharacterPoolUsageKey()
	{
		return PlayerPrefs.GetString(CHARACTER_POOL_USAGE_KEY);
	}
	public static void SetCharacterPoolUsageKey(string whichMode)
	{
		if(whichMode == "Random & Character Pool" || whichMode == "Character Pool Only" || whichMode == "Random Only")
		{
			PlayerPrefs.SetString(CHARACTER_POOL_USAGE_KEY, whichMode);
		}
		else
			Debug.LogError("Player Prefs Error");
	}
}
