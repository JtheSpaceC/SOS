using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class ShipStats : ScriptableObject 
{
	/*[HideInInspector]*/ public List<Fighter> arrowFighters = new List<Fighter>();
	/*[HideInInspector] */public List<Fighter> mantisFighters = new List<Fighter>();


	public void ReorderFighterList(List<Fighter> whichList)
	{
		List<Fighter> craftWithLevels = new List<Fighter>();
		List<Fighter> specialCraft = new List<Fighter>();

		foreach(Fighter fighter in whichList)
		{
			if(fighter.specialShip == "")
			{
				craftWithLevels.Add(fighter);
			}
			else
			{
				specialCraft.Add(fighter);
			}
		}
		whichList.Clear();
		whichList.AddRange(craftWithLevels);

		for(int i = 0; i < whichList.Count; i++)
		{
			whichList[i].level = i+1;
		}
		whichList.AddRange(specialCraft);
	}
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

	public float dodgeSkillFront = 40;
	public float dodgeSkillSide = 20;
	public float dodgeSkillRear = 5;
	public float missileMultiplier = 0.5f;
	public float asteroidMultiplier = 10;

	public string specialShip = ""; //eg. "demo Arrow 1" to give special stats for a demo or story-based ship


	public static Fighter CopyFighter(Fighter fighterToCopy)
	{
		Fighter returnFighter = new Fighter();

		returnFighter.level = fighterToCopy.level;
		returnFighter.maxHealth = fighterToCopy.maxHealth;
		returnFighter.startingAwareness = fighterToCopy.startingAwareness;
		returnFighter.maxAwareness = fighterToCopy.maxAwareness;
		returnFighter.snapFocus = fighterToCopy.snapFocus;
		returnFighter.awarenessRecharge = fighterToCopy.awarenessRecharge;

		returnFighter.dodgeSkillFront = fighterToCopy.dodgeSkillFront;
		returnFighter.dodgeSkillSide = fighterToCopy.dodgeSkillSide;
		returnFighter.dodgeSkillRear = fighterToCopy.dodgeSkillRear;
		returnFighter.missileMultiplier = fighterToCopy.missileMultiplier;
		returnFighter.asteroidMultiplier = fighterToCopy.asteroidMultiplier;

		returnFighter.specialShip = fighterToCopy.specialShip;

		return returnFighter;
	}
}
