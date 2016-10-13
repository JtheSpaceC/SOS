using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class ShipStats : ScriptableObject {

	/*[HideInInspector]*/ public List<Fighter> allFighters = new List<Fighter>();
}

[System.Serializable]
public class Fighter
{
	public enum ShipType {Custom, Arrow, Mantis};
	public ShipType myShipType;

	public int level = 1;
	public int maxHealth = 2;
	public int startingAwareness = 0;
	public int maxAwareness = 2;
	public int snapFocus = 0;
	public float awarenessRecharge = 5;
}
