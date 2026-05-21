using Godot;

namespace TH_Yuyuko.Scripts.VFX
{
[Tool]
public partial class YuyukoFlyToTarget : Node2D
{
	[Export] public bool Enabled { get; set; } = true;
	[Export] public bool AutoStart { get; set; } = true;
	[Export] public Vector2 StartOffset { get; set; } = new Vector2(-120f, 0f);
	[Export] public float Speed { get; set; } = 1200f;
	[Export] public float CurveAmount { get; set; } = 0f;
	[Export] public float AutoFreeSeconds { get; set; } = 0.6f;
	[Export] public bool PreviewInEditor { get; set; } = true;
	[Export] public Vector2 EditorPreviewTargetOffset { get; set; } = new Vector2(240f, 0f);
	[Export] public bool LoopInEditor { get; set; } = true;

	private Vector2 _start;
	private Vector2 _target;
	private Vector2 _control;
	private float _t;
	private float _duration;
	private float _life;
	private bool _finished;

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
		}
	}

	public override void _Ready()
	{
		if (!Enabled || !AutoStart)
		{
			return;
		}

		if (Engine.IsEditorHint() && PreviewInEditor)
		{
			_start = GlobalPosition;
			_target = _start + EditorPreviewTargetOffset;
		}
		else
		{
			_target = GlobalPosition;
			_start = _target + StartOffset;
			GlobalPosition = _start;
		}

		float dist = _start.DistanceTo(_target);
		_duration = (Speed > 0.001f) ? (dist / Speed) : 0f;
		_duration = Mathf.Max(_duration, 0.001f);

		Vector2 mid = (_start + _target) * 0.5f;
		Vector2 dir = (_target - _start);
		if (dir.LengthSquared() > 0.0001f)
		{
			dir = dir.Normalized();
		}
		Vector2 perp = new Vector2(-dir.Y, dir.X);
		_control = mid + perp * CurveAmount;
	}

	public override void _Process(double delta)
	{
		if (!Enabled || !AutoStart)
		{
			return;
		}

		float dt = (float)delta;
		_life += dt;

		if (!_finished)
		{
			_t += dt / _duration;
			if (_t >= 1f)
			{
				_t = 1f;
				_finished = true;
			}

			if (Mathf.Abs(CurveAmount) <= 0.001f)
			{
				GlobalPosition = _start.Lerp(_target, _t);
			}
			else
			{
				Vector2 q0 = _start.Lerp(_control, _t);
				Vector2 q1 = _control.Lerp(_target, _t);
				GlobalPosition = q0.Lerp(q1, _t);
			}
		}

		if (Engine.IsEditorHint() && PreviewInEditor && _finished && LoopInEditor)
		{
			_t = 0f;
			_life = 0f;
			_finished = false;
			GlobalPosition = _start;
			return;
		}

		if (!Engine.IsEditorHint() && AutoFreeSeconds > 0f && _finished && _life >= AutoFreeSeconds)
		{
			QueueFree();
		}
	}
}
}
