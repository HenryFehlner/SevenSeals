using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalData : Node
{
	public static string ActivePhoto;
	public static List<Color> SavedColors = new List<Color>();

	// Instance wrapper for GDScript
	public Godot.Collections.Array<Color> GetSavedColors()
	{
		return new Godot.Collections.Array<Color>(SavedColors);
	}

	public void AddSavedColor(Color color)
	{
		SavedColors.Add(color);
	}
}
