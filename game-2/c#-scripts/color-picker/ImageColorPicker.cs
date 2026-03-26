using Godot;
using System;
using System.Collections.Generic;

public partial class ImageColorPicker : Node2D
{
	[Export] protected Node2D userPhoto;
	private Sprite2D photoSprite;
	private Image cachedImage;
	private ColorRect colorDisplayRect;
	private ColorRect colorOutlineRect;
	[Export] public Button checkColorButton;
	private Color pickedColor = new Color(1, 1, 1, 1);
	
	private Vector2 viewportSize;
	private Vector2 viewportCenter;
	private Vector2 worldPos;
	
	const int MAX_COLORS = 8;
	
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
		
		// Assign add color button
		checkColorButton.Pressed += CheckColor;
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
		//pickedColor = cachedImage.GetPixel(clampedCoordX, clampedCoordY);
		pickedColor = GetAveragePixelColor(clampedCoordX, clampedCoordY);
		
		// Set picker outline colors
		colorOutlineRect.SetColor(InvertColorGrayscale(pickedColor));
		colorDisplayRect.SetColor(pickedColor);
		//GD.Print("Picked color: " + pickedColor + "\n");
	}
	
	private Color GetAveragePixelColor(int xPos, int yPos)
	{
		// TODO: Get the average color of a small group of pixels
		
		// Temporary color components
		float r = 0.0f;
		float g = 0.0f;
		float b = 0.0f;
		float a = 0.0f;
		
		// Get number of pixels that would be contained in a square of the radius's size
		for (int col = yPos - 1; col <= yPos + 1; col++)
		{
			for (int row = xPos - 1; row <= xPos + 1; row++)
			{
				Color pixelColor = cachedImage.GetPixel(row, col);
				
				r += pixelColor.R;
				g += pixelColor.G;
				b += pixelColor.B;
				a += pixelColor.A;
			}
		}
		
		return new Color(r / 9.0f, g / 9.0f, b / 9.0f, a / 9.0f);
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
	
	private float PivotRGB(float n)
	{
		return (n > 0.04045f) ? Mathf.Pow((n + 0.055f) / 1.055f, 2.4f) : n / 12.92f;
	}
	
	private float PivotXYZ(float n)
	{
		return (n > 0.008856f) ? Mathf.Pow(n, 1.0f / 3.0f) : (7.787f * n) + (16.0f / 116.0f);
	}
	
	private Vector3 RGBToLAB(Color input)
	{
		// sRGB to linear RGB
		float r = PivotRGB(input.R);
		float g = PivotRGB(input.G);
		float b = PivotRGB(input.B);
		//float r = input.R;
		//float g = input.G;
		//float b = input.B;
		
		// Linear RGB to XYZ
		float x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
		float y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
		float z = r * 0.0193f + g * 0.1192f + b * 0.9505f;
		
		// Scale to 0-100
		x *= 100.0f;
		y *= 100.0f;
		z *= 100.0f;
		
		// XYZ to LAB
		// Reference white D65
		float refX = 95.047f;
		float refY = 100.0f;
		float refZ = 108.883f;
		
		x /= refX;
		y /= refY;
		z /= refZ;
		
		x = PivotXYZ(x);
		y = PivotXYZ(y);
		z = PivotXYZ(z);
		
		float L = (116.0f * y) - 16.0f;
		float A = 500.0f * (x - y);
		float B = 200.0f * (y - z);
		
		return new Vector3(L, A, B);
	}
	
	// Calculate difference between two colors (CIE76 implementation)
	private float DeltaE(Vector3 LAB1, Vector3 LAB2)
	{
		float dL = LAB1.X - LAB2.X;
		float dA = LAB1.Y - LAB2.Y;
		float dB = LAB1.Z - LAB2.Z;
		
		return Mathf.Sqrt(dL * dL + dA * dA + dB * dB);
	}
	
	private void CheckColor()
	{
		// Iterate through required colors
		foreach (KeyValuePair<Color, bool> kvp in GlobalData.ActivePainting.RequiredColors)
		{	
			// If color hasnt been found check for similarity
			if (!kvp.Value)
			{
				// Get required color
				Color requiredColor = kvp.Key;
				
				// Convert RGB colors to CIELAB
				Vector3 requiredLAB = RGBToLAB(requiredColor);
				Vector3 pickedLAB = RGBToLAB(pickedColor);
				
				// Calculate deltaE
				float deltaE = DeltaE(requiredLAB, pickedLAB);
				GD.Print("Delta E: " + deltaE);
				
				// Compare & mark as found
				if (deltaE < 15.0f)
				{
					GD.Print("Color found!");
					GlobalData.ActivePainting.RequiredColors[kvp.Key] = true;
					
					GetNode<ColorPickerPalette>("../PalettePanelContainer").UpdateFoundColors();
				}
			}
		}
		GD.Print("\n");
	}
	
	private void AddColor()
	{
		if (GlobalData.SavedColors.Count >= MAX_COLORS)
		{
			GlobalData.SavedColors.RemoveAt(0);
		}
		
		GlobalData.SavedColors.Add(pickedColor);
	}
	
	
	
	public void _on_return_button_pressed()
	{
		GD.Print("Returning to gallery");
		GetTree().ChangeSceneToFile("res://scenes/image-gallery.tscn");
	}
	
	public void _on_add_color_button_pressed()
	{
		GD.Print("Pressed check color button");
	}
}
