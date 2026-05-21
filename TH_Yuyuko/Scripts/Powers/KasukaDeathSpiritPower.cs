using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class KasukaDeathSpiritPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this), HoverTipFactory.FromPower<DeathPower>()];

		public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (applier != base.Owner || base.Owner?.Player == null)
			{
				return;
			}

			if (amount <= 0m || power is not DeathPower || power.Owner.Side == base.Owner.Side)
			{
				return;
			}
			PlayDeathOrbVfx(power.Owner, base.Owner);
			Flash();
			await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);
		}

		private static void PlayDeathOrbVfx(Creature from, Creature to)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || from.IsDead || to.IsDead)
			{
				return;
			}

			Node? container = NCombatRoom.Instance.CombatVfxContainer;
			if (container == null)
			{
				return;
			}

			var fromNode = NCombatRoom.Instance.GetCreatureNode(from);
			var toNode = NCombatRoom.Instance.GetCreatureNode(to);
			if (fromNode == null || toNode == null)
			{
				return;
			}

			Texture2D tex = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/energy_orb_shine.png");
			if (tex == null)
			{
				return;
			}

			Material additive = ResourceLoader.Load<Material>("res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres")
				?? ResourceLoader.Load<Material>("res://themes/canvas_item_material_additive_shared.tres");

			Vector2 start = fromNode.Hitbox.GlobalPosition + fromNode.Hitbox.Size * 0.5f;
			start.Y -= fromNode.Hitbox.Size.Y * 0.18f;

			Vector2 end = toNode.Hitbox.GlobalPosition + toNode.Hitbox.Size * 0.5f;
			end.Y -= toNode.Hitbox.Size.Y * 0.18f;

			var root = new Node2D
			{
				GlobalPosition = start,
				Modulate = new Color(0.35f, 0.06f, 0.48f, 0f),
				ZAsRelative = false,
				ZIndex = 95
			};

			var sprite = new Sprite2D
			{
				Texture = tex,
				Centered = true,
				Material = additive,
				Scale = Vector2.One * 0.12f
			};

			root.AddChild(sprite);
			container.AddChildSafely(root);

			Tween tween = root.CreateTween();
			tween.SetParallel();
			tween.TweenProperty(sprite, "scale", Vector2.One * 0.62f, 0.12).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(root, "modulate:a", 1f, 0.12).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.Chain().TweenInterval(0.14);

			tween.SetParallel();
			tween.TweenProperty(root, "global_position", end, 0.35).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(sprite, "scale", Vector2.One * 0.14f, 0.35).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			tween.TweenProperty(root, "modulate:a", 0f, 0.35).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			tween.TweenCallback(Callable.From(root.QueueFreeSafely));
		}
	}
}
