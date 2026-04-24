extends RayCast3D
@onready var flashLight = $".."

func _physics_process(_delta):
	#self.transform = get_parent().transform
	#target_position = transform.basis.z * flashLight.spot_range
	#self.cast_to()
	_check_collision(get_collider())

func _check_collision(collider: Object):
	if collider == null:
		return
	
	if collider.is_in_group("enemies") and flashLight.visible:
		if collider.has_method("react_to_flashlight"):
			collider.react_to_flashlight()
	#elif(collider.is_in_group("enemies") and flashLight.visible == false):
		#collider.changeState(1)
