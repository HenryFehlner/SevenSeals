using Godot;
using System;
using System.Collections.Generic;

public partial class GameNavigator : Node
{
	[Export] public string[] LevelPaths;
	[Export] public GameManager GameManager;
	public List<Level> allLevels;

	private LevelLoader loader;

	public override void _Ready()
	{
		loader = new LevelLoader();
		GD.Print(LevelPaths == null ? "LevelPaths NULL" : "LevelPaths OK");
		allLevels = LoadLevels();
		GameManager.StartLevel(allLevels[0]);
	}

	// Enter level information here
	public List<Level> LoadLevels()
	{
		List<Level> levels = new List<Level>();

		// Level 1
		Level level1 = new Level();
		level1.Setup(
			1,
			7, // three-star moves
			8, // two-star moves
			9, // one-star moves
			loader.LoadFromFile(LevelPaths[0])
		);
		levels.Add(level1);

		// Level 2
		Level level2 = new Level();
		level2.Setup(
			2,
			20,
			30,
			40,
			loader.LoadFromFile(LevelPaths[1])
		);
		levels.Add(level2);

		// Level 3
		Level level3 = new Level();
		level3.Setup(
			3,
			20,
			30,
			40,
			loader.LoadFromFile(LevelPaths[2])
		);
		levels.Add(level3);

		return levels;
	}
}
