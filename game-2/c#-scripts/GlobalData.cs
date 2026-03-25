using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalData : Node
{
	public static string ActivePhoto;
	public static List<Color> SavedColors = new List<Color>();
	public static Dictionary<Color, bool> CurrentRequiredPalette = new Dictionary<Color, bool>();
	public static PaintingStruct ActivePainting;

	// Instance wrapper for GDScript
	public Godot.Collections.Array<Color> GetSavedColors()
	{
		return new Godot.Collections.Array<Color>(SavedColors);
	}

	public void AddSavedColor(Color color)
	{
		SavedColors.Add(color);
	}
	
	//public Godot.Collections.Dictionary<Color, bool> GetRequiredPalette()
	//{
		//return new Godot.Collections.Dictionary<Color, bool>(CurrentRequiredPalette);
	//}
	
	public PaintingStruct GetActivePainting()
	{
		return ActivePainting;
	}
}
