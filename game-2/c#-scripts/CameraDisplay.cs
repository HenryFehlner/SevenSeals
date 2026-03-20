/*

using Godot;
using System;

public partial class CameraDisplay : Node
{
	
	[Export]public TextureRect display;
	private CameraFeed feed;

	public override void _Ready()
	{
		CameraServer.CameraFeedsUpdated += OnFeedsUpdated;
		CameraServer.MonitoringFeeds = true;
	}

	private void OnFeedsUpdated()
	{
		if (CameraServer.GetFeedCount() == 0)
		{
			GD.Print("No cameras detected");
			return;
		}

		feed = CameraServer.GetFeed(0);
		GD.Print("Using camera ID: " + feed.GetId());
	}

	public override void _Process(double delta)
	{
		if (feed == null)
			return;

		display.Texture = feed.Texture;
	}
	
	public void  startCamera(){
		feed.feed_is_active = true; 
	}
	
}
*/
