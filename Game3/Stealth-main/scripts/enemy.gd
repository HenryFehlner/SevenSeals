extends CharacterBody3D

enum States{
	patrol, 
	wait,
	stalk,
	chase
}

var _currentState: States
var _navigationAgent: NavigationAgent3D

@export var wayPoints: Array[Marker3D]
@export var chaseSpeed = 2
@export var patrolSpeed = 1
@export var chaseGraceDuration: float = 1.0

@onready var stateIndicator = $StateIndicator
@onready var destinationIndicator = $DestinationIndicator
@onready var stateMat : StandardMaterial3D = stateIndicator.get_surface_override_material(0)
@onready var patrolTimer = $PatrolTimer
@onready var heartBeat = $AudioStreamPlayer3D

var wayPointIndex: int
var player

var playerInEarshotFar : bool = false
var playerInEarshotClose : bool = false

var lastKnownPlayerPos: Vector3
var hasLastKnownPos: bool = false

var chaseGraceTimer: float = 0.0

func _ready():
	_navigationAgent = $NavigationAgent3D
	player = get_tree().get_nodes_in_group("Player")[0]
	
	changeState(States.patrol)
	updateStateIndicator()
	_navigationAgent.target_position = wayPoints[0].global_position


func _process(delta):
	match _currentState:
		
		States.patrol:
			destinationIndicator.visible = false
			if _navigationAgent.is_navigation_finished():
				changeState(States.wait)
				patrolTimer.start()
				return
			
			MoveTowardsPoint(delta, patrolSpeed)
			CheckForPlayer()
		
		
		States.wait:
			CheckForPlayer()
			destinationIndicator.visible = false
		
		
		States.stalk:
			if hasLastKnownPos:
				_navigationAgent.target_position = lastKnownPlayerPos
				updateDestinationIndicator()
			
			if _navigationAgent.is_navigation_finished():
				hasLastKnownPos = false
				changeState(States.patrol)
				return
			
			MoveTowardsPoint(delta, patrolSpeed)
			CheckForPlayer()
			destinationIndicator.visible = true
		
		
		States.chase:
			CheckForPlayer()
			destinationIndicator.visible = false
			
			if playerInEarshotClose:
				chaseGraceTimer = chaseGraceDuration
				
				_navigationAgent.target_position = player.global_position
				lastKnownPlayerPos = player.global_position
				hasLastKnownPos = true
			else:
				chaseGraceTimer -= delta
				
				if chaseGraceTimer <= 0.0:
					changeState(States.stalk)
					return
				
				if hasLastKnownPos:
					_navigationAgent.target_position = lastKnownPlayerPos
			
			if _navigationAgent.is_navigation_finished():
				changeState(States.stalk)
				return
			
			MoveTowardsPoint(delta, chaseSpeed)
	
	heartBeatSounds()


func MoveTowardsPoint(_delta, speed):
	var targetPos = _navigationAgent.get_next_path_position()
	
	if global_position.distance_to(targetPos) < 0.05:
		velocity = Vector3.ZERO
		return
	
	var direction = global_position.direction_to(targetPos)
	faceDirection(targetPos)
	velocity = direction * speed
	move_and_slide()


func CheckForPlayer():
	var space_state = get_world_3d().direct_space_state
	var result = space_state.intersect_ray(
		PhysicsRayQueryParameters3D.create(global_position, player.global_position)
	)
	
	if result.size() > 0 and result["collider"].is_in_group("Player"):
		if result["collider"].crouching == false:
			
			lastKnownPlayerPos = player.global_position
			hasLastKnownPos = true
			
			if playerInEarshotClose:
				changeState(States.chase)
				return
			
			elif playerInEarshotFar:
				changeState(States.stalk)


func faceDirection(direction: Vector3):
	var target = Vector3(direction.x, global_position.y, direction.z)
	
	if global_position.distance_to(target) < 0.01:
		return
		
	look_at(target, Vector3.UP)


func _on_patrol_timer_timeout():
	changeState(States.patrol)
	
	wayPointIndex += 1
	if wayPointIndex > wayPoints.size() - 1:
		wayPointIndex = 0
	
	_navigationAgent.target_position = wayPoints[wayPointIndex].global_position


func _on_hearing_far_body_entered(body):
	if body.is_in_group("Player"):
		playerInEarshotFar = true


func _on_hearing_close_body_entered(body):
	if body.is_in_group("Player"): 
		playerInEarshotClose = true


func _on_hearing_far_body_exited(body):
	if body.is_in_group("Player"): 
		playerInEarshotFar = false


func _on_hearing_close_body_exited(body):
	if body.is_in_group("Player"): 
		playerInEarshotClose = false


func changeState(newState):
	if _currentState == newState:
		return
	
	_currentState = newState
	updateStateIndicator()


func heartBeatSounds():
	if _currentState != States.chase and heartBeat.playing:
		heartBeat.stop()
	elif _currentState == States.chase and heartBeat.playing == false:
		heartBeat.play()


func updateStateIndicator():
	if stateMat == null:
		print("Material is NULL!")
		return
	
	match _currentState:
		States.patrol:
			stateMat.albedo_color = Color(0, 1, 0)
			stateMat.emission = Color(0, 1, 0)
			destinationIndicator.visible = false
			
		States.wait:
			stateMat.albedo_color = Color(1, 1, 0)
			stateMat.emission = Color(1, 1, 0)
			destinationIndicator.visible = false
			
		States.stalk:
			stateMat.albedo_color = Color(1, 0.5, 0)
			stateMat.emission = Color(1, 0.5, 0)
			destinationIndicator.visible = true
			
		States.chase:
			stateMat.albedo_color = Color(1, 0, 0)
			stateMat.emission = Color(1, 0, 0)
			destinationIndicator.visible = false


var lastFlashHitTime = 0.0

func updateDestinationIndicator():
	if not destinationIndicator.visible:
		return
	
	if hasLastKnownPos:
		destinationIndicator.global_position = lastKnownPlayerPos


func react_to_flashlight():
	if Time.get_ticks_msec() - lastFlashHitTime < 200:
		return
	
	lastFlashHitTime = Time.get_ticks_msec()
	
	lastKnownPlayerPos = player.global_position
	hasLastKnownPos = true
	changeState(States.chase)
