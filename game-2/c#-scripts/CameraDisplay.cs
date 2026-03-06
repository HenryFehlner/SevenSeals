using Godot;
using System;

public partial class CameraDisplay : Node
{
	/*
	private TextureRect display;
	private CameraFeed feed;

	public override void _Ready()
	{
		display = GetNode<TextureRect>("TextureRect");

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
	*/
}
