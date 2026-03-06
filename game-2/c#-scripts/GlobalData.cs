using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalData : Node
{
	 //Put all persisting data between scenes in here
	// For example, user-picked colors, selected photo for color picking
	
	public static string ActivePhoto;
	public static List<Color> SavedColors = new List<Color>();
}
