@tool
extends Node2D

@export var start_offset: Vector2 = Vector2(-120, 0)
@export var speed: float = 250.0
@export var curve_amount: float = 0.0
@export var duration_seconds: float = 0.6
@export var preview_in_editor: bool = true
@export var loop_in_editor: bool = true

var _t: float = 0.0
var _flight_seconds: float = 0.2
var _life: float = 0.0
var _start: Vector2
var _end: Vector2
var _control: Vector2
var _end_global: Vector2
var _has_end: bool = false

func _enter_tree() -> void:
	if Engine.is_editor_hint():
		set_process(true)

func _ready() -> void:
	_reset_path()

func _process(delta: float) -> void:
	if Engine.is_editor_hint() and not preview_in_editor:
		return

	_life += delta
	_t += delta / max(_flight_seconds, 0.001)
	if _t >= 1.0:
		_t = 1.0

	global_position = _eval(_t)

	var should_restart := false
	if _t >= 1.0 and Engine.is_editor_hint() and loop_in_editor:
		should_restart = true
	elif not Engine.is_editor_hint() and duration_seconds > 0.0 and _life >= duration_seconds:
		queue_free()
		return
	elif Engine.is_editor_hint() and loop_in_editor and duration_seconds > 0.0 and _life >= duration_seconds:
		should_restart = true

	if should_restart:
		_reset_path()

func _reset_path() -> void:
	if not _has_end:
		_end_global = global_position
		_has_end = true

	_start = _end_global + start_offset
	_end = _end_global
	global_position = _start
	_t = 0.0
	_life = 0.0

	var dist := _start.distance_to(_end)
	_flight_seconds = dist / max(speed, 0.001)
	_flight_seconds = max(_flight_seconds, 0.001)

	var mid := (_start + _end) * 0.5
	var dir := (_end - _start)
	if dir.length_squared() > 0.0001:
		dir = dir.normalized()
	var perp := Vector2(-dir.y, dir.x)
	_control = mid + perp * curve_amount

func _eval(t: float) -> Vector2:
	if abs(curve_amount) <= 0.001:
		return _start.lerp(_end, t)
	var q0 := _start.lerp(_control, t)
	var q1 := _control.lerp(_end, t)
	return q0.lerp(q1, t)
