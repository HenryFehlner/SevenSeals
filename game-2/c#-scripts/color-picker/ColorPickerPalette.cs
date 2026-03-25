using Godot;
using System;
using System.Collections.Generic;

public partial class ColorPickerPalette : PanelContainer
{
	protected GridContainer paletteGridContainer;
	
	public override void _Ready()
	{
		GD.Print("Loading color palette");
		
		// Get color grid container node
		paletteGridContainer = GetNode<GridContainer>("PaletteGridContainer");
		
		// Get active painting
		PaintingStruct activePainting = GlobalData.ActivePainting;
		GD.Print("Active painting: " + activePainting);
		
		// Set number of columns to number of colors
		paletteGridContainer.Columns = activePainting.RequiredColors.Count;
		
		// Iterate through required palette and add a grid item to the ui
		foreach (KeyValuePair<Color, bool> kvp in activePainting.RequiredColors)
		{
			GD.Print("Palette item: " + kvp);
			
			// Create new ColorRect
			ColorRect newColorRect = new ColorRect();
			newColorRect.Color = kvp.Key;
			newColorRect.CustomMinimumSize = new Vector2(720 / paletteGridContainer.Columns, 300);
			
			// Add to screen
			paletteGridContainer.AddChild(newColorRect);
		}
	}
}
