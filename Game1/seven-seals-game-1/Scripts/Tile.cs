using Godot;
using System;
using System.Collections.Generic;

public enum TileContent
{
	Empty,
	Rock,
	Wall,
	Start,
	End,
	Invalid
}

public enum HexDirection
{
	UpLeft,
	UpRight,
	DownLeft,
	DownRight,
	Left,
	Right
}

public partial class Tile : Node
{
	[Export] public TileContent Content { get; set; } = TileContent.Empty;
	[Export] public Vector2 TileLocation { get; set; }

	private Dictionary<HexDirection, Tile> Neighbors;

	public override void _Ready()
	{
		InitializeNeighbors();
	}

	private void InitializeNeighbors()
	{
		Neighbors = new Dictionary<HexDirection, Tile>();
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			Neighbors[dir] = null;
		}
	}

	public void Setup(int x, int y, TileContent content)
	{
		TileLocation = new Vector2(x, y);
		Content = content;
	}

	public void SetNeighbor(HexDirection direction, Tile tile)
	{
		Neighbors[direction] = tile;
	}

	public Tile GetNeighbor(HexDirection direction)
	{
		return Neighbors.TryGetValue(direction, out var tile) ? tile : null;
	}

	public bool IsWalkable()
	{
		return Content == TileContent.Empty ||
			   Content == TileContent.Start ||
			   Content == TileContent.End;
	}
}
