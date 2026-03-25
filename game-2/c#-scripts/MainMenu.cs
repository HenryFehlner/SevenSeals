using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenu : Control
{
	private List<PaintingStruct> paintingObjects;
	
	public override void _Ready()
	{
		// Initialize list
		paintingObjects = new List<PaintingStruct>();
		
		// TODO: hardcode a bunch of PaintingStruct objects
		paintingObjects.Add(new PaintingStruct(
			"mareanie",
			"res://coloring-images/mareanie.png",
			new Dictionary<Color, bool> {
				{ new Color(0.39f, 0.676f, 0.704f, 1.0f), false },
				{ new Color(0.615f, 0.41f, 0.802f, 1.0f), false },
				{ new Color(0.881f, 0.819f, 0.404f, 1.0f), false }
			}
		));
		
		// Set global plainting list to initialized paintings
		GlobalData.PaintingList = paintingObjects;
		
		//GD.Print("Painting list: " + GlobalData.PaintingList);
	}
	
	public void _on_play_button_pressed()
	{
		GD.Print("Loading painting-gallery.tcsn");
		GetTree().ChangeSceneToFile("res://scenes/painting-gallery.tscn");
	}
}
