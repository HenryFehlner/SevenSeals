using Godot;
using System;
using System.Collections.Generic;

// ─────────────────────────────────────────────
// LEVEL DATA CLASS
// ─────────────────────────────────────────────
// This class represents ONE playable level.
//
// It does NOT control gameplay.
// It does NOT draw anything.
// It ONLY stores configuration data.
//
// Think of it as a "level blueprint" that the
// Board and GameManager use to set up the game state.
public class Level
{
	// Unique identifier for the level
	// Useful for UI, save data, progression, etc.
	public int LevelNumber { get; private set; }

	// Move thresholds for star ratings
	// Lower move counts = better performance
	public int ThreeStarMoves { get; private set; }
	public int TwoStarMoves   { get; private set; }
	public int OneStarMoves   { get; private set; }

	// Mapping of board coordinates → tile content
	//
	// This defines the actual layout of the level:
	// - Where walls are
	// - Where rocks start
	// - Player start position
	// - Goal tile
	// - Playable vs invalid area
	//
	// Vector2I = grid coordinate (x, y)
	// TileContent = what exists at that coordinate
	public Dictionary<Vector2I, TileContent> TileMap { get; private set; }

	// ─────────────────────────────
	// INITIALIZATION METHOD
	// ─────────────────────────────
	// Populates the level with all necessary data.
	
	public void Setup(
		int levelNumber,
		int threeStarMoves,
		int twoStarMoves,
		int oneStarMoves,
		Dictionary<Vector2I, TileContent> tileMap
	)
	{
		// Store level identity
		LevelNumber     = levelNumber;

		// Store star thresholds
		ThreeStarMoves  = threeStarMoves;
		TwoStarMoves    = twoStarMoves;
		OneStarMoves    = oneStarMoves;

		// Store layout data
		TileMap         = tileMap;
	}
}
