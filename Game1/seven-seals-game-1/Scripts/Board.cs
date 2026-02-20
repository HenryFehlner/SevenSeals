using Godot;
using System;
using System.Collections.Generic;

public partial class Board : Node
{
	[Export] public PackedScene TileScene;
	[Export] private float hexSideLength = 32f;

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

	// ─────────────────────────────
	// TILE CREATION (SINGLE SOURCE)
	// ─────────────────────────────
	private void GenerateTiles()
	{
		Tiles = new Dictionary<Vector2I, Tile>();

		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				Vector2I coord = new Vector2I(x, y);

				Tile tile = TileScene.Instantiate<Tile>();
				tile.Position = AxialToWorldCoords(coord);
				tile.SetContent(TileContent.Empty);

				AddChild(tile);
				Tiles[coord] = tile;
			}
		}
	}

	// ─────────────────────────────
	// NEIGHBOR LINKING
	// ─────────────────────────────
	private void LinkTiles()
	{
		foreach (var pair in Tiles)
		{
			Vector2I c = pair.Key;
			Tile tile = pair.Value;

			TrySetNeighbor(tile, HexDirection.UpLeft,    c.X - 1, c.Y - 1);
			TrySetNeighbor(tile, HexDirection.UpRight,   c.X + 1, c.Y - 1);

			TrySetNeighbor(tile, HexDirection.Left,      c.X - 1, c.Y);
			TrySetNeighbor(tile, HexDirection.Right,     c.X + 1, c.Y);

			TrySetNeighbor(tile, HexDirection.DownLeft,  c.X - 1, c.Y + 1);
			TrySetNeighbor(tile, HexDirection.DownRight, c.X + 1, c.Y + 1);
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

	// ─────────────────────────────
	// LEVEL APPLICATION
	// ─────────────────────────────
	public void ApplyLevel(Level level)
	{
		foreach (Tile tile in Tiles.Values)
			tile.SetContent(TileContent.Invalid);

		foreach (var pair in level.TileMap)
		{
			if (Tiles.TryGetValue(pair.Key, out Tile tile))
				tile.SetContent(pair.Value);
		}
	}

	// ─────────────────────────────
	// COORD CONVERSION
	// ─────────────────────────────
	private Vector2 AxialToWorldCoords(Vector2I coord)
	{
		float widthOffset = Mathf.Sqrt(3.0f) * hexSideLength;
		float yPos = (3.0f / 2.0f) * hexSideLength * coord.Y;

		float xPos = (coord.Y % 2 == 0)
			? -widthOffset * coord.X
			: -widthOffset * coord.X - (widthOffset / 2.0f);

		return new Vector2(xPos, yPos);
	}
}
