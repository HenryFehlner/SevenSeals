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
	string paintingID;							// name of painting
	string coloringImagePath;					// path to lineart
	List<List<Color>> requiredColors;		// a list comprised of colors and whether they have been found, should always be initialized to false
}
