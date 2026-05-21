using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace TH_Yuyuko.Scripts.VFX;

public sealed partial class YuyukoPointingHandsVfx : Node2D
{
	private const string _handTexturePath = "res://images/packed/vfx/combat/spooky_hand.png";
	private const string _additiveMatPath = "res://themes/canvas_item_material_additive_shared.tres";
	private const float _wristYMul = 0.86f;
	private const float _handScale = 0.58f;
	private const float _moveSeconds = 0.24f;
	private const float _fadeInSeconds = 0.10f;
	private const float _holdSeconds = 0.36f;
	private const float _fadeOutSeconds = 0.22f;

	private readonly List<Node2D> _handRoots = new();
	private Func<Vector2?>? _getTargetPos;
	private int _handCount;
	private Texture2D? _tex;
	private Material? _additiveMat;
	private float _life;

	public static YuyukoPointingHandsVfx? Create(Creature target, int extraHands)
	{
		if (NCombatRoom.Instance == null || NCombatRoom.Instance.CombatVfxContainer == null)
		{
			return null;
		}

		int handCount = Mathf.Clamp(1 + Math.Max(0, extraHands), 1, 12);
		var targetNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (targetNode == null)
		{
			return null;
		}

		var vfx = new YuyukoPointingHandsVfx();
		vfx._handCount = handCount;
		vfx._getTargetPos = () =>
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
			Vector2 p = node.Hitbox.GlobalPosition + node.Hitbox.Size * 0.5f;
			p.Y -= node.Hitbox.Size.Y * 0.18f;
			return p;
		};

		NCombatRoom.Instance.CombatVfxContainer.AddChild(vfx);
		return vfx;
	}

	public override void _Ready()
	{
		_tex = PreloadManager.Cache.GetTexture2D(_handTexturePath);
		_additiveMat = PreloadManager.Cache.GetMaterial(_additiveMatPath);
		if (_tex == null)
		{
			QueueFree();
			return;
		}

		ProcessPriority = 20;
		ZAsRelative = false;
		ZIndex = 90;
		SpawnHands();
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_life += dt;

		Vector2? targetPos = _getTargetPos?.Invoke();
		if (targetPos.HasValue)
		{
			for (int i = _handRoots.Count - 1; i >= 0; i--)
			{
				Node2D root = _handRoots[i];
				if (!GodotObject.IsInstanceValid(root))
				{
					_handRoots.RemoveAt(i);
					continue;
				}
				Vector2 toTarget = (targetPos.Value - root.GlobalPosition);
				if (toTarget.LengthSquared() > 0.001f)
				{
					root.Rotation = toTarget.Angle() - Mathf.Pi * 0.5f;
				}
			}
		}

		if (_life > _moveSeconds + _fadeInSeconds + _holdSeconds + _fadeOutSeconds + 0.35f)
		{
			QueueFree();
		}
	}

	private void SpawnHands()
	{
		if (_tex == null)
		{
			return;
		}

		Vector2? targetPos = _getTargetPos?.Invoke();
		if (!targetPos.HasValue)
		{
			QueueFree();
			return;
		}

		float w = _tex.GetWidth();
		float h = _tex.GetHeight();
		Vector2 wrist = new Vector2(w * 0.5f, h * _wristYMul);

		float baseAngle = Rng.Chaotic.NextFloat(0f, Mathf.Tau);
		for (int i = 0; i < _handCount; i++)
		{
			float a = baseAngle + Mathf.Tau * i / _handCount;
			Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

			float startRadius = Rng.Chaotic.NextFloat(500f, 620f);
			float endRadius = Rng.Chaotic.NextFloat(180f, 240f);

			Vector2 start = targetPos.Value + dir * startRadius;
			Vector2 end = targetPos.Value + dir * endRadius;

			Node2D handRoot = new Node2D
			{
				GlobalPosition = start
			};

			var sprite = new Sprite2D
			{
				Texture = _tex,
				Centered = false,
				Position = -wrist,
				Scale = Vector2.One * _handScale,
				Modulate = new Color(0.833333f, 0.5f, 1f, 0f),
				Material = _additiveMat
			};
			handRoot.AddChild(sprite);
			AddChild(handRoot);
			_handRoots.Add(handRoot);

			Vector2 toTarget = targetPos.Value - start;
			if (toTarget.LengthSquared() > 0.001f)
			{
				handRoot.Rotation = toTarget.Angle() - Mathf.Pi * 0.5f;
			}

			Tween moveTween = handRoot.CreateTween();
			moveTween.TweenProperty(handRoot, "global_position", end, _moveSeconds)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);

			Tween fadeTween = sprite.CreateTween();
			fadeTween.TweenProperty(sprite, "modulate:a", 1f, _fadeInSeconds)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);
			fadeTween.TweenInterval(_holdSeconds);
			fadeTween.TweenProperty(sprite, "modulate:a", 0f, _fadeOutSeconds)
				.SetEase(Tween.EaseType.In)
				.SetTrans(Tween.TransitionType.Sine);
			fadeTween.TweenCallback(Callable.From(() =>
			{
				if (GodotObject.IsInstanceValid(handRoot))
				{
					handRoot.QueueFree();
				}
			}));
		}
	}
}
