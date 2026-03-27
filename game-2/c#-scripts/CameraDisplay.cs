using Godot;
using System;

public partial class CameraDisplay : Sprite2D
{
	[Export] public string CameraName = "";
	
	[Export] public TextureButton PhotoButton;

	private CameraFeed camera;

	public override void _Ready()
	{
		CameraServer.MonitoringFeeds = true;
		
		GD.Print("cameras:");

		foreach (CameraFeed feed in CameraServer.Feeds())
		{
			string name = feed.GetName();
			GD.Print(name);

			if (camera == null && (CameraName == "" || name == CameraName))
			{
				camera = feed;
			}
		}

		if (camera == null)
		{
			GD.Print("no matching camera");
			return;
		}

		GD.Print($"using camera {camera} ({camera.GetName()})");

		camera.FeedIsActive = true;

		var mat = Material as ShaderMaterial;
		if (mat == null)
		{
			GD.PrintErr("Material is not a ShaderMaterial!");
			return;
		}

		
		CameraTexture camTexY = (CameraTexture)mat.GetShaderParameter("camera_y");
		CameraTexture camTexCbCr = (CameraTexture)mat.GetShaderParameter("camera_CbCr");

		camTexY.CameraFeedId = camera.GetId();
		camTexCbCr.CameraFeedId = camera.GetId();

		mat.SetShaderParameter("camera_y", camTexY);
		mat.SetShaderParameter("camera_CbCr", camTexCbCr);
	}
	
	public void TakePhoto(){
		
	}
	
}
