using Godot;
using System;
using System.Collections.Generic;

// Manages core gameplay logic for a single level:
// - Player movement
// - Rock pushing
// - Valid move highlighting
// - Move counting
// - Level completion & star rating
// - UI updates
public partial class GameManager : Node
{
	// Reference to the game board containing all tiles
	[Export] public Board Board;

	// The tile the player currently occupies
	private Tile currentTile;
	
	// Number of moves taken in the current level
	private int moveCount = 0;
	
	// Currently active level data
	private Level currentLevel;
	
	// (Unused flag) Intended to indicate reaching the end tile
	private bool reachedEnd = false;
	
	// Prevents further input after level completion
	private bool levelComplete = false;
	
	// UI elements for level completion display
	[Export] public HBoxContainer LevelCompletePanel;
	[Export] public TextureRect Star1;
	[Export] public TextureRect Star2;
	[Export] public TextureRect Star3;
	
	// UI element showing current move count
	[Export] public RichTextLabel MoveLabel;
	

	// Called when the node enters the scene tree
	public override void _Ready()
	{
		// Subscribe to click events for every tile on the board
		foreach (Tile tile in Board.GetAllTiles())
		{
			tile.Clicked += OnTileClicked;
		}
	}
	
	// Returns a list of tiles the player can currently move to
	public List<Tile> GetValidMoves()
	{
		List<Tile> validMoves = new List<Tile>();

		// Cannot move if player position is unknown
		if (currentTile == null)
			return validMoves;

		// Check all six hex directions
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			Tile neighbor = currentTile.GetNeighbor(dir);

			if (neighbor == null)
				continue;

			// Normal movement: tile is walkable
			if (neighbor.IsWalkable())
			{
				validMoves.Add(neighbor);
				continue;
			}

			// Rock interaction: check if rock can be pushed or broken
			if (neighbor.Content == TileContent.Rock)
			{
				Tile destination = neighbor.GetNeighbor(dir);

				if (destination == null)
					continue;

				// Rock can move into empty/start OR break on wall/end
				if (destination.Content == TileContent.Empty ||
					destination.Content == TileContent.Start ||
					destination.Content == TileContent.Wall ||
					destination.Content == TileContent.End)
				{
					validMoves.Add(neighbor);
				}
			}
		}

		return validMoves;
	}

	// Attempts to move the player in a given direction
	public bool TryMove(HexDirection dir)
	{
		if (currentTile == null)
			return false;

		Tile adjacent = currentTile.GetNeighbor(dir);
		if (adjacent == null)
			return false;

		GD.Print(" → Moving", dir);
		
		// Handle pushing rocks first
		if (adjacent.Content == TileContent.Rock)
		{
			return TryPushRock(adjacent, dir);
		}

		// Handle normal movement
		if (adjacent.IsWalkable())
		{
			// Determine if this move reaches the end tile
			bool reachedEnd = (adjacent.Content == TileContent.End);

			// Move player marker:
			// Current tile becomes empty
			// Destination becomes Start (player position)
			currentTile.SetContent(TileContent.Empty);
			adjacent.SetContent(TileContent.Start);
			currentTile = adjacent;

			// Register move and check completion
			takeMove(reachedEnd);

			// Update highlighted tiles
			UpdateHighlights();
			return true;
		}

		return false;
	}

	// Handles pushing or breaking a rock
	private bool TryPushRock(Tile rockTile, HexDirection dir)
	{
		Tile destination = rockTile.GetNeighbor(dir);

		if (destination == null)
			return false;

		switch (destination.Content)
		{
			case TileContent.Empty:
			case TileContent.Start:
				// Rock moves forward into empty/start tile
				takeMove(false);
				destination.SetContent(TileContent.Rock);
				rockTile.SetContent(TileContent.Empty);
				UpdateHighlights();
				
				return true;

			case TileContent.Wall:
			case TileContent.End:
				// Rock breaks when pushed into wall or end tile
				takeMove(false);
				rockTile.SetContent(TileContent.Empty);
				UpdateHighlights();
				
				return true;

			// Rock cannot move into another rock or invalid tile
			case TileContent.Rock:
			case TileContent.Invalid:
			default:
				return false;
		}
	}

	// Finds the tile marked as the player's starting position
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

	// Called when a tile is clicked
	private void OnTileClicked(Tile tile)
	{
		// Ignore input after level completion
		if (levelComplete) return; 
		
		if (currentTile == null)
			return;

		// Determine direction of clicked tile relative to player
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			if (currentTile.GetNeighbor(dir) == tile)
			{
				TryMove(dir);
				return;
			}
		}
	}

	// Updates which tiles are visually highlighted as valid moves
	private void UpdateHighlights()
	{
		// Clear all highlights first
		foreach (Tile tile in Board.GetAllTiles())
		{
			tile.SetHighlight(false);
		}

		// Do not show highlights if level is complete
		if (levelComplete)
			return;   
		
		// Highlight valid destination tiles
		foreach (Tile tile in GetValidMoves())
		{
			tile.SetHighlight(true);
		}
		
		// Debug output
		PrintValidMoves(GetValidMoves());
	}

	// Initializes and starts a new level
	public void StartLevel(Level level)
	{
		LevelCompletePanel.Visible = false;
		levelComplete = false;
		currentLevel = level;
		moveCount = 0;

		UpdateMoveDisplay();

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

		// Apply level layout to the board
		Board.ApplyLevel(level);

		// Locate player start position
		currentTile = FindStartTile();

		if (currentTile == null)
		{
			GD.PrintErr("GameManager: Start tile not found");
			return;
		}

		GD.Print($"Level {level.LevelNumber} started");

		// Show valid moves immediately
		UpdateHighlights();
	}

	// Debug helper: prints valid moves to console
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
		
		// Also print all neighbors of current tile
		foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
		{
			Tile n = currentTile.GetNeighbor(dir);
			if (n != null)
				GD.Print(dir, ": ", n.Content);
		}
	}

	// Registers a move and checks for level completion
	public void takeMove(bool reachedEnd)
	{
		moveCount++;
		UpdateMoveDisplay();
		
		GD.Print("Moves Taken: ", moveCount);
		
		// If not on end tile, nothing else to do
		if (!reachedEnd) return;

		levelComplete = true;

		// Clear highlights
		foreach (Tile tile in Board.GetAllTiles()){
			tile.SetHighlight(false);
			UpdateHighlights();
		}
			
		GD.Print("Level completed in ", moveCount, " moves");

		int stars = CalculateStars();

		GD.Print("LEVEL COMPLETE!");
		GD.Print($"Stars Earned: {stars}");

		ShowStarResult(stars);
	}

	// Updates the on-screen move counter
	private void UpdateMoveDisplay()
	{
		if (MoveLabel != null)
			MoveLabel.Text = $"Moves: {moveCount}";
	}

	// Calculates star rating based on move thresholds
	private int CalculateStars()
	{
		if (moveCount > currentLevel.OneStarMoves)
			return 0;
		if (moveCount > currentLevel.TwoStarMoves)
			return 1;
		if (moveCount > currentLevel.ThreeStarMoves)
			return 2;

		return 3;
	}

	// Displays star results on the completion panel
	private void ShowStarResult(int stars)
	{
		LevelCompletePanel.Visible = true;

		Star1.Visible = stars >= 1;
		Star2.Visible = stars >= 2;
		Star3.Visible = stars >= 3;
	}
}
