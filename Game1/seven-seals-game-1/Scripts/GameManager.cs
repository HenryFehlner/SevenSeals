using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	[Export] public Board Board;

	private Tile currentTile;
	
	private int moveCount;
	
	

	public override void _Ready()
	{
		foreach (Tile tile in Board.GetAllTiles())
	{
		tile.Clicked += OnTileClicked;
	}
	}
	
		public List<Tile> GetValidMoves()
	{
		List<Tile> validMoves = new List<Tile>();

		if (currentTile == null)
			return validMoves;

		foreach (HexDirection dir in System.Enum.GetValues(typeof(HexDirection)))
		{
			Tile neighbor = currentTile.GetNeighbor(dir);

			if (neighbor == null)
				continue;

			if (!neighbor.IsWalkable())
				continue;

			validMoves.Add(neighbor);
		}

		return validMoves;
	}
	//private dictionary
	public bool TryMove(HexDirection dir)
{
	if (currentTile == null)
		return false;

	Tile adjacent = currentTile.GetNeighbor(dir);
	if (adjacent == null)
		return false;

if (adjacent.Content == TileContent.Rock)
{
	return TryPushRock(adjacent, dir);
}

if (adjacent.IsWalkable())
{
	currentTile = adjacent;
	return true;
}

	return false;
}
private bool TryPushRock(Tile rockTile, HexDirection dir)
{
	Tile destination = rockTile.GetNeighbor(dir);

	if (destination == null)
		return false;

	switch (destination.Content)
	{
		case TileContent.Empty:
		case TileContent.Start:
			// Rock moves forward
			destination.SetContent(TileContent.Rock);
			rockTile.SetContent(TileContent.Empty);
			return true;

		case TileContent.Wall:
		case TileContent.End:
			// Rock breaks
			rockTile.SetContent(TileContent.Empty);
			return true;

		case TileContent.Rock:
		case TileContent.Invalid:
		default:
			return false;
	}
}

private Tile FindStartTile()
{
	if (Board == null)
	{
		GD.PrintErr("GameManager: Board reference not set");
		return null;
	}

	foreach (Tile tile in Board.GetAllTiles())
	{
		if (tile.Content == TileContent.Start)
		{
			GD.Print($"Start tile found at {tile.TileLocation}");
			return tile;
		}
	}

	GD.PrintErr("GameManager: No Start tile found");
	return null;
}

private void OnTileClicked(Tile tile)
{
	if (currentTile == null)
		return;

	// Determine which direction this tile is from current
	foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
	{
		if (currentTile.GetNeighbor(dir) == tile)
		{
			TryMove(dir);
			//UpdatePlayerVisual();
			return;
		}
	}
}

private void UpdateHighlights()
{
	// Clear all highlights
	foreach (Tile tile in Board.GetAllTiles())
	{
		tile.SetHighlight(false);
	}

	// Highlight valid moves
	foreach (Tile tile in GetValidMoves())
	{
		tile.SetHighlight(true);
	}
	
	PrintValidMoves(GetValidMoves());
}

public void StartLevel(Level level)
{
	moveCount = 0;
	if (Board == null)
	{
		GD.PrintErr("GameManager: Board reference not set");
		return;
	}

	if (level == null)
	{
		GD.PrintErr("GameManager: Level is null");
		return;
	}

	// Apply level data to board
	Board.ApplyLevel(level);

	// Find player start
	currentTile = FindStartTile();

	if (currentTile == null)
	{
		GD.PrintErr("GameManager: Start tile not found");
		return;
	}

	GD.Print($"Level {level.LevelNumber} started");
	UpdateHighlights();
}

private void PrintValidMoves(List<Tile> validMoves)
{
	if (validMoves.Count == 0)
	{
		GD.Print("Valid Moves: NONE");
		return;
	}

	GD.Print("Valid Moves:");

	foreach (Tile tile in validMoves)
	{
		GD.Print(" → Tile at ", tile.Position,
	" Content: ", tile.Content);
	}
	
	foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
{
	Tile n = currentTile.GetNeighbor(dir);
	if (n != null)
		GD.Print(dir, ": ", n.Content);
}
}

}
