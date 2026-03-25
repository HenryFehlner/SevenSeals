using Godot;
using System;
using System.Collections.Generic;

public partial class ColorPickerPalette : PanelContainer
{
	protected GridContainer paletteGridContainer;
	protected GridContainer checkmarkGridContainer;
	protected Texture2D checkTexture;
	
	public override void _Ready()
	{
		GD.Print("Loading color palette");
		
		// Get active painting
		PaintingStruct activePainting = GlobalData.ActivePainting;
		GD.Print("Active painting: " + activePainting.PaintingID);
		
		// Init check texture
		checkTexture = ResourceLoader.Load<Texture2D>("res://IconImages/Checkmark.png");
		
		// Get color grid container node
		paletteGridContainer = GetNode<GridContainer>("PaletteGridContainer");
		checkmarkGridContainer = GetNode<GridContainer>("CheckmarkGridContainer");
		
		// Set number of columns to number of colors
		paletteGridContainer.Columns = activePainting.RequiredColors.Count;
		
		// Iterate through required palette and add a grid item to the ui
		foreach (KeyValuePair<Color, bool> kvp in activePainting.RequiredColors)
		{
			GD.Print("Palette item: " + kvp);
			
			// Create new ColorRect
			ColorRect newColorRect = new ColorRect();
			newColorRect.Color = kvp.Key;
			newColorRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			newColorRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			
			// Create new TextureRect (for check)
			TextureRect checkTextureRect = new TextureRect();
			checkTextureRect.Texture = checkTexture;
			checkTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			checkTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			checkTextureRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			checkTextureRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			
			// Overlay checkmark if color has been found
			if (kvp.Value)
			{
				checkmarkGridContainer.AddChild(checkTextureRect);
			}
			
			// Add to screen
			paletteGridContainer.AddChild(newColorRect);
		}
	}
}
