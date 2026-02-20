extends Control

func _ready():
	$AnimationPlayer.play("RESET")

func resume():
	get_tree().paused = false
	$AnimationPlayer.play_backwards("blur")
	$".".visible = false

func pause():
	get_tree().paused = true
	$AnimationPlayer.play("blur")

func esc():
	if Input.is_action_just_pressed("ui_cancel") and get_tree().paused == false:
		pause()
	elif Input.is_action_just_pressed("ui_cancel") and get_tree().pause == true:
		resume()
	

func _on_resume_pressed():
	resume()


func _on_restart_pressed():
	resume()
	get_tree().reload_current_scene()

func _on_back_home_pressed():
	resume()
	get_tree().change_scene_to_file("res://scenes/starting_menu.tscn")


func _process(_delta):
	esc()
