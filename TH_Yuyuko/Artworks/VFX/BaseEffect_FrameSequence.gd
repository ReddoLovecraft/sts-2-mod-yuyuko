@tool
extends Node2D

@export var frames_per_second: float = 12.0
@export var loop: bool = true
@export var play_in_editor: bool = true

var _frames: Array[CanvasItem] = []
var _t: float = 0.0
var _index: int = 0
var _last_child_count: int = -1

func _enter_tree() -> void:
	if Engine.is_editor_hint():
		set_process(true)

func _ready() -> void:
	_refresh()

func _process(delta: float) -> void:
	if Engine.is_editor_hint() and not play_in_editor:
		return

	if get_child_count() != _last_child_count:
		_refresh()

	if _frames.size() <= 1:
		return

	if frames_per_second <= 0.001:
		return

	_t += delta
	var frame_time := 1.0 / frames_per_second
	while _t >= frame_time:
		_t -= frame_time
		_set_index(_index + 1)

func _refresh() -> void:
	_last_child_count = get_child_count()
	_frames.clear()
	for child in get_children():
		var item := child as CanvasItem
		if item != null:
			_frames.append(item)
	_index = 0
	_t = 0.0
	_apply_visibility()

func _set_index(i: int) -> void:
	if _frames.size() == 0:
		return

	if i >= _frames.size():
		if loop:
			_index = 0
		else:
			_index = _frames.size() - 1
	else:
		_index = i

	_apply_visibility()

func _apply_visibility() -> void:
	for i in range(_frames.size()):
		_frames[i].visible = (i == _index)
