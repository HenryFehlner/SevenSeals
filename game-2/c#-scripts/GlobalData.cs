using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalData : Node
{
	public static string ActivePhoto;
	public static string ActivePaintingId;
	public static List<Color> SavedColors = new List<Color>();
	public static Dictionary<Color, bool> CurrentRequiredPalette = new Dictionary<Color, bool>();
	public Dictionary<string, List<string>> ImagePhotos = new();
	
	public static List<PaintingStruct> PaintingList;
	public static PaintingStruct ActivePainting;


	public static GlobalData Instance;

	public override void _Ready()
	{
		Instance = this;
	}


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
	
	public List<PaintingStruct> GetPaintingList()
	{
		return PaintingList;
	}
	
	public PaintingStruct GetActivePainting()
	{
		return ActivePainting;
	}
	
	public static Dictionary<string, List<string>> ImagePhotosStatic
{
	get => Instance.ImagePhotos;
}
	
	public static void AddPhoto(string paintingId, string fileName)
{
	if (!Instance.ImagePhotos.ContainsKey(paintingId))
	{
		Instance.ImagePhotos[paintingId] = new List<string>();
	}
	
	Instance.ImagePhotos[paintingId].Add(fileName);
	Instance.PrintImagePhotos();
	
	
	}
	
public void PrintImagePhotos()
	{
	GD.Print("=== ImagePhotos Contents ===");

	foreach (var pair in ImagePhotos)
	{
		string paintingId = pair.Key;
		List<string> photos = pair.Value;

		GD.Print($"Image: {paintingId}");

		foreach (string photo in photos)
		{
			GD.Print("  - ", photo);
		}
	}
}
	
}
