using Godot;
using System;
using System.Collections.Generic;


// If there is any way to make this return a dictionary 
// mapping a vector2 to the tile contents, then 
// I can easily use that later 
public partial class LevelLoader : Node2D
{
	private string levelDataFromJson;
	[Export] private PackedScene tileScene;
	
	// Tile details for conversion to world coords
	[Export] private float hexSideLength;
	
	// Tile textures
	[Export] Texture2D emptyTexture;
	[Export] Texture2D rockTexture;
	[Export] Texture2D wallTexture;
	[Export] Texture2D startTexture;
	[Export] Texture2D endTexture;
	[Export] Texture2D invalidTexture;
	
	public override void _Ready()
	{
		GD.Print("Building Level");
		
		// Get data file
		Godot.Collections.Dictionary jsonData = LoadJsonFile("res://LevelData/level1.json");
		
		// Get the size of the level
		Godot.Collections.Dictionary boundsData = jsonData["bounds"].AsGodotDictionary();
		int xSize = (int)boundsData["max"].AsGodotDictionary()["x"];
		int ySize = (int)boundsData["max"].AsGodotDictionary()["y"];
		int tileCount = xSize * ySize;
		
		// Loop through the tiles and get their data
		Godot.Collections.Array tilesData = jsonData["tiles"].AsGodotArray();
		for (int i = 0; i < tileCount; ++i)
		{
			// Get each individual tile
			Godot.Collections.Dictionary tile = tilesData[i].AsGodotDictionary();
			
			// Get tile type
			string tileType = (string)tile["tileId"];
			
			// Get tile position
			int tilePosX = (int)tile["pos"].AsGodotDictionary()["x"];
			int tilePosY = (int)tile["pos"].AsGodotDictionary()["y"];
			
			// Convert from axial to world coordinates
			Vector2 worldCoords = AxialToWorldCoords(tilePosX, tilePosY);
			
			// Print data for debugging
			GD.Print(tileType + " " + tilePosX + ", " + tilePosY);
			//GD.Print(worldCoords);
			//if (i % 7 == 6) { GD.Print(""); }	// split by row
			
			// Instantiate tiles
			Node newTile = tileScene.Instantiate();
			Tile tileScript = newTile as Tile;
			
			// Set tile properties
			TileContent content = TileContent.Invalid;
			Texture2D tileTexture = invalidTexture;
			switch (tileType)
			{
				case "01U1":
					content = TileContent.Empty;
					tileTexture = emptyTexture;
					break;
				case "M1C4":
					content = TileContent.Rock;
					tileTexture = rockTexture;
					break;
				case "02U1":
					content = TileContent.Wall;
					tileTexture = wallTexture;
					break;
				case "PR06":
					content = TileContent.Start;
					tileTexture = startTexture;
					break;
				case "M1S4":
					content = TileContent.End;
					tileTexture = endTexture;
					break;
				default:
					content = TileContent.Invalid;
					tileTexture = invalidTexture;
					break;
			}
			tileScript.Setup(worldCoords, content, tileTexture);
			
			AddChild(newTile);
			
			/*
			Empty,	// 01U1
			Rock,	// M1C4
			Wall,	// 02U1
			Start,	// PR06
			End,	// M1S4
			Invalid	// 03U1*/
			
			if (i % 7 == 6) { GD.Print(""); }	// split by row for console logging
		}
	}
	
	private Godot.Collections.Dictionary LoadJsonFile(string filePath)
	{
		// Check file exists
		if (!FileAccess.FileExists(filePath))
		{
			GD.PrintErr($"{filePath} not found");
			return null;
		}
		
		// Read the file
		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonText = file.GetAsText();
		file.Close();
		
		// Parse data
		Variant parsedResult = Json.ParseString(jsonText);
		Godot.Collections.Dictionary resultDict = parsedResult.AsGodotDictionary();
		return resultDict;
	}
	
	// this math is WEIRD
	private Vector2 AxialToWorldCoords(int q, int r)
	{
		float widthOffset = Mathf.Sqrt(3.0f) * hexSideLength;
		float xPos = -(widthOffset * ((r / 2.0f) + q));
		float yPos = (3.0f / 2.0f) * hexSideLength * r;
		
		if (r % 2 == 0)	// Offset every other row because everything is stupid
		{
			xPos = -widthOffset * q;
		}
		else
		{
			xPos = -widthOffset * q - (widthOffset / 2.0f);
		}
		
		return new Vector2(xPos, yPos);
	}
}
