using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
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
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class BanquetPosthumousReunionPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];

		public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
		{
			if (dealer != base.Owner)
			{
				return;
			}
			if (!props.IsPoweredAttack())
			{
				return;
			}
			if (target.GetPowerAmount<DeathPower>() <= 0)
			{
				return;
			}
			if (result.UnblockedDamage <= 0)
			{
				return;
			}

			IEnumerable<Creature> others = base.CombatState.HittableEnemies.Where(e => e != target && e.IsAlive);
			if (!others.Any())
			{
				return;
			}
			Flash();
			decimal loss = (decimal)(result.TotalDamage+result.OverkillDamage);
			foreach (Creature other in others)
			{
				PlayGroundHandVfx(other);
			}
			await CreatureCmd.Damage(choiceContext, others, loss, ValueProp.Unblockable | ValueProp.Unpowered, null);
		}

		private static void PlayGroundHandVfx(Creature target)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null || target.IsDead)
			{
				return;
			}

			Node? container = NCombatRoom.Instance.CombatVfxContainer;
			if (container == null)
			{
				return;
			}

			var nCreature = NCombatRoom.Instance.GetCreatureNode(target);
			if (nCreature == null)
			{
				return;
			}

			Texture2D tex = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/spooky_hand.png");
			if (tex == null)
			{
				return;
			}

			Vector2 spawn = nCreature.GetBottomOfHitbox();
			float w = tex.GetWidth();
			float h = tex.GetHeight();
			Vector2 wrist = new Vector2(w * 0.5f, h * 0.86f);

			Vector2 start = spawn + new Vector2(0f, 240f);
			Vector2 emerge = spawn + new Vector2(0f, -nCreature.Hitbox.Size.Y * 0.10f);
			Vector2 end = emerge + new Vector2(0f, 10f);

			var root = new Node2D
			{
				GlobalPosition = start,
				Modulate = new Color(0.75f, 0.35f, 1f, 0f),
				ZAsRelative = false,
				ZIndex = 70
			};

			var sprite = new Sprite2D
			{
				Texture = tex,
				Centered = false,
				Position = -wrist,
				Scale = Vector2.One * 1f
			};

			root.AddChild(sprite);
			container.AddChildSafely(root);

			const float emergeSeconds = 0.18f;
			const float fadeInSeconds = 0.12f;
			const float slowMul = 2f;
			const float holdSeconds = 0.18f;
			const float fadeOutSeconds = 0.22f;

			Tween tween = root.CreateTween();
			tween.SetParallel();
			tween.TweenProperty(root, "global_position", emerge, emergeSeconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(root, "modulate:a", 1f, fadeInSeconds).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

			tween.Chain().TweenInterval(holdSeconds * slowMul);

			tween.SetParallel();
			tween.TweenProperty(root, "global_position", end, fadeOutSeconds * slowMul).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			tween.TweenProperty(root, "modulate:a", 0f, fadeOutSeconds * slowMul).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
			tween.Chain().TweenCallback(Callable.From(root.QueueFreeSafely));
		}
	}
}
