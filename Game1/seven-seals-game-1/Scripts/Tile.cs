using Godot;
using System;
using System.Collections.Generic;

public enum TileContent
{
	Empty,	// 01U1
	Rock,	// M1C4
	Wall,	// 02U1
	Start,	// PR06
	End,	// M1S4
	Invalid	// 03U1
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

public partial class Tile : Node2D
{
	[Export] public TileContent Content { get; set; } = TileContent.Empty;
	[Export] public Vector2 TileLocation { get; set; }
	private Sprite2D tileSprite;

	private Dictionary<HexDirection, Tile> Neighbors;
	
	[Export] public Texture2D EmptyTexture;
	[Export] public Texture2D RockTexture;
	[Export] public Texture2D WallTexture;
	[Export] public Texture2D StartTexture;
	[Export] public Texture2D EndTexture;
	[Export] public Texture2D InvalidTexture;
	

	public override void _Ready()
	{
		InitializeNeighbors();
		tileSprite = GetNode<Sprite2D>("TileSprite");
		UpdateVisual();
	}

	private void InitializeNeighbors()
	{
		Neighbors = new Dictionary<HexDirection, Tile>();
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			Neighbors[dir] = null;
		}
	}

	public void Setup(TileContent content)
	{
		Content = content;
	}
	
	public void Setup(Vector2 pos, TileContent content, Texture2D tileTexture)
	{
		// Get sprite
		tileSprite = GetNode<Sprite2D>("TileSprite");
		
		// Set position, tile type, and texture
		SetTilePosition(pos);
		SetTexture(tileTexture);
		Content = content;
		GD.Print(Content);
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
	
	public void SetContent(TileContent newContent)
	{
		if (Content == newContent)
			return;
		Content = newContent;
		UpdateVisual();
	}
	
	public void SetTilePosition(Vector2 newPos)
	{
		Position = newPos;
	}

	public void SetTexture(Texture2D texture)
	{
		GD.Print(texture);
		tileSprite.Texture = texture;
	}
	
		private void UpdateVisual()
	{
		GD.Print(tileSprite.Texture?.GetSize());
		if (tileSprite == null)
			return;

		tileSprite.Texture = Content switch
		{
			TileContent.Empty => EmptyTexture,
			TileContent.Rock => RockTexture,
			TileContent.Wall => WallTexture,
			TileContent.Start => StartTexture,
			TileContent.End => EndTexture,
			_ => InvalidTexture
		};
	}
}
