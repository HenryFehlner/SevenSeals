using Godot;
using System;
using System.Collections.Generic;

public partial class Board : Node
{
	[Export] public PackedScene TileScene;

	private const int Width = 7;
	private const int Height = 7;

	private Dictionary<Vector2I, Tile> Tiles;

	public override void _Ready()
	{
		GenerateTiles();
		LinkTiles();
	}
	
	public IEnumerable<Tile> GetAllTiles()
	{
		return Tiles.Values;
	}

	private void GenerateTiles()
	{
		Tiles = new Dictionary<Vector2I, Tile>();

		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				Vector2I coord = new Vector2I(x, y);

				Tile tile = TileScene.Instantiate<Tile>();
				AddChild(tile);

				//tile.Setup(x, y, TileContent.Empty);
				Tiles[coord] = tile;
			}
		}
	}

	private void LinkTiles()
	{
		foreach (var pair in Tiles)
		{
			Vector2I c = pair.Key;
			Tile tile = pair.Value;

			// Top
			TrySetNeighbor(tile, HexDirection.UpLeft,     c.X - 1, c.Y - 1);
			TrySetNeighbor(tile, HexDirection.UpRight,    c.X + 1, c.Y - 1);

			// Middle
			TrySetNeighbor(tile, HexDirection.Left,       c.X - 1, c.Y);
			TrySetNeighbor(tile, HexDirection.Right,      c.X + 1, c.Y);

			// Bottom
			TrySetNeighbor(tile, HexDirection.DownLeft,   c.X - 1, c.Y + 1);
			TrySetNeighbor(tile, HexDirection.DownRight,  c.X + 1, c.Y + 1);
		}
	}

	private void TrySetNeighbor(Tile tile, HexDirection dir, int x, int y)
	{
		Vector2I coord = new Vector2I(x, y);

		if (Tiles.TryGetValue(coord, out Tile neighbor))
		{
			tile.SetNeighbor(dir, neighbor);
		}
		
		
		
	}
	public void ApplyLevel(Level level)
{
	// Step 1: Reset everything to Invalid (or Empty)
	foreach (Tile tile in Tiles.Values)
	{
		tile.SetContent(TileContent.Invalid);
	}

	// Step 2: Apply level-defined tiles
	foreach (var pair in level.TileMap)
	{
		if (Tiles.TryGetValue(pair.Key, out Tile tile))
		{
			tile.SetContent(pair.Value);
		}
	}
}

}
