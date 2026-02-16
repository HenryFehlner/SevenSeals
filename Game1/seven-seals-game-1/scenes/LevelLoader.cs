using Godot;
using System;

public partial class LevelLoader : Node2D
{
	string levelDataFromJson;
	
	public override void _Ready()
	{
		GD.Print("Building Level");
		
		// Get data file
		Godot.Collections.Dictionary jsonData = LoadJsonFile("res://LevelData/testLevel2.json");
		
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
			
			// Print data for debugging
			GD.Print(tileType + " " + tilePosX + ", " + tilePosY);
			if (i % 7 == 6) { GD.Print(""); }	// split by row
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
}
