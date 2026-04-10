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
@onready var patrolTimer = $PatrolTimer
@onready var chaseTimer = $Timer
@onready var heartBeat = $AudioStreamPlayer3D
var wayPointIndex: int
var player
var playerInEarshotFar : bool 
var playerInEarshotClose : bool

func _ready():
	_currentState = States.patrol
	_navigationAgent = $NavigationAgent3D
	player = get_tree().get_nodes_in_group("Player")[0]
	_navigationAgent.target_position = wayPoints[0].global_position
	pass

func _process(delta):
	
	match _currentState:
		States.patrol:
			if(_navigationAgent.is_navigation_finished()):
				_currentState = States.wait
				patrolTimer.start()
				return
			MoveTowardsPoint(delta, patrolSpeed)
			CheckForPlayer()
			heartBeatSounds()
			pass
		States.wait:
			heartBeatSounds()
			pass
		States.stalk:
			if(_navigationAgent.is_navigation_finished()):
				patrolTimer.start()
				_currentState = States.wait
			MoveTowardsPoint(delta, patrolSpeed)
			heartBeatSounds()
			pass
		States.chase:
			if(_navigationAgent.is_navigation_finished()):
				patrolTimer.start()
				_currentState = States.wait
			_navigationAgent.target_position = player.global_position
			MoveTowardsPoint(delta, chaseSpeed)
			heartBeatSounds()
			pass
	pass

func MoveTowardsPoint(delta, speed):
	var targetPos = _navigationAgent.get_next_path_position()
	var direction = global_position.direction_to(targetPos)
	faceDirection(targetPos)
	velocity = direction * speed
	move_and_slide()
	if(playerInEarshotFar):
		CheckForPlayer()

func CheckForPlayer():
	var space_state = get_world_3d().direct_space_state
	var result = space_state.intersect_ray(PhysicsRayQueryParameters3D.create(self.global_position, player.global_position))
	if result.size() > 0:
		if(result["collider"].is_in_group("Player")):
			if(playerInEarshotClose):
				if(result["collider"].crouching == false):
					_currentState = States.chase
					_navigationAgent.target_position = result["collider"].global_position
			if(playerInEarshotFar):
				if(result["collider"].crouching == false):
					_currentState = States.stalk
					_navigationAgent.target_position = result["collider"].global_position

func faceDirection(direction: Vector3):
	look_at(Vector3(direction.x, global_position.y, direction.z ), Vector3.UP)

func _on_patrol_timer_timeout():
	_currentState = States.patrol
	wayPointIndex += 1
	if wayPointIndex > wayPoints.size() - 1:
		wayPointIndex = 0
	_navigationAgent.target_position = wayPoints[wayPointIndex].global_position
	pass # Replace with function body.


func _on_hearing_far_body_entered(body):
	if body.is_in_group("Player"):
		playerInEarshotFar = true 
		print("Player is far in earshot")
	pass # Replace with function body.


func _on_hearing_close_body_entered(body):
	if body.is_in_group("Player"): 
		playerInEarshotClose = true
		print("Player is close in earshot")
	pass # Replace with function body.


func _on_hearing_far_body_exited(body):
	if body.is_in_group("Player"): 
		print("Player is not far in earshot")
		playerInEarshotFar = false
	pass # Replace with function body.


func _on_hearing_close_body_exited(body):
	if body.is_in_group("Player"): 
		playerInEarshotClose = false
		print("Player has left close earshot")
	pass # Replace with function body.

func changeState(newState):
	_currentState = newState

func heartBeatSounds():
	if _currentState != States.chase and heartBeat.playing:
			heartBeat.stop()
	elif heartBeat.playing == false:
		heartBeat.play() 

func _on_timer_timeout():
	if(_currentState == States.chase):
		_currentState = States.patrol
	pass # Replace with function body.
