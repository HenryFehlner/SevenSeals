using Godot;
using System;
using System.Collections.Generic;

public partial class ColorPickerPalette : PanelContainer
{
	protected GridContainer paletteGridContainer;
	
	public override void _Ready()
	{
		GD.Print("Loading color palette");
		GlobalData.CurrentRequiredPalette.Add(new Color(0.885f, 0.023f, 0.332f, 1.0f), false);
		
		// Get color grid container node
		paletteGridContainer = GetNode<GridContainer>("PaletteGridContainer");
		
		// Iterate through required palette and add a grid item to the ui
		foreach (KeyValuePair<Color, bool> kvp in GlobalData.CurrentRequiredPalette)
		{
			GD.Print("Palette item: " + kvp);
			
			// Create new ColorRect
			ColorRect newColorRect = new ColorRect();
			newColorRect.Color = kvp.Key;
			
			// Add to screen
			paletteGridContainer.AddChild(newColorRect);
		}
	}
}
