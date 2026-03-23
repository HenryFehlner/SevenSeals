using Godot;
using System;
using System.Collections.Generic;

public partial class GalleryButtons : Control
{
	private GridContainer photoContainer;
	private List<string> photoFilePaths;
	
	public override void _Ready()
	{
		// Get grid container
		photoContainer = GetNode<GridContainer>("PhotoContainer");
		
		// Get photos
		photoFilePaths = GetPhotos("res://user-photos");
		
		// Create a button for each photo
		GD.Print("Loading photos");
		foreach (string file in photoFilePaths)
		{
			// Complete photo path
			string photoPath = "res://user-photos/" + file;
			GD.Print("File loaded: " + photoPath);
			
			// Create button for image
			Vector2 buttonSize = new Vector2(GetViewport().GetVisibleRect().Size.X / 2.0f, GetViewport().GetVisibleRect().Size.X / 2.0f);
			Button button = new Button();
			button.CustomMinimumSize = buttonSize;
			
			// Load image from complete path
			//Image loadedImage = new Image();
			//loadedImage.Load(photoPath);
			//ImageTexture texture = ImageTexture.CreateFromImage(loadedImage);
			var texture = ResourceLoader.Load<Texture2D>(photoPath);
			
			// Populate photoRect with loaded image
			TextureRect photoRect = new TextureRect();
			photoRect.Texture = texture;
			
			// Set texture rect properties
			photoRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			photoRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			
			photoRect.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			photoRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
			photoRect.Size = buttonSize;
			
			// Add texture to button as a child
			button.AddChild(photoRect);
			
			// More button properties
			button.Flat = true;
			button.FocusMode = Control.FocusModeEnum.None;
			
			// Set signal
			button.Pressed += () => OnPhotoPressed(photoPath);
			
			// Add to scene
			photoContainer.AddChild(button);
		}
	}
	
	private List<string> GetPhotos(string photoFolderPath)
	{
		// Open folder path
		using DirAccess dir = DirAccess.Open(photoFolderPath);
		List<string> photos = new List<string>();
		
		if (dir != null)
		{
			dir.ListDirBegin();
			string filename = dir.GetNext();
			
			while (filename != "")
			{
				GD.Print(filename);
				if (!dir.CurrentIsDir() && filename.EndsWith(".png"))	// Ignore .import files
				{
					photos.Add(filename);
				}
				
				filename = dir.GetNext();
			}
			
			dir.ListDirEnd();
		}
		
		return photos;
	}
	
	private void OnPhotoPressed(string path)
	{
		// Set global active photo
		GlobalData.ActivePhoto = path;
		GD.Print("New active photo: " + GlobalData.ActivePhoto);
		
		// Change scene to color picker
		GetTree().ChangeSceneToFile("res://scenes/ImageColorPick.tscn");
	}
}
