using Godot;
using System;
using System.Collections.Generic;

public class Level
{
	public int LevelNumber { get; private set; }

	public int ThreeStarMoves { get; private set; }
	public int TwoStarMoves   { get; private set; }
	public int OneStarMoves   { get; private set; }

	public Dictionary<Vector2I, TileContent> TileMap { get; private set; }

	public void Setup(
		int levelNumber,
		int threeStarMoves,
		int twoStarMoves,
		int oneStarMoves,
		Dictionary<Vector2I, TileContent> tileMap
	)
	{
		LevelNumber     = levelNumber;
		ThreeStarMoves  = threeStarMoves;
		TwoStarMoves    = twoStarMoves;
		OneStarMoves    = oneStarMoves;
		TileMap         = tileMap;
	}
}
