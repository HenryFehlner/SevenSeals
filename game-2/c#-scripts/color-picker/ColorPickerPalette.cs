using Godot;
using System;
using System.Collections.Generic;

public partial class ColorPickerPalette : PanelContainer
{
	protected GridContainer paletteGridContainer;
	protected GridContainer checkmarkGridContainer;
	protected Texture2D checkTexture;
	protected PaintingStruct activePainting;
	
	public override void _Ready()
	{
		GD.Print("Loading color palette");
		
		// Get active painting
		activePainting = GlobalData.ActivePainting;
		GD.Print("Active painting: " + activePainting.PaintingID);
		
		// Init check texture
		checkTexture = ResourceLoader.Load<Texture2D>("res://IconImages/Checkmark.png");
		
		// Get color grid container node
		paletteGridContainer = GetNode<GridContainer>("PaletteGridContainer");
		checkmarkGridContainer = GetNode<GridContainer>("CheckmarkGridContainer");
		
		// Set number of columns to number of colors
		paletteGridContainer.Columns = activePainting.RequiredColors.Count;

		UpdateFoundColors();
	}
	
	public void UpdateFoundColors()
	{
		// Set active painting
		activePainting = GlobalData.ActivePainting;
		
		// Clear grids to refill them
		foreach (Node child in paletteGridContainer.GetChildren())
		{
			child.QueueFree();
		}
		foreach (Node child in checkmarkGridContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// Iterate through required palette and add a grid item to the ui
		foreach (KeyValuePair<Color, bool> kvp in activePainting.RequiredColors)
		{
			//GD.Print("Palette item: " + kvp);
			
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
			
			TextureRect emptyTextureRect = new TextureRect();
			emptyTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			emptyTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			emptyTextureRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			emptyTextureRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			
			// Overlay checkmark if color has been found
			if (kvp.Value)
			{
				checkmarkGridContainer.AddChild(checkTextureRect);
			}
			else
			{
				checkmarkGridContainer.AddChild(emptyTextureRect);
			}
			
			// Add to screen
			paletteGridContainer.AddChild(newColorRect);
		}
	}
}
