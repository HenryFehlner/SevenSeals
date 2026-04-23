extends CharacterBody3D

# Player Nodes 
@onready var standing_collision = $standingCollision
@onready var crouching_collision = $crouchingCollision
@onready var head = $head
@onready var ray_cast_3d = $RayCast3D
@onready var eyes = $head/eyes
@onready var hand = $hand
@onready var flashLight = $hand/SpotLight3D
@onready var walkingSounds = $walkingSound
@onready var runningSounds = $runningSound
@onready var slideResetTimer = $SlideResetTimer
@onready var slideDurationTimer = $SlideDurationTimer
#Movement Variables
var SPEED = 5.0

const walking_speed = 5.0
const sprinting_speed = 12.0
const crouching_speed = 2.0
const JUMP_VELOCITY = 4.5

var direction = Vector3.ZERO
var lerp_speed = 10.0
var crouching_depth = -0.5

#sliding
const slide_speed = 0.1
var slide_dir = Vector3.ZERO

#Head Bobbing variables
const head_bobbing_sprinting_speed = 22.0
const head_bobbing_walking_speed = 12.0
const head_bobbing_crouching_speed = 9.0

const head_bobbing_sprinting_intensity = 0.3
const head_bobbing_walking_intensity = 0.2
const head_bobbing_crouching_intensity = 0.05

var head_bobbing_vector = Vector2.ZERO
var head_bobbing_index = 0.0
var head_bobbing_current_intensity =0.0

#states
var walking = false;
var sprinting = false; 
var crouching = false; 
var sliding = false; 
var slide_ready = true;
var moving = false;

#Camera Controls 
@export var MOUSE_SENSITIVITY : float = 0.4
@export var TILT_LOWER_LIMIT := deg_to_rad(-90.0)
@export var TILT_UPPER_LIMIT := deg_to_rad(90.0)
@export var CAMERA_CONTROLLER : Camera3D
var _mouse_input : bool = false
var _mouse_rotation : Vector3
var _rotation_input : float
var _tilt_input : float
var _player_rotation : Vector3
var _camera_rotation : Vector3

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

func _input(event):
	if event.is_action_pressed("escape"):
		get_tree().quit()

func _unhandled_input(event):
	_mouse_input = event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED
	if _mouse_input:
		_rotation_input = -event.relative.x * MOUSE_SENSITIVITY
		_tilt_input = -event.relative.y * MOUSE_SENSITIVITY
		

func _update_camera(delta):
	_mouse_rotation.x += _tilt_input * delta
	_mouse_rotation.x = clamp(_mouse_rotation.x, TILT_LOWER_LIMIT, TILT_UPPER_LIMIT)
	_mouse_rotation.y += _rotation_input * delta
	
	_player_rotation = Vector3(0.0, _mouse_rotation.y, 0.0)
	_camera_rotation = Vector3(_mouse_rotation.x,0.0,0.0)
	
	head.transform.basis = Basis.from_euler(_camera_rotation)
	head.rotation.z = 0.0
	
	global_transform.basis = Basis.from_euler(_player_rotation)
	
	_rotation_input = 0.0
	_tilt_input = 0.0
	
	#supposed to be following head.y
	flashLight.rotation.y = _camera_rotation.y
	
	#supposed to be following eyes.x
	flashLight.rotation.x = _camera_rotation.x

func _ready():
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	flashLight.spot_range = 50
	slideResetTimer.one_shot = true
	slideDurationTimer.one_shot = true

func _physics_process(delta):
	var input_dir = Input.get_vector("left", "right", "forward", "backwards")

	# Add the gravity.
	if not is_on_floor():
		velocity.y -= gravity * delta
		
	_update_camera(delta)
	
	#Flashlight toggle
	if Input.is_action_just_pressed("flashLightToggle"):
		flashLight.visible = !flashLight.visible
	
	#Handling movement states
	#crouching is not working 
	if Input.is_action_pressed("crouch"):
		SPEED = crouching_speed
		head.position.y = lerp(CAMERA_CONTROLLER.position.y, .6 +crouching_depth, delta*lerp_speed)
		standing_collision.disabled = true
		crouching_collision.disabled= false
		crouching = true
		sprinting = false
		walking = false
		
	elif !ray_cast_3d.is_colliding():
		standing_collision.disabled = false
		crouching_collision.disabled= true
		
		head.position.y = lerp(CAMERA_CONTROLLER.position.y, .6, delta*lerp_speed)
		
		crouching = false
		sprinting = false
		walking = true
		head_bobbing_current_intensity = head_bobbing_walking_intensity
		head_bobbing_index += head_bobbing_walking_speed* delta
	
		if Input.is_action_pressed("sprint"):
				SPEED = sprinting_speed
				crouching = false
				sprinting = true
				walking = false
				
		else: 
			SPEED = walking_speed
	
	#Sounds		
	if input_dir != Vector2.ZERO:
		if !sprinting:
			if not walkingSounds.playing:
				walkingSounds.play()
			if runningSounds.playing:
				runningSounds.stop()
		elif sprinting and !walking:
			if not runningSounds.playing:
				runningSounds.play()
			if walkingSounds.playing:
				walkingSounds.stop()
	else:
		if walkingSounds.playing:
			walkingSounds.stop()
		if runningSounds.playing:
			runningSounds.stop()
		
	#HeadBobbing
	if sprinting:
		head_bobbing_current_intensity = head_bobbing_sprinting_intensity
		head_bobbing_index += head_bobbing_sprinting_speed*delta
	elif walking:
		head_bobbing_current_intensity = head_bobbing_walking_intensity
		head_bobbing_index += head_bobbing_walking_speed*delta
	elif crouching:
		head_bobbing_current_intensity = head_bobbing_crouching_intensity
		head_bobbing_index += head_bobbing_crouching_speed*delta
	
	if is_on_floor() && input_dir != Vector2.ZERO:
		head_bobbing_vector.y = sin(head_bobbing_index)
		head_bobbing_vector.x = sin(head_bobbing_index/2)+0.5
		
		eyes.position.y = lerp(eyes.position.y,head_bobbing_vector.y*(head_bobbing_current_intensity/2.0),delta*lerp_speed)
		eyes.position.x = lerp(eyes.position.x,head_bobbing_vector.x*(head_bobbing_current_intensity),delta*lerp_speed)
		
	else:
		eyes.position.y = lerp(eyes.position.y,0.0,delta*lerp_speed)
		eyes.position.x = lerp(eyes.position.x,0.0,delta*lerp_speed)
		
	# Handle jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	
	# As good practice, you should replace UI actions with custom gameplay actions.
	direction = lerp(direction,(transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized(), delta*lerp_speed)
	
	# Allow input only if not sliding
	if sliding:
		position += slide_dir * slide_speed
		head.position.y = .6 +crouching_depth
	elif direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
		
	# just check for movement input
	if input_dir:
		moving = true
	else:
		moving = false
	
	#slide
	if Input.is_action_just_pressed("slide") and moving and slide_ready:
		sliding = true
		slide_ready = false
		slide_dir = Vector3(velocity.x, 0, velocity.z)
		slideResetTimer.start()
		slideDurationTimer.start()
		print("Initiate slide")

	move_and_slide()


func _on_slide_timer_timeout() -> void:
	print("Reset slide timer")
	slide_ready = true

func _on_slide_duration_timer_timeout() -> void:
	print("End slide")
	sliding = false
	head.position.y = lerp(CAMERA_CONTROLLER.position.y, .6, lerp_speed)
