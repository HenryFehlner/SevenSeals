using Godot;
using System;
using System.Collections.Generic;

// ─────────────────────────────────────────────
// BOARD CLASS
// ─────────────────────────────────────────────
// Responsible for:
// ✔ Creating the grid of tiles
// ✔ Positioning tiles in world space
// ✔ Linking neighbors (hex graph)
// ✔ Applying level data to tiles
// ✔ Providing access to tiles
public partial class Board : Node
{
	// Tile scene prefab (set in Inspector)
	// This scene will be instantiated for every grid cell
	[Export] public PackedScene TileScene;

	// Size of hex used for coordinate-to-world conversion
	[Export] private float hexSideLength = 32f;

	// Fixed board dimensions (7×7 grid)
	private const int Width = 7;
	private const int Height = 7;

	// Dictionary mapping grid coordinates → Tile instance
	// Vector2I is perfect for grid indexing
	private Dictionary<Vector2I, Tile> Tiles;

	// Called when node enters scene tree
	public override void _Ready()
	{
		// Step 1: Create tile objects
		GenerateTiles();

		// Step 2: Link them as neighbors
		LinkTiles();
	}

	// Returns all tiles on the board
	// Used by GameManager for iteration
	public IEnumerable<Tile> GetAllTiles()
	{
		return Tiles.Values;
	}

	// ─────────────────────────────
	// TILE CREATION (SINGLE SOURCE)
	// ─────────────────────────────
	private void GenerateTiles()
	{
		// Initialize dictionary
		Tiles = new Dictionary<Vector2I, Tile>();

		// Loop through grid coordinates
		for (int x = 0; x < Width; x++)
		{
			for (int y = 0; y < Height; y++)
			{
				Vector2I coord = new Vector2I(x, y);

				// Instantiate tile scene
				Tile tile = TileScene.Instantiate<Tile>();

				// Convert grid coordinate → world position
				tile.Position = AxialToWorldCoords(coord);

				// Default content before level is applied
				tile.SetContent(TileContent.Empty);

				// Add to scene tree so it becomes visible/active
				AddChild(tile);

				// Store tile in dictionary for lookup
				Tiles[coord] = tile;
			}
		}
	}

	// ─────────────────────────────
	// NEIGHBOR LINKING
	// ─────────────────────────────
	// Creates graph connections between tiles.
	// Uses "odd-row offset" hex layout.
	private void LinkTiles()
	{
		foreach (var pair in Tiles)
		{
			Vector2I c = pair.Key;
			Tile tile = pair.Value;

			// Determine row parity
			bool isOddRow = (c.Y % 2 == 1);

			if (isOddRow)
			{
				// Neighbor coordinates for odd rows
				TrySetNeighbor(tile, HexDirection.UpLeft,    c.X,     c.Y - 1);
				TrySetNeighbor(tile, HexDirection.UpRight,   c.X + 1, c.Y - 1);

				TrySetNeighbor(tile, HexDirection.Left,      c.X - 1, c.Y);
				TrySetNeighbor(tile, HexDirection.Right,     c.X + 1, c.Y);

				TrySetNeighbor(tile, HexDirection.DownLeft,  c.X,     c.Y + 1);
				TrySetNeighbor(tile, HexDirection.DownRight, c.X + 1, c.Y + 1);
			}
			else
			{
				// Neighbor coordinates for even rows
				TrySetNeighbor(tile, HexDirection.UpLeft,    c.X - 1, c.Y - 1);
				TrySetNeighbor(tile, HexDirection.UpRight,   c.X,     c.Y - 1);

				TrySetNeighbor(tile, HexDirection.Left,      c.X - 1, c.Y);
				TrySetNeighbor(tile, HexDirection.Right,     c.X + 1, c.Y);

				TrySetNeighbor(tile, HexDirection.DownLeft,  c.X - 1, c.Y + 1);
				TrySetNeighbor(tile, HexDirection.DownRight, c.X,     c.Y + 1);
			}
		}
	}

	// Attempts to assign a neighbor if coordinate exists
	private void TrySetNeighbor(Tile tile, HexDirection dir, int x, int y)
	{
		Vector2I coord = new Vector2I(x, y);

		// Only link if tile exists in dictionary
		if (Tiles.TryGetValue(coord, out Tile neighbor))
		{
			tile.SetNeighbor(dir, neighbor);
		}
	}

	// ─────────────────────────────
	// LEVEL APPLICATION
	// ─────────────────────────────
	// Applies level data to the existing grid
	public void ApplyLevel(Level level)
	{
		// Step 1: Reset all tiles to Invalid
		// (outside playable area)
		foreach (Tile tile in Tiles.Values)
			tile.SetContent(TileContent.Invalid);

		// Step 2: Apply level-specific tile data
		foreach (var pair in level.TileMap)
		{
			// Only apply if coordinate exists on board
			if (Tiles.TryGetValue(pair.Key, out Tile tile))
				tile.SetContent(pair.Value);
		}
	}

	// ─────────────────────────────
	// COORD CONVERSION
	// ─────────────────────────────
	// Converts grid coordinates to world space positions
	// Uses pointy-top hex layout with odd-row offset
	private Vector2 AxialToWorldCoords(Vector2I coord)
	{
		// Horizontal distance between columns
		float widthOffset = Mathf.Sqrt(3.0f) * hexSideLength;

		// Vertical spacing between rows
		float yPos = (3.0f / 2.0f) * hexSideLength * coord.Y;

		// Horizontal position depends on row parity
		float xPos = (coord.Y % 2 == 0)
			? -widthOffset * coord.X                // even row
			: -widthOffset * coord.X - (widthOffset / 2.0f); // odd row shifted

		return new Vector2(xPos, yPos);
	}
}
