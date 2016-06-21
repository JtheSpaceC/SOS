using UnityEngine;
using System.Text.RegularExpressions;

public class StaticTools: MonoBehaviour {

	public static string SplitCamelCase(string input)
	{
		return Regex.Replace(input, "([A-Z])", " $1").Trim();
	}

	public static bool IsInLayerMask(GameObject obj, LayerMask mask)
	{
		return ((mask.value & (1 << obj.layer)) > 0);
	}
}
