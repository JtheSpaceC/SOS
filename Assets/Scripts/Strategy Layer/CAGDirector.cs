using UnityEngine;
using System.Collections;

public class CAGDirector : MonoBehaviour {

	public static CAGDirector instance;

	public int gameDay = 1;
	[HideInInspector] public bool wearingClothes = false;

	public int currentRoom = 0;
	public Room[] rooms;
	public GameObject jumpsuit;


	public void Awake()
	{
		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("There were two CAGDirectors. Destroying one.");
			Destroy(gameObject);
		}

		ActivateNextRoom();
	}
		
	public void NewDay()
	{
		gameDay++;
		ResetJumpsuit();
		currentRoom = 0;
	}
	void ResetJumpsuit()
	{
		jumpsuit.GetComponent<SpriteRenderer>().enabled = true;
		jumpsuit.GetComponent<BoxCollider2D>().enabled = true;
		wearingClothes = false;
	}
	public void PutOnJumpsuit()
	{
		wearingClothes = true;
		jumpsuit.GetComponent<SpriteRenderer>().enabled = false;
		jumpsuit.GetComponent<BoxCollider2D>().enabled = false;
		jumpsuit.GetComponent<AudioSource>().Play();
	}


	public void ActivateNextRoom()
	{
		DeactivateRooms();
		rooms[currentRoom].gameObject.SetActive(true);
		rooms[currentRoom].myHeaderIcon.color = Color.green;

		currentRoom++;
	}
	void DeactivateRooms()
	{
		foreach(Room room in rooms)
		{
			room.gameObject.SetActive(false);
			room.myHeaderIcon.color = Color.white;
		}
		CAGManager.instance.contextualText.text = "";
	}

}
