using UnityEngine;
using System.Collections;

public class SpriteWhitener : MonoBehaviour {

	[Tooltip("If true, the hotkey will toggle the sprite whitener on/off")]
	public bool debugMode = false;
	public KeyCode hotkey;

	private SpriteRenderer myRenderer;
	private Shader shaderGUItext;
	private Shader shaderSpritesDefault;
	private Color originalColor;


	void Start () 
	{
		myRenderer = gameObject.GetComponent<SpriteRenderer>();
		shaderGUItext = Shader.Find("GUI/Text Shader");
		shaderSpritesDefault = Shader.Find("Sprites/Default"); // or whatever sprite shader is being used
		originalColor = myRenderer.color;
	}

	void Update()
	{
		if(debugMode && Input.GetKeyDown(hotkey))
		{
			if(myRenderer.material.shader == shaderSpritesDefault)
				WhitenSprite();
			else
				NormalizeSprite();
		}
	}

	//To set the sprite to white:
	public void WhitenSprite() 
	{
		myRenderer.material.shader = shaderGUItext;
		myRenderer.color = Color.white;
	}

	//And then to set the sprite back to normal:
	public void NormalizeSprite() 
	{
		myRenderer.material.shader = shaderSpritesDefault;
		myRenderer.color = originalColor;
	}
}