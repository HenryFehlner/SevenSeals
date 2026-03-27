using Godot;
using System;
using System.Collections.Generic;

enum CameraState
{
	Idle,
	Preview
}



public partial class CameraDisplay : Sprite2D
{
	[Export] public string CameraName = "2 | BACK";
	
	[Export] public TextureButton PhotoButton;
	
	[Export] public TextureButton AcceptButton;
	
	[Export] public TextureButton RejectButton;
	
	[Export] public SubViewport subViewport;
	
	private CameraTexture camTex;

	private CameraFeed camera;
	
	CameraState currentState = CameraState.Idle;
	Image capturedImage = null;

	public override async void _Ready()
{
	GD.Print("START _Ready");

	CameraServer.MonitoringFeeds = true;
	GD.Print("Monitoring enabled");

	await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	GD.Print("Waited a frame");

	GD.Print("Getting feeds...");
	var feeds = CameraServer.Feeds();

	if (feeds == null)
	{
		GD.PrintErr("Feeds is NULL!");
		return;
	}

	GD.Print($"Feeds count: {feeds.Count}");

	foreach (CameraFeed feed in feeds)
	{
		if (feed == null)
		{
			GD.PrintErr("Feed is NULL!");
			continue;
		}

		string name = feed.GetName();
		GD.Print("Feed name: ", name);

		if (camera == null && (CameraName == "" || name == CameraName))
		{
			camera = feed;
		}
	}

	if (camera == null)
	{
		GD.PrintErr("Camera is NULL!");
		return;
	}

	GD.Print("Camera selected: ", camera.GetName());

	var formats = camera.GetFormats();
	GD.Print("Formats count: ", formats.Count);

int selected = 0;

for (int i = 0; i < formats.Count; i++)
{
	var f = formats[i];

	// You may need to inspect properties depending on Godot version
	if (f.ToString().Contains("1280") || f.ToString().Contains("1920"))
	{
		selected = i;
		break;
	}
}

camera.SetFormat(selected, new Godot.Collections.Dictionary());


	GD.Print("Format set");

	camera.FeedIsActive = true;
	GD.Print("Camera activated");

	var mat = Material as ShaderMaterial;
	GD.Print("Material: ", mat);

	if (mat == null)
	{
		GD.PrintErr("Material is NULL!");
		return;
	}

	// 🔥 Create textures instead of reading them
CameraTexture camTexY = new CameraTexture();
CameraTexture camTexCbCr = new CameraTexture();

camTexY.CameraFeedId = camera.GetId();
camTexCbCr.CameraFeedId = camera.GetId();

camTex = new CameraTexture();
camTex.CameraFeedId = camera.GetId();

// Assign to shader
mat.SetShaderParameter("camera_y", camTexY);
mat.SetShaderParameter("camera_CbCr", camTexCbCr);

GD.Print("Camera textures created and assigned");
GD.Print("NEW BUILD CONFIRMED");

	GD.Print("DONE");
	
	if (PhotoButton != null)
{
	PhotoButton.Pressed += TakePhoto;
	AcceptButton.Pressed += AcceptPhoto;
	RejectButton.Pressed += RejectPhoto;
	UpdateUI();
		GD.Print("ButtonLinked");
}
}
	
public async void TakePhoto()
{
	if (currentState != CameraState.Idle)
		return;

	await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

	var img = subViewport.GetTexture().GetImage();

	if (img == null)
	{
		GD.PrintErr("Failed to capture SubViewport!");
		return;
	}

	img.FlipY();

	capturedImage = img;
	currentState = CameraState.Preview;

	GD.Print("Photo captured, now in PREVIEW state");
	UpdateUI();

	// Optional: pause camera
	if (camera != null)
		camera.FeedIsActive = false;
}

public void AcceptPhoto()
{
	if (capturedImage == null)
		return;

	string imageId = "image_001"; // TODO: replace with your actual image ID
	string dirPath = $"user://photos/{imageId}/";

	DirAccess.MakeDirRecursiveAbsolute(dirPath);

	string fileName = $"shot_{Time.GetUnixTimeFromSystem()}.png";
	string fullPath = dirPath + fileName;

	var err = capturedImage.SavePng(fullPath);

	if (err != Error.Ok)
	{
		GD.PrintErr("Failed to save image!");
	}
	else
	{
		GD.Print("Saved photo: ", fullPath);
	}
		GD.Print("Saved to: ", fullPath);
		GD.Print(ProjectSettings.GlobalizePath(fullPath));
		GlobalData.AddPhoto(imageId,fileName);
		
		ResetToIdle();
	}
	
public void RejectPhoto()
{
	GD.Print("Photo rejected");

	capturedImage = null;

	ResetToIdle();
}

void ResetToIdle()
{
	currentState = CameraState.Idle;
	UpdateUI();
		
	if (camera != null)
		camera.FeedIsActive = true;
}
	
	void UpdateUI()
{
	bool isIdle = currentState == CameraState.Idle;
	bool isPreview = currentState == CameraState.Preview;

	if (PhotoButton != null)
		PhotoButton.Visible = isIdle;

	if (AcceptButton != null)
		AcceptButton.Visible = isPreview;

	if (RejectButton != null)
		RejectButton.Visible = isPreview;
}
	
}
