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

				tile.Setup(x, y, TileContent.Empty);
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
}
