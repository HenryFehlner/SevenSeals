using Godot;
using System;

public partial class ImageColorPicker : Node2D
{
	[Export] protected Node2D userPhoto;
	private Sprite2D photoSprite;
	private Image cachedImage;
	private ColorRect colorDisplayRect;
	private ColorRect colorOutlineRect;
	
	private Color pickedColor = new Color(1, 1, 1, 1);
	
	private Vector2 viewportSize;
	private Vector2 viewportCenter;
	private Vector2 worldPos;
	
	public override void _Ready()
	{
		// Get photo texture and color rect
		photoSprite = GetNode<Sprite2D>("../UserPhoto/PhotoSprite");
		colorDisplayRect = GetNode<ColorRect>("ColorDisplayRect");
		colorOutlineRect = GetNode<ColorRect>("RectOutline");
		
		// Set sprite to the global active photo
		photoSprite.Texture = GD.Load<Texture2D>(GlobalData.ActivePhoto);
		
		// Cache image for performance when getting pixel color
		cachedImage = photoSprite.Texture.GetImage();
		
		// Set position to center of screen
		viewportSize = GetViewportRect().Size;
		viewportCenter = viewportSize / 2.0f;
		Position = viewportCenter;
		
		// Get world pos for conversion to local coords
		worldPos = GetViewport().GetCanvasTransform().AffineInverse() * viewportCenter;
	}
	
	public override void _Process(double delta)
	{
		// Get texture position in local coords
		Vector2 localPos = photoSprite.ToLocal(viewportCenter);
		//GD.Print("Local photo position: " + localPos);
		
		// Convert sprite coords to texture coords
		Vector2 textureCoord = localPos + photoSprite.Texture.GetSize() / 2.0f;
		
		// Clamp because of float freakiness
		int clampedCoordX = Mathf.Clamp((int)textureCoord.X, 1, photoSprite.Texture.GetWidth() - 1);
		int clampedCoordY = Mathf.Clamp((int)textureCoord.Y, 1, photoSprite.Texture.GetHeight() - 1);
		//GD.Print("Clamped texture coord: ", clampedCoordX, ", ", clampedCoordY);
		
		// Get color from pixel
		pickedColor = cachedImage.GetPixel(clampedCoordX, clampedCoordY);
		
		// Set picker colors
		colorOutlineRect.SetColor(InvertColorGrayscale(pickedColor));
		colorDisplayRect.SetColor(pickedColor);
		//GD.Print("Picked color: " + pickedColor + "\n");
	}
	
	private void GetAveragePixelColor(int xPos, int yPos, int radius)
	{
		// TODO: Get the average color of a small group of pixels
		
		// Get number of pixels that would be contained in a square of the radius's size
		
		// For loop with to go over that number of pixels
			// Iterate over each pixel in game of life fashion
		
		// Get average ove all the colors
		
		// Return the new color
	}
	
	private Color InvertColor(Color color)
	{
		return new Color(
			1.0f - color.R,
			1.0f - color.G,
			1.0f - color.B,
			color.A
		);
	}
	
	private Color InvertColorHue(Color color)
	{
		Color inverted = color;
		inverted.H = Mathf.PosMod(color.H + 0.5f, 1.0f);
		return inverted;
	}
	
	private Color InvertColorGrayscale(Color color, float threshold = 0.5f)
	{
		float brightness = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
		//float inverted = 1.0f - brightness;
		//return new Color(inverted, inverted, inverted, 1.0f);
		if (brightness > threshold)
			return new Color(0.0f, 0.0f, 0.0f, 1.0f);
		else
			return new Color(1.0f, 1.0f, 1.0f, 1.0f);
	}
}
