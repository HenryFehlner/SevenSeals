using Godot;
using System;

public partial class Board : Node
{
	Tile[] AllTiles; 
	
	
	void GenerateTiles(){
		for (x=0; x < 7; x++){
			for(y=0; y < 7; y++){
				Tile tile = new Tile(x,y,tileContent.empty);
			}
		}
	}
	
	
	// Top y- 1
	// Bottom y + 1 
	// TopRight y - 1 x + 1
	//TopLeft  y - 1 x - 1
	//BottomRight y + 1 x + 1
	//BottomLeft  y + 1 x - 1
	
	void LinkTiles(){
		
	}
	
	
	
}
