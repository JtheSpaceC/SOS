using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StaticTools: object {

	public static string SplitCamelCase(string input)
	{
		return Regex.Replace(input, "([A-Z])", " $1").Trim();
	}

	public static bool IsInLayerMask(GameObject obj, LayerMask mask)
	{
		return ((mask.value & (1 << obj.layer)) > 0);
	}

	public static T GetRandomElement<T>(this List<T> list)
	{
		return list[Random.Range(0, list.Count)];
	}
	public static T PopRandomElement<T>(this List<T> list)
	{
		object item = list[Random.Range(0, list.Count)];
		list.Remove((T) item);
		return (T) item;
	}
}

public class FadeTimes
{
	public float startTime;
	public float fadeTime;
}