extends RayCast3D
@onready var flashLight = $".."

func _physics_process(delta):
	#self.transform = get_parent().transform
	#target_position = transform.basis.z * flashLight.spot_range
	#self.cast_to()
	_check_collision(get_collider())

func _check_collision(collider: Object):
	if (collider == null):
		return
	
	print_debug(collider)
	if (collider.is_in_group("enemies") and flashLight.visible != false):
		collider.changeState(3) 
		collider.chaseTimer.start()
	#elif(collider.is_in_group("enemies") and flashLight.visible == false):
		#collider.changeState(1)
