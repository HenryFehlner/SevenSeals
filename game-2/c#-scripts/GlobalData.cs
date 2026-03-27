using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalData : Node
{
	public static string ActivePhoto = "";
	public static string ActivePaintingId = "";
	public static List<Color> SavedColors = new List<Color>();
	public static Dictionary<Color, bool> CurrentRequiredPalette = new Dictionary<Color, bool>();

	public Dictionary<string, List<string>> ImagePhotos = new();

	public static List<PaintingStruct> PaintingList = new List<PaintingStruct>();
	public static PaintingStruct ActivePainting;

	public static GlobalData Instance;

	public override void _Ready()
	{
		Instance = this;

		// Only build default paintings if none exist yet
		if (PaintingList == null || PaintingList.Count == 0)
		{
			BuildDefaultPaintings();
		}

		// Make sure there is always an active painting
		if (ActivePainting == null && PaintingList.Count > 0)
		{
			SetActivePainting(PaintingList[0]);
		}
	}

	// --------------------------------------------------
	// DEFAULT PAINTINGS
	// --------------------------------------------------
	private void BuildDefaultPaintings()
	{
		PaintingList = new List<PaintingStruct>();

		// Pikachu
		var pikachuPalette = new Dictionary<Color, bool>()
		{
			{ new Color(1.0f, 0.84f, 0.0f, 1.0f), false },   // yellow
			{ new Color(0.55f, 0.27f, 0.07f, 1.0f), false }, // brown
			{ new Color(1.0f, 0.0f, 0.0f, 1.0f), false },    // red
			{ new Color(0.0f, 0.0f, 0.0f, 1.0f), false }     // black
		};

		var pikachu = new PaintingStruct(
			"pikachu",
			"res://coloring-images/pikachu2.png",
			pikachuPalette
		);

		// Mareanie
		var mareaniePalette = new Dictionary<Color, bool>()
		{
			{ new Color(0.6f, 0.2f, 0.8f, 1.0f), false },   // purple
			{ new Color(0.2f, 0.8f, 0.9f, 1.0f), false },   // cyan
			{ new Color(0.95f, 0.75f, 0.85f, 1.0f), false } // light pink
		};

		var mareanie = new PaintingStruct(
			"mareanie",
			"res://coloring-images/mareanie.png",
			mareaniePalette
		);

		// Sylveon
		var sylveonPalette = new Dictionary<Color, bool>()
		{
			{ new Color(1.0f, 0.75f, 0.85f, 1.0f), false }, // pink
			{ new Color(0.7f, 0.9f, 1.0f, 1.0f), false },   // light blue
			{ new Color(1.0f, 1.0f, 1.0f, 1.0f), false },   // white
			{ new Color(0.2f, 0.2f, 0.2f, 1.0f), false }    // dark gray
		};

		var sylveon = new PaintingStruct(
			"sylveon",
			"res://coloring-images/slyveon.png",
			sylveonPalette
		);

		PaintingList.Add(pikachu);
		PaintingList.Add(mareanie);
		PaintingList.Add(sylveon);
	}

	// --------------------------------------------------
	// ACTIVE PAINTING
	// --------------------------------------------------
	public void SetActivePainting(PaintingStruct painting)
	{
		if (painting == null)
			return;

		ActivePainting = painting;
		ActivePaintingId = painting.PaintingID;
		ActivePhoto = painting.ColoringImagePath;
		CurrentRequiredPalette = painting.RequiredColors;

		RefreshSavedColorsFromActivePainting();
	}

	public void SetActivePaintingById(string paintingId)
	{
		if (PaintingList == null)
			return;

		foreach (var painting in PaintingList)
		{
			if (painting.PaintingID == paintingId)
			{
				SetActivePainting(painting);
				return;
			}
		}

		GD.Print("Could not find painting with id: ", paintingId);
	}

	public PaintingStruct GetActivePainting()
	{
		return ActivePainting;
	}

	public List<PaintingStruct> GetPaintingList()
	{
		return PaintingList;
	}

	public string GetActivePaintingId()
	{
		return ActivePaintingId;
	}

	public string GetActivePhotoPath()
	{
		return ActivePhoto;
	}

	// --------------------------------------------------
	// PALETTE / FOUND COLORS
	// --------------------------------------------------
	public Godot.Collections.Array<Color> GetSavedColors()
	{
		return new Godot.Collections.Array<Color>(SavedColors);
	}

	public Godot.Collections.Dictionary GetRequiredPalette()
	{
		var dict = new Godot.Collections.Dictionary();

		if (CurrentRequiredPalette == null)
			return dict;

		foreach (var pair in CurrentRequiredPalette)
		{
			dict[pair.Key] = pair.Value;
		}

		return dict;
	}

	public void RefreshSavedColorsFromActivePainting()
	{
		SavedColors.Clear();

		if (ActivePainting == null || ActivePainting.RequiredColors == null)
			return;

		foreach (var pair in ActivePainting.RequiredColors)
		{
			if (pair.Value)
			{
				SavedColors.Add(pair.Key);
			}
		}
	}

	public void AddSavedColor(Color color)
	{
		if (!SavedColors.Contains(color))
		{
			SavedColors.Add(color);
		}
	}

	public void AddFoundColorToActivePainting(Color color)
	{
		if (ActivePainting == null || ActivePainting.RequiredColors == null)
			return;

		if (ActivePainting.RequiredColors.ContainsKey(color))
		{
			ActivePainting.RequiredColors[color] = true;
			CurrentRequiredPalette = ActivePainting.RequiredColors;
			RefreshSavedColorsFromActivePainting();
			GD.Print("Unlocked color for active painting: ", color);
		}
		else
		{
			GD.Print("Color not found in active painting palette: ", color);
		}
	}

	public bool PlayerHasColor(Color color)
	{
		if (ActivePainting == null || ActivePainting.RequiredColors == null)
			return false;

		if (!ActivePainting.RequiredColors.ContainsKey(color))
			return false;

		return ActivePainting.RequiredColors[color];
	}

	// --------------------------------------------------
	// IMAGE PHOTO STORAGE
	// --------------------------------------------------
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
