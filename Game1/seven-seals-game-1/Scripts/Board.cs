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
	
	void LinkTiles(){
		
	}
	
	
	
}
