using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

public partial class NYuyukoLaserVfx : NLaserVfx
{
	private static readonly StringName _color = new StringName("Color");

	[Export] public float duration_seconds { get; set; } = 0.6f;

	private sealed record LaserPart(Node2D spineNode, MegaSprite controller, Node2D targetingBone, Vector2 localOffset);

	private readonly List<LaserPart> _parts = new();

	public override void _Ready()
	{
		TryAddPart("SpineSprite", Vector2.Zero);
		TryAddPart("SpineSprite2", Vector2.Zero);
		TryAddPart("SpineSprite3", Vector2.Zero);

		foreach (var p in _parts)
		{
			p.controller.GetAnimationState().SetAnimation("animation");
			p.spineNode.Visible = true;
		}

		InitializeDefaultPositions();

		if (duration_seconds > 0f)
		{
			var t = new Godot.Timer
			{
				OneShot = true,
				WaitTime = duration_seconds,
				Autostart = true
			};
			AddChild(t);
			t.Timeout += QueueFree;
		}
	}

	private void TryAddPart(string nodeName, Vector2 localOffset)
	{
		var spine = GetNodeOrNull<Node2D>(nodeName);
		if (spine == null)
		{
			return;
		}

		var bone = GetNodeOrNull<Node2D>($"{nodeName}/TargetingBone");
		if (bone == null)
		{
			return;
		}

		_parts.Add(new LaserPart(spine, new MegaSprite(spine), bone, localOffset));
	}

	private void InitializeDefaultPositions()
	{
		if (NCombatRoom.Instance == null)
		{
			return;
		}

		CombatState? state = CombatManager.Instance.DebugOnlyGetState();
		if (state == null || state.Players.Count == 0)
		{
			return;
		}

		Creature caster = state.Players[0].Creature;
		var casterNode = NCombatRoom.Instance.GetCreatureNode(caster);
		if (casterNode == null)
		{
			return;
		}

		Vector2 start = casterNode.VfxSpawnPosition + new Vector2(1150f, -200f);

		GlobalPosition = start;

		for (int i = 0; i < _parts.Count; i++)
		{
			var p = _parts[i];
			p.spineNode.Position = new Vector2(80f, p.spineNode.Position.Y);
		}
	}

	public new void ExtendLaser(Vector2 targetPos)
	{
		_ = targetPos;
		for (int i = 0; i < _parts.Count; i++)
		{
			var p = _parts[i];
			p.spineNode.Visible = true;
			p.controller.GetAnimationState().SetAnimation("animation");
		}
	}

	private void SetLaserColor(Color color)
	{
		for (int i = 0; i < _parts.Count; i++)
		{
			var p = _parts[i];
			if (p.controller.GetAdditiveMaterial() is ShaderMaterial mat)
			{
				mat.SetShaderParameter(_color, color);
			}
		}
	}
}
