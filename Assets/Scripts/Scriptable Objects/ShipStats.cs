using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class ShipStats : ScriptableObject 
{
	/*[HideInInspector]*/ public List<Fighter> arrowFighters = new List<Fighter>();
	/*[HideInInspector] */public List<Fighter> mantisFighters = new List<Fighter>();
}

[System.Serializable]
public class Fighter
{
	public int level = 1;
	public int maxHealth = 2;
	public int startingAwareness = 0;
	public int maxAwareness = 2;
	public int snapFocus = 0;
	public float awarenessRecharge = 5;
	public string specialShip = ""; //eg. "demo Arrow 1" to give special stats for a demo or story-based ship
}
