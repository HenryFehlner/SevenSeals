using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
	[Export] public Board Board;

	private Tile currentTile;
	
	




	public override void _Ready()
	{
		// Example: start on a known tile
		currentTile = FindStartTile();
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

	// Normal movement
	if (adjacent.IsWalkable())
	{
		currentTile = adjacent;
		return true;
	}

	// Push interaction
	if (adjacent.Content == TileContent.Rock)
	{
		return TryPushRock(adjacent, dir);
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

}
