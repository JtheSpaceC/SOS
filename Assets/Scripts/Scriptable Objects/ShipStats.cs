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
}
