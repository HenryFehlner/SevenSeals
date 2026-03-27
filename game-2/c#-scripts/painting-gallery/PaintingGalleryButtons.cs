using Godot;
using System;
using System.Collections.Generic;

public partial class PaintingGalleryButtons : Control
{
	private GridContainer paintingContainer;
	private List<string> paintingFilePaths;
	
	public override void _Ready()
	{
		// Get grid container
		paintingContainer = GetNode<GridContainer>("PaintingContainer");
		
		// Get photos
		//paintingFilePaths = GetPaintings("res://coloring-images");
		
		// Create a button for each photo
		GD.Print("Loading paintings...");
		foreach (PaintingStruct painting in GlobalData.PaintingList)
		{
			// Complete photo path
			string paintingPath = painting.ColoringImagePath;
			GD.Print("Painting image added: " + paintingPath);
			
			// Create button for image
			Vector2 buttonSize = new Vector2(GetViewport().GetVisibleRect().Size.X / 2.0f, GetViewport().GetVisibleRect().Size.X / 2.0f);
			Button button = new Button();
			button.CustomMinimumSize = buttonSize;
			
			// Load image from complete path
			//Image loadedImage = new Image();
			//loadedImage.Load(photoPath);
			//ImageTexture texture = ImageTexture.CreateFromImage(loadedImage);
			var texture = ResourceLoader.Load<Texture2D>(paintingPath);
			
			// Populate photoRect with loaded image
			TextureRect paintingRect = new TextureRect();
			paintingRect.Texture = texture;
			
			// Set texture rect properties
			paintingRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			paintingRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			
			paintingRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			paintingRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			paintingRect.Size = buttonSize;
			
			// Add texture to button as a child
			button.AddChild(paintingRect);
			
			// More button properties
			button.Flat = true;
			button.FocusMode = Control.FocusModeEnum.None;
			
			// Set signal
			button.Pressed += () => OnPaintingPressed(painting);
			
			// Add to scene
			paintingContainer.AddChild(button);
		}
	}
	
	private List<string> GetPaintings(string paintingFolderPath)
	{
		// Open folder path
		using DirAccess dir = DirAccess.Open(paintingFolderPath);
		List<string> paintings = new List<string>();
		
		if (dir != null)
		{
			dir.ListDirBegin();
			string filename = dir.GetNext();
			
			while (filename != "")
			{
				GD.Print(filename);
				if (!dir.CurrentIsDir() && filename.EndsWith(".png"))	// Ignore .import files
				{
					paintings.Add(filename);
				}
				
				filename = dir.GetNext();
			}
			
			dir.ListDirEnd();
		}
		
		return paintings;
	}
	
	private void OnPaintingPressed(PaintingStruct painting)
	{
		// Set global active photo
		GlobalData.ActivePainting = painting;
		GD.Print("New active painting: " + GlobalData.ActivePainting.PaintingID);
		
		// Change scene to color picker
		GetTree().ChangeSceneToFile("res://scenes/game_play.tscn");
	}
	
	public void _on_return_button_pressed()
	{
		GD.Print("Returning to main menu");
		// TODO: Link to main menu scene when it exists
		GetTree().ChangeSceneToFile("res://scenes/main-menu.tscn");
	}
}
