using Godot;
using System;
using System.Collections.Generic;

public partial class ImageGalleryButtons : Control
{
	private GridContainer photoContainer;
	private List<string> photoFilePaths;
	
	public override void _Ready()
	{
		// Get grid container
		photoContainer = GetNode<GridContainer>("PhotoContainer");
		
		
		
		// Get photos
		string imageId = GlobalData.ActivePainting.PaintingID;
		//string folderPath = $"user://photos/{imageId}/";

		//photoFilePaths = GetPhotos(folderPath);
		//photoFilePaths = GetPhotos("res://user-photos");
		
		GD.Print("Active Painting ID: " + imageId);

		if (GlobalData.Instance.ImagePhotos.ContainsKey(imageId))
		{
			GD.Print("Photo count: " + GlobalData.Instance.ImagePhotos[imageId].Count);
			
			foreach (string path in GlobalData.Instance.ImagePhotos[imageId])
			{
				GD.Print("Photo path: " + path);
			}
		}
		else
		{
			GD.Print("No entry in ImagePhotos for this painting");
		}
		
		
		
		if (GlobalData.Instance.ImagePhotos.ContainsKey(imageId))
		{
			photoFilePaths = GlobalData.Instance.ImagePhotos[imageId];
		}
		else
		{
			GD.Print("No photos found for: " + imageId);
			photoFilePaths = new List<string>();
		}
		
		
		
		
		
		// Create a button for each photo
		GD.Print("Loading photos");
		foreach (string file in photoFilePaths)
		{
			// Complete photo path
			//string photoPath = "res://user-photos/" + file;
			string photoPath = file;
			GD.Print("File loaded: " + photoPath);
			
			// Create button for image
			Vector2 buttonSize = new Vector2(GetViewport().GetVisibleRect().Size.X / 2.0f, GetViewport().GetVisibleRect().Size.X / 2.0f);
			Button button = new Button();
			button.CustomMinimumSize = buttonSize;
			
			// Load image from complete path
			//Image loadedImage = new Image();
			//loadedImage.Load(photoPath);
			//ImageTexture texture = ImageTexture.CreateFromImage(loadedImage);
			//var texture = ResourceLoader.Load<Texture2D>(photoPath);
			
			Image image = new Image();
			Error err = image.Load(photoPath);

			if (err != Error.Ok)
			{
				GD.PrintErr("Failed to load image: " + photoPath);
				continue;
			}

			ImageTexture texture = ImageTexture.CreateFromImage(image);
			
			
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
		GetTree().ChangeSceneToFile("res://scenes/image-color-pick.tscn");
	}
	
	public void _on_return_button_pressed()
	{
		GD.Print("Returning to painting scene");
		GetTree().ChangeSceneToFile("res://scenes/game_play.tscn");
	}
}
