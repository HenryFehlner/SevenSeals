using Godot;
using System;
using System.Collections.Generic;

// This is the struct for a painting
// Should contain:
//		GUID for creating a filepath for unique photo storage
//		Image to paint
//		Required colors palette
[GlobalClass]
public partial class PaintingStruct : Resource
{
	string PaintingID;							// name of painting
	string ColoringImagePath;					// path to lineart
	Dictionary<Color, bool> RequiredColors;		// required colors and whether they have been found
	
	public PaintingStruct(string paintingID, string coloringImagePath, Dictionary<Color, bool> requiredColors)
	{
		PaintingID = paintingID;
		ColoringImagePath = coloringImagePath;
		RequiredColors = requiredColors;
	}
}
