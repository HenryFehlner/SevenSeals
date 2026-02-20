extends Control

@onready var pause = $pause
@onready var pauseMenu = $PauseMenu

func _on_pause_pressed():
	pauseMenu.visible = true
	pauseMenu.pause()
	
