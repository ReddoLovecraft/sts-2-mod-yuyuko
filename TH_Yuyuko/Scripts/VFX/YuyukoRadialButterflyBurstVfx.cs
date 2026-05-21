using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace TH_Yuyuko.Scripts.VFX;

public sealed partial class YuyukoRadialButterflyBurstVfx : Node2D
{
	private sealed class Particle
	{
		public required Sprite2D Sprite { get; init; }
		public required Vector2 Direction { get; init; }
		public required float Speed { get; init; }
		public required float LifetimeSeconds { get; init; }
		public required float RotationSpeed { get; init; }
		public float AgeSeconds { get; set; }
		public float InitialAlpha { get; init; }
	}

	[Export] public float duration_seconds = 1.0f;
	[Export] public float spawn_rate = 180f;
	[Export] public int burst_size = 14;
	[Export] public float particle_lifetime_seconds = 0.88f;
	[Export] public float speed_min = 520f;
	[Export] public float speed_max = 940f;
	[Export] public float start_radius = 18f;
	[Export] public float scale_min = 0.44f;
	[Export] public float scale_max = 0.78f;
	[Export] public float hitbox_center_y_offset_mul = 0.12f;
	[Export] public Color tint = new Color(1f, 1f, 1f, 0.95f);

	private const string _deathButterflyTexPath = "res://TH_Yuyuko/Artworks/VFX/bulletAa000.png";
	private const string _additiveMaterialPath = "res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres";

	private readonly List<Particle> _particles = new();
	private Texture2D? _texture;
	private Material? _additiveMat;
	private float _elapsed;
	private float _spawnAcc;
	private System.Func<Vector2?>? _getCenter;
	private string? _texturePathOverride;
	private Color? _tintOverride;

	public static YuyukoRadialButterflyBurstVfx? PlayOnCreature(Creature target, float durationSeconds)
	{
		return PlayOnCreature(target, durationSeconds, null, null);
	}

	public static YuyukoRadialButterflyBurstVfx? PlayOnCreature(Creature target, float durationSeconds, string? texturePath, Color? tintOverride)
	{
		Node? container = NCombatRoom.Instance?.CombatVfxContainer;
		if (container == null || NCombatRoom.Instance == null)
		{
			return null;
		}

		var creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}

		var vfx = new YuyukoRadialButterflyBurstVfx();
		vfx.duration_seconds = Mathf.Max(0.01f, durationSeconds);
		vfx._texturePathOverride = texturePath;
		vfx._tintOverride = tintOverride;
		vfx.GlobalPosition = GetCenter(creatureNode, vfx.hitbox_center_y_offset_mul);
		vfx._getCenter = () =>
		{
			if (NCombatRoom.Instance == null)
			{
				return null;
			}
			var node = NCombatRoom.Instance.GetCreatureNode(target);
			if (node == null)
			{
				return null;
			}
			return GetCenter(node, vfx.hitbox_center_y_offset_mul);
		};

		container.AddChild(vfx);
		return vfx;
	}

	public override void _Ready()
	{
		if (_tintOverride.HasValue)
		{
			tint = _tintOverride.Value;
		}
		_texture = ResourceLoader.Load<Texture2D>(_texturePathOverride ?? _deathButterflyTexPath);
		_additiveMat = ResourceLoader.Load<Material>(_additiveMaterialPath);
		if (_texture == null)
		{
			QueueFree();
			return;
		}

		ProcessPriority = 20;
		ZAsRelative = false;
		ZIndex = 90;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		if (_getCenter != null)
		{
			Vector2? p = _getCenter.Invoke();
			if (p.HasValue)
			{
				GlobalPosition = p.Value;
			}
		}

		_elapsed += dt;
		if (_elapsed <= duration_seconds)
		{
			_spawnAcc += spawn_rate * dt;
			int burst = Mathf.Max(1, burst_size);
			while (_spawnAcc >= burst)
			{
				_spawnAcc -= burst;
				SpawnBurst(burst);
			}
		}

		for (int i = _particles.Count - 1; i >= 0; i--)
		{
			Particle p = _particles[i];
			p.AgeSeconds += dt;
			if (!GodotObject.IsInstanceValid(p.Sprite) || p.AgeSeconds >= p.LifetimeSeconds)
			{
				p.Sprite?.QueueFree();
				_particles.RemoveAt(i);
				continue;
			}

			p.Sprite.Position += p.Direction * (p.Speed * dt);
			p.Sprite.Rotation += p.RotationSpeed * dt;

			float t = Mathf.Clamp(p.AgeSeconds / p.LifetimeSeconds, 0f, 1f);
			float fadeIn = Mathf.Clamp(t / 0.12f, 0f, 1f);
			float fadeOut = 1f - Mathf.Clamp((t - 0.75f) / 0.25f, 0f, 1f);
			float a = p.InitialAlpha * fadeIn * fadeOut;

			Color m = p.Sprite.Modulate;
			m.A = a;
			p.Sprite.Modulate = m;
		}

		if (_elapsed > duration_seconds + particle_lifetime_seconds && _particles.Count == 0)
		{
			QueueFree();
		}
	}

	private void SpawnBurst(int count)
	{
		if (_texture == null)
		{
			return;
		}

		float baseAngle = Rng.Chaotic.NextFloat(0f, Mathf.Tau);
		float step = Mathf.Tau / Mathf.Max(1, count);
		for (int i = 0; i < count; i++)
		{
			float angle = baseAngle + step * i + Rng.Chaotic.NextFloat(-step * 0.08f, step * 0.08f);
			Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
			float speed = Rng.Chaotic.NextFloat(speed_min, speed_max);
			float scale = Rng.Chaotic.NextFloat(scale_min, scale_max);
			float rotSpeed = Rng.Chaotic.NextFloat(-6.2f, 6.2f);
			float life = Rng.Chaotic.NextFloat(particle_lifetime_seconds * 0.75f, particle_lifetime_seconds * 1.15f);

			var sprite = new Sprite2D
			{
				Texture = _texture,
				Centered = true,
				Material = _additiveMat,
				Position = dir * start_radius,
				Scale = Vector2.One * scale,
				Rotation = Rng.Chaotic.NextFloat(-Mathf.Pi, Mathf.Pi),
				ZAsRelative = false,
				ZIndex = 0,
				Modulate = new Color(tint.R, tint.G, tint.B, 0f)
			};
			AddChild(sprite);

			_particles.Add(new Particle
			{
				Sprite = sprite,
				Direction = dir,
				Speed = speed,
				LifetimeSeconds = Mathf.Max(0.05f, life),
				RotationSpeed = rotSpeed,
				InitialAlpha = tint.A
			});
		}
	}

	private static Vector2 GetCenter(MegaCrit.Sts2.Core.Nodes.Combat.NCreature node, float offsetMul)
	{
		Vector2 c = node.Hitbox.GlobalPosition + node.Hitbox.Size * 0.5f;
		c.Y -= node.Hitbox.Size.Y * offsetMul;
		return c;
	}
}
