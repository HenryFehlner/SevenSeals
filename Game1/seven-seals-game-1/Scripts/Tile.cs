using Godot;
using System;
using System.Collections.Generic;

// ─────────────────────────────────────────────
// ENUM: What exists on a tile
// ─────────────────────────────────────────────
// These values represent the gameplay state of a tile.
// The comments show the IDs from your level editor / JSON.
public enum TileContent
{
	Empty,	// 01U1 — walkable
	Rock,	// M1C4 — pushable obstacle
	Wall,	// 02U1 — impassable
	Start,	// PR06 — player position
	End,	// M1S4 — goal tile
	Invalid	// 03U1 — outside playable area
}

// ─────────────────────────────────────────────
// ENUM: Directions in a hex grid
// ─────────────────────────────────────────────
// Used to link tiles together as neighbors.
public enum HexDirection
{
	UpLeft,
	UpRight,
	DownLeft,
	DownRight,
	Left,
	Right
}

// ─────────────────────────────────────────────
// TILE CLASS
// ─────────────────────────────────────────────
// Represents ONE hex cell on the board.
// Handles visuals, input, neighbors, and content.
public partial class Tile : Node2D
{
	// Current logical content of this tile
	[Export] public TileContent Content { get; set; } = TileContent.Empty;

	// Optional stored coordinate (not required for rendering)
	[Export] public Vector2 TileLocation { get; set; }

	// Reference to the sprite that displays the tile
	private Sprite2D tileSprite;

	// Event fired when this tile is clicked
	// GameManager subscribes to this
	public event Action<Tile> Clicked;

	// Neighbor map for hex connectivity
	private Dictionary<HexDirection, Tile> Neighbors;
	
	// ─────────────────────────────────────────
	// Textures assigned via Inspector (Prefab)
	// ─────────────────────────────────────────
	[Export] public Texture2D EmptyTexture;
	[Export] public Texture2D RockTexture;
	[Export] public Texture2D WallTexture;
	[Export] public Texture2D StartTexture;
	[Export] public Texture2D EndTexture;
	[Export] public Texture2D InvalidTexture;
	

	// Called when the node enters the scene tree
	public override void _Ready()
	{
		// Prepare neighbor dictionary
		InitializeNeighbors();

		// Get the sprite child node
		tileSprite = GetNode<Sprite2D>("TileSprite");

		// Set correct texture based on Content
		UpdateVisual();
		
		// Connect input detection from Area2D
		var area = GetNode<Area2D>("Area2D");
		area.InputEvent += OnInputEvent;
	}

	// ─────────────────────────────────────────
	// Initializes dictionary with all directions
	// ─────────────────────────────────────────
	private void InitializeNeighbors()
	{
		Neighbors = new Dictionary<HexDirection, Tile>();

		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			Neighbors[dir] = null;
		}
	}

	// Simple setup — assign content only
	public void Setup(TileContent content)
	{
		Content = content;
	}
	
	// Full setup — position, content, texture
	public void Setup(Vector2 pos, TileContent content, Texture2D tileTexture)
	{
		// Get sprite reference
		tileSprite = GetNode<Sprite2D>("TileSprite");
		
		// Apply position and visual
		SetTilePosition(pos);
		SetTexture(tileTexture);

		// Set logical content
		Content = content;

		GD.Print(Content);
	}

	// ─────────────────────────────────────────
	// Neighbor management
	// ─────────────────────────────────────────

	// Assign neighbor in a direction
	public void SetNeighbor(HexDirection direction, Tile tile)
	{
		Neighbors[direction] = tile;
	}

	// Retrieve neighbor in a direction
	public Tile GetNeighbor(HexDirection direction)
	{
		return Neighbors.TryGetValue(direction, out var tile) ? tile : null;
	}

	// ─────────────────────────────────────────
	// Gameplay logic helpers
	// ─────────────────────────────────────────

	// Determines if player can step on this tile
	public bool IsWalkable()
	{
		return Content == TileContent.Empty ||
			   Content == TileContent.Start ||
			   Content == TileContent.End;
	}
	
	// Change tile content AND update visuals
	public void SetContent(TileContent newContent)
	{
		if (Content == newContent)
			return;

		Content = newContent;
		UpdateVisual();
	}
	
	// Move tile in world space
	public void SetTilePosition(Vector2 newPos)
	{
		Position = newPos;
	}

	// Directly set sprite texture
	public void SetTexture(Texture2D texture)
	{
		GD.Print(texture);
		tileSprite.Texture = texture;
	}
	
	// ─────────────────────────────────────────
	// Updates sprite based on current Content
	// ─────────────────────────────────────────
	private void UpdateVisual()
	{
		// Debug: print current texture size
		GD.Print(tileSprite.Texture?.GetSize());

		if (tileSprite == null)
			return;

		// Select texture using C# switch expression
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
	
	// ─────────────────────────────────────────
	// Highlighting for valid moves
	// ─────────────────────────────────────────
	public void SetHighlight(bool on)
	{
		// Modulate color without changing texture
		tileSprite.Modulate = on ? Colors.Yellow : Colors.White;
	}
	
	// ─────────────────────────────────────────
	// Mouse click handling
	// ─────────────────────────────────────────
	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		// Detect left mouse button press
		if (@event is InputEventMouseButton mouse &&
			mouse.Pressed &&
			mouse.ButtonIndex == MouseButton.Left)
		{
			GD.Print("Tile clicked!");

			// Notify listeners (GameManager)
			Clicked?.Invoke(this);
		}
	}
}
