using Godot;
using Godot.Collections;

namespace TH_Yuyuko.Scripts.VFX
{
[Tool]
public partial class YuyukoSpriteFramePlayer : Sprite2D
{
	[Export] public Array<Texture2D> Frames { get; set; } = new Array<Texture2D>();
	[Export] public float FramesPerSecond { get; set; } = 12f;
	[Export] public bool Loop { get; set; } = true;
	[Export] public bool PlayOnReady { get; set; } = true;
	[Export] public bool PreviewInEditor { get; set; } = true;

	private float _time;
	private int _index;
	private bool _playing;

	public override void _EnterTree()
	{
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
		}
	}

	public override void _Ready()
	{
		if (Frames.Count == 0 && Texture != null)
		{
			Frames.Add(Texture);
		}

		if (Frames.Count > 0 && Texture == null)
		{
			Texture = Frames[0];
		}

		_playing = PlayOnReady && Frames.Count > 1 && FramesPerSecond > 0.001f;
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint() && !PreviewInEditor)
		{
			return;
		}

		if (!_playing)
		{
			return;
		}

		_time += (float)delta;
		float frameTime = 1f / FramesPerSecond;
		while (_time >= frameTime)
		{
			_time -= frameTime;
			_index++;
			if (_index >= Frames.Count)
			{
				if (Loop)
				{
					_index = 0;
				}
				else
				{
					_index = Frames.Count - 1;
					_playing = false;
					break;
				}
			}
			Texture = Frames[_index];
		}
	}
}
}
