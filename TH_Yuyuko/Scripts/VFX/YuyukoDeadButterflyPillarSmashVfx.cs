using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace TH_Yuyuko.Scripts.VFX;

public partial class YuyukoDeadButterflyPillarSmashVfx : Node2D
{
	[Export] public float fall_height { get; set; } = 750f;
	[Export] public float fall_seconds { get; set; } = 0.24f;
	[Export] public float impact_pause_seconds { get; set; } = 0.03f;
	[Export] public float butterfly_fly_seconds { get; set; } = 0.45f;
	[Export] public float butterfly_distance { get; set; } = 160f;
	[Export] public float duration_seconds { get; set; } = 0.9f;

	private static readonly Vector2[] _directionOptions =
	[
		new Vector2(0.1f, -1f).Normalized(),
		new Vector2(0.75f, -0.75f).Normalized(),
		new Vector2(1f, -0.1f).Normalized(),
		new Vector2(0.75f, 0.75f).Normalized(),
		new Vector2(-0.1f, 1f).Normalized(),
		new Vector2(-0.75f, 0.75f).Normalized(),
		new Vector2(-1f, 0.1f).Normalized(),
		new Vector2(-0.75f, -0.75f).Normalized()
	];

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private async Task PlaySequence()
	{
		Vector2 landing = GlobalPosition;
		GlobalPosition = landing + Vector2.Up * fall_height;

		AnimatedSprite2D? pillar = GetNodeOrNull<AnimatedSprite2D>("Pillar");
		if (pillar != null)
		{
			float minScale = Mathf.Min(pillar.Scale.X, pillar.Scale.Y);
			float factor = minScale < 1f ? (1f / Mathf.Max(0.0001f, minScale)) : 2f;
			pillar.Scale *= factor;

			Texture2D? tex = pillar.SpriteFrames?.GetFrameTexture(pillar.Animation, 0);
			if (tex != null)
			{
				pillar.Position = new Vector2(0f, -tex.GetHeight() * 0.5f * pillar.Scale.Y);
			}
		}

		Tween fallTween = CreateTween();
		fallTween.TweenProperty(this, "global_position", landing, Mathf.Max(0.01f, fall_seconds))
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.In);
		await ToSignal(fallTween, Tween.SignalName.Finished);

		await WaitSeconds(impact_pause_seconds);

		Node2D butterfliesRoot = GetNodeOrNull<Node2D>("Butterflies") ?? this;
		SpawnButterflies(butterfliesRoot, landing);

		if (pillar != null)
		{
			Tween fade = pillar.CreateTween();
			fade.TweenInterval(0.08);
			fade.TweenProperty(pillar, "modulate:a", 0f, 0.22)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);
		}

		await WaitSeconds(duration_seconds);
		if (GodotObject.IsInstanceValid(this))
		{
			QueueFree();
		}
	}

	private void SpawnButterflies(Node2D parent, Vector2 landingGlobalPosition)
	{
		Texture2D tex = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/bulletAa000.png", null, ResourceLoader.CacheMode.Reuse);
		if (tex == null)
		{
			return;
		}

		Material additive = ResourceLoader.Load<Material>("res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres")
			?? ResourceLoader.Load<Material>("res://themes/canvas_item_material_additive_shared.tres");

		var rng = new RandomNumberGenerator();
		rng.Randomize();

		var chosen = new HashSet<int>();
		for (int i = 0; i < 4; i++)
		{
			int idx;
			int guard = 0;
			do
			{
				idx = rng.RandiRange(0, _directionOptions.Length - 1);
				guard++;
			}
			while (chosen.Contains(idx) && guard < 32);
			chosen.Add(idx);

			Vector2 dir = _directionOptions[idx];
			float dist = butterfly_distance * rng.RandfRange(0.85f, 1.2f);

			var sprite = new Sprite2D
			{
				Texture = tex,
				Centered = true,
				Material = additive,
				SelfModulate = new Color(0.93f, 0.25f, 0.62f, 0.95f),
				Scale = Vector2.One * rng.RandfRange(0.24f, 0.32f),
				GlobalPosition = landingGlobalPosition + new Vector2(rng.RandfRange(-10f, 10f), rng.RandfRange(-10f, 10f)) + Vector2.Up * 8f,
				Rotation = rng.RandfRange(-0.6f, 0.6f)
			};
			parent.AddChildSafely(sprite);

			Vector2 end = sprite.GlobalPosition + dir * dist;
			float seconds = Mathf.Max(0.08f, butterfly_fly_seconds * rng.RandfRange(0.85f, 1.15f));

			Tween t = sprite.CreateTween();
			t.TweenProperty(sprite, "global_position", end, seconds)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			t.Parallel().TweenProperty(sprite, "rotation", sprite.Rotation + rng.RandfRange(-1.5f, 1.5f), seconds);
			t.Parallel().TweenProperty(sprite, "modulate:a", 0f, seconds)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.In);
			t.TweenCallback(Callable.From(sprite.QueueFreeSafely));
		}
	}

	private async Task WaitSeconds(float seconds)
	{
		if (seconds <= 0.001f || !IsInsideTree())
		{
			return;
		}
		SceneTreeTimer timer = GetTree().CreateTimer(seconds);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
	}
}
