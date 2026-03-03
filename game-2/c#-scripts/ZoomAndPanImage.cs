using Godot;
using System;
using System.Collections.Generic;

public partial class ZoomAndPanImage : Node2D
{
	private Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
	
	// Image transformation variables
	private float zoom = 1.0f;
	private float minZoom = 1.0f;
	private float maxZoom = 5.0f;
	
	private float lastPinchDistance = 0.0f;
	
	// Dictates how much the player is allowed to zoom in on their photo
	[Export] private float maxAllowedZoomScale = 10.0f;
	
	// For zooming and panning to act correctly, pan must be applied to the parent node and scale to this node
	[Export] private Sprite2D sprite;
	
	public override void _Ready()
	{
		// Initialize the photo centered on the screen
		FitToScreen();
		
		// Get sprite
		//sprite = GetNode<Sprite2D>("PhotoSprite");
	}

	private void FitToScreen()
	{
		// Get viewport and photo sizes
		Vector2 screenSize = GetViewportRect().Size;
		Vector2 imageSize = sprite.Texture.GetSize();
		Position = screenSize / 2.0f;
		
		// Calculate image scale
		float scaleX = screenSize.X / imageSize.X;
		float scaleY = screenSize.Y / imageSize.Y;
		
		float scale = Mathf.Min(scaleX, scaleY);	// Keep aspect ratio
		
		// Set min and max scales and set internal scale
		minZoom = scale;
		maxZoom = scale * maxAllowedZoomScale;
		Zoom(scale);
		
		sprite.Scale = new Vector2(scale, scale);			// Set sprite's scale property
		
		sprite.Position = Vector2.Zero;
		sprite.Centered = true;
	}	
	
	// Main input handler
	public override void _Input(InputEvent e)
	{
		// Use touch controls if on mobile
		if (OS.HasFeature("mobile"))
		{
			MobileInput(e);
		}
		else
		{
			DesktopInput(e);
		}
		
		// Clamp the position
		ClampPosition();
	}
	
	// For mobile builds
	private void MobileInput(InputEvent e)
	{
		// Get touch inputs
		if (e is InputEventScreenTouch touch)
		{
			if (touch.Pressed)
			{
				touches[touch.Index] = touch.Position;
			}
			else
			{
				touches.Remove(touch.Index);
				lastPinchDistance = 0.0f;
			}
		}
		
		// Get drag inputs
		// If one touch, pan, if two touches, zoom
		if (e is InputEventScreenDrag drag)
		{
			touches[drag.Index] = drag.Position;
			
			if (touches.Count == 1)
			{
				Pan(drag.Relative);
			}
			else if (touches.Count == 2)
			{
				PinchZoom();
			}
		}
	}
	
	// For desktop testing
	private void DesktopInput(InputEvent e)
	{
		// Zoom
		if (e is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp)
				Zoom(1.1f);
			if (mouseButton.ButtonIndex == MouseButton.WheelDown)
				Zoom(0.9f);
		}
		// Pan
		if (e is InputEventMouseMotion mouseMotion)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Left))
			{
				// Pan the sprite
				Pan(mouseMotion.Relative);
			}
		}
	}
	
	private void Pan(Vector2 delta)
	{
		Position += delta;
		
		ClampPosition();
	}
	
	private void Zoom(float factor)
	{
		// Multiply sprite scale by factor
		sprite.Scale *= factor;
		
		// Clamp the scale
		float currentScale = sprite.Scale.X;
		currentScale = Mathf.Clamp(currentScale, minZoom, maxZoom);
		sprite.Scale = Vector2.One * currentScale;
		
		// Clamp position
		ClampPosition();
	}
	
	private void PinchZoom()
	{
		var enumerator = touches.Values.GetEnumerator();
		
		// Point 1
		enumerator.MoveNext();
		Vector2 point1 = enumerator.Current;
		
		// Point 2
		enumerator.MoveNext();
		Vector2 point2 = enumerator.Current;
		
		// Get distance between two drag points
		float distance = point1.DistanceTo(point2);
		//GD.Print("Distance between drag points: " + distance);
		
		// Perform zoom
		if (lastPinchDistance != 0)
		{
			float factor = distance / lastPinchDistance;
			Zoom(factor);
		}
		
		lastPinchDistance = distance;
	}
	
	private void ClampPosition()
	{
		Vector2 screen = GetViewportRect().Size;
		Vector2 center = screen / 2.0f;
		
		Vector2 image = sprite.Texture.GetSize() * sprite.Scale;
		
		float halfWidth = image.X / 2.0f;
		float halfHeight = image.Y / 2.0f;
		
		float minX = center.X - halfWidth;
		float maxX = center.X + halfWidth;
		
		float minY = center.Y - halfHeight;
		float maxY = center.Y + halfHeight;
		
		// Create new position vector
		Vector2 newPos = Position;
		
		newPos.X = Mathf.Clamp(newPos.X, minX, maxX);
		newPos.Y = Mathf.Clamp(newPos.Y, minY, maxY);
		
		Position = newPos;
	}
}
