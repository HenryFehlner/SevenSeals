using Godot;
using System;
using System.Collections.Generic;

// Responsible for managing multiple levels and switching between them.
// This sits ABOVE the GameManager in control hierarchy.
//
// Duties:
// - Load level data from files
// - Construct Level objects
// - Store all levels in memory
// - Start selected levels
// - Allow keyboard-based level switching
public partial class GameNavigator : Node
{
	// Paths to level JSON files (set in Inspector)
	// Example: res://LevelData/level1.json
	[Export] public string[] LevelPaths;

	// Reference to the GameManager that runs gameplay
	[Export] public GameManager GameManager;

	// List containing all loaded Level objects
	public List<Level> allLevels;

	// Loader responsible for reading JSON files
	private LevelLoader loader;

	// Called when this node enters the scene tree
	public override void _Ready()
	{
		// Create loader instance
		loader = new LevelLoader();

		// Debug check: confirm LevelPaths array exists
		GD.Print(LevelPaths == null ? "LevelPaths NULL" : "LevelPaths OK");

		// Load all levels from files into memory
		allLevels = LoadLevels();

		// Start a default level immediately
		// (Currently starts Level 2 — index 1)
		GameManager.StartLevel(allLevels[0]);
	}

	// Creates Level objects using file data and metadata
	public List<Level> LoadLevels()
	{
		List<Level> levels = new List<Level>();

		// ─────────────────────────────
		// LEVEL 1
		// ─────────────────────────────
		Level level1 = new Level();

		// Setup assigns:
		// - Level number
		// - Star thresholds
		// - Tile layout from file
		level1.Setup(
			1,
			7, // Three-star move limit
			8, // Two-star move limit
			9, // One-star move limit
			loader.LoadFromFile(LevelPaths[0])
		);

		levels.Add(level1);

		// ─────────────────────────────
		// LEVEL 2
		// ─────────────────────────────
		Level level2 = new Level();

		level2.Setup(
			2,
			7,
			8,
			9,
			loader.LoadFromFile(LevelPaths[1])
		);

		levels.Add(level2);

		// ─────────────────────────────
		// LEVEL 3
		// ─────────────────────────────
		Level level3 = new Level();

		level3.Setup(
			3,
			7,
			8,
			9,
			loader.LoadFromFile(LevelPaths[2])
		);

		levels.Add(level3);
		
		// ─────────────────────────────
		// LEVEL 4
		// ─────────────────────────────
		Level level4 = new Level();

		level4.Setup(
			3,
			8,
			9,
			10,
			loader.LoadFromFile(LevelPaths[3])
		);

		levels.Add(level4);
		
		// ─────────────────────────────
		// LEVEL 5
		// ─────────────────────────────
		Level level5 = new Level();

		level5.Setup(
			3,
			8,
			10,
			12,
			loader.LoadFromFile(LevelPaths[4])
		);

		levels.Add(level5);

		// Return fully constructed level list
		return levels;
	}
	
	// Handles keyboard input globally
	// Allows quick switching between levels using keys 1–7
	public override void _Input(InputEvent @event)
	{
		// Only process key presses (ignore releases and repeats)
		if (@event is InputEventKey key &&
			key.Pressed &&
			!key.Echo)
		{
			int index = -1;

			// Map number keys to level indices
			switch (key.Keycode)
			{
				case Key.Key1: index = 0; break;
				case Key.Key2: index = 1; break;
				case Key.Key3: index = 2; break;
				case Key.Key4: index = 3; break;
				case Key.Key5: index = 4; break;
				case Key.Key6: index = 5; break;
				case Key.Key7: index = 6; break;
			}

			// Ensure index is valid before starting level
			if (index >= 0 && index < allLevels.Count)
			{
				GD.Print($"Starting Level {index + 1}");

				// Tell GameManager to run the selected level
				GameManager.StartLevel(allLevels[index]);
			}
		}
	}
}
