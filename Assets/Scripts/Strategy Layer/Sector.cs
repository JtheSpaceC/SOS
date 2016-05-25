using UnityEngine;
using System.Collections;

public class Sector:MonoBehaviour{

	SpriteRenderer myRenderer;

	public enum MySectorGroup {Capital, Alpha, Beta, Gamma, Delta};
	public MySectorGroup mySectorGroup;

	public enum BaseType {None, Civilian, Enemy};
	public BaseType baseType;

	public bool enemyFleetPresent = false;
	public int dayExplored = -1;

	void Awake()
	{
		myRenderer = GetComponent<SpriteRenderer>();

		if(baseType == BaseType.Civilian)
			myRenderer.color = Color.blue;
		else if(baseType == BaseType.Enemy)
			myRenderer.color = Color.red;
	}

	public string OutPutMyStats()
	{
		return "";	
	}
}