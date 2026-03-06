using Godot;
using System;

public partial class ColorPickerButtons : PanelContainer
{
	public void _on_return_button_pressed()
	{
		GD.Print("Returning to main scene");
		GetTree().ChangeSceneToFile("res://scenes/game_play.tscn");
	}
	
	public void _on_add_color_button_pressed()
	{
		GD.Print("Pressed add color button");
	}
}
