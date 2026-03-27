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
			"Mareanie",
			"res://coloring-images/mareanie.png",
			new Dictionary<Color, bool> {
				{ new Color(0.39f, 0.676f, 0.704f, 1.0f), false },
				{ new Color(0.615f, 0.41f, 0.802f, 1.0f), false },
				{ new Color(0.881f, 0.819f, 0.404f, 1.0f), false }
			}
		));
		paintingObjects.Add(new PaintingStruct(
			"Pikachu",
			"res://coloring-images/pikachu2.png",
			new Dictionary<Color, bool> {
				{ new Color(0.95f, 0.86f, 0.181f, 1.0f), false },
				{ new Color(1.0f, 0.08f, 0.095f, 1.0f), false }
			}
		));
		paintingObjects.Add(new PaintingStruct(
			"Sylveon",
			"res://coloring-images/slyveon.png",		// This is spelled wrong lol, not gonna change it
			new Dictionary<Color, bool> {
				{ new Color(1.0f, 1.0f, 1.0f, 1.0f), false },
				{ new Color(1.0f, 0.55f, 0.663f, 1.0f), false },
				{ new Color(0.418f, 0.773f, 0.95f, 1.0f), false },
				{ new Color(0.109f, 0.49f, 0.68f, 1.0f), false }
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
