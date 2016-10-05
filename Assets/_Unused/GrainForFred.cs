using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GrainTest : MonoBehaviour {

	public Canvas playerUICanvas;
	public Slider alphaSlider;
	public Slider redSlider;
	public Slider greenSlider;
	public Slider blueSlider;
	public SpriteRenderer grainImage;
	public GradientColorKey colorKey;

	void Start()
	{
		grainImage.enabled = true;
		redSlider.value = grainImage.color.r;
		greenSlider.value = grainImage.color.g;
		blueSlider.value = grainImage.color.b;
		alphaSlider.value = grainImage.color.a;
	}

	public void ToggleUIGrain()
	{
		if(playerUICanvas.renderMode == RenderMode.ScreenSpaceCamera)
			playerUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		else if(playerUICanvas.renderMode == RenderMode.ScreenSpaceOverlay)
			playerUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
	}

	public void UpdateColour()
	{
		Color grainColour = grainImage.color;
		grainColour.r = redSlider.value;
		grainColour.g = greenSlider.value;
		grainColour.b = blueSlider.value;
		grainColour.a = alphaSlider.value;
		grainImage.color = grainColour;
	}
	
}
