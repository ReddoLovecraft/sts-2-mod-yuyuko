using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class GhostDanceOukaRanmanPower : TH_Yuyuko.Scripts.Main.YuyukoPowerModel
	{
		private const string PetalTexturePath = "res://TH_Yuyuko/Artworks/VFX/花瓣(petal)_爱给网_aigei_com.png";
		private static Texture2D? _petalTexture;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<SpringPower>()
		];

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Models.CardModel card, bool fromHandDraw)
		{
			if (fromHandDraw)
			{
				return;
			}
			if (card.Owner != base.Owner.Player)
			{
				return;
			}
			if (base.Amount <= 0)
			{
				return;
			}

			Flash();
			SpawnPetalVfx(base.Amount * 8);
			await PowerCmd.Apply<SpringPower>(base.Owner, base.Amount, base.Owner, null);
		}

		public static void SpawnPetalVfx(int petalCount)
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null)
			{
				return;
			}

			if (petalCount <= 0)
			{
				return;
			}

			Node parent = NCombatRoom.Instance.CombatVfxContainer;
			Viewport viewport = parent.GetViewport();
			Vector2 size = viewport.GetVisibleRect().Size;

			if (_petalTexture == null)
			{
				_petalTexture = ResourceLoader.Load<Texture2D>(PetalTexturePath, null, ResourceLoader.CacheMode.Reuse);
				if (_petalTexture == null)
				{
					return;
				}
			}

			for (int i = 0; i < petalCount; i++)
			{
				float startX = GD.Randf() * size.X;
				float startY = -Mathf.Lerp(40f, 260f, GD.Randf());
				float endY = size.Y + 260f;
				float endX = startX + Mathf.Lerp(-size.X * 0.35f, size.X * 0.35f, GD.Randf());

				float fallSeconds = Mathf.Lerp(2.8f, 4.8f, GD.Randf());
				float scale = Mathf.Lerp(0.26f, 0.52f, GD.Randf());
				float startRot = Mathf.Lerp(-Mathf.Pi, Mathf.Pi, GD.Randf());
				float rotDelta = Mathf.Lerp(-3.2f, 3.2f, GD.Randf());
				float waveAmp = Mathf.Lerp(18f, 85f, GD.Randf());
				float waves = Mathf.Lerp(0.75f, 2.0f, GD.Randf());

				CombatState? combatState = CombatManager.Instance.DebugOnlyGetState();
				bool isSumizomeActive = combatState?.Players.Any(p => p.Creature.HasPower<SumizomeSakuraPower>()) ?? false;
				float rgb = isSumizomeActive ? 0.55f : 1f;
				var startColor = new Color(rgb, rgb, rgb, 1f);
				var endColor = new Color(rgb, rgb, rgb, 0f);

				var petal = new Sprite2D
				{
					Texture = _petalTexture,
					Centered = true,
					GlobalPosition = new Vector2(startX, startY),
					Scale = Vector2.One * scale,
					Rotation = startRot,
					Modulate = startColor,
					ZIndex = 5
				};
				parent.AddChildSafely(petal);

				Vector2 start = petal.GlobalPosition;
				Vector2 end = new Vector2(endX, endY);

				var tween = petal.CreateTween();
				tween.TweenMethod(
					Callable.From<float>(t =>
					{
						float baseT = Mathf.Clamp(t, 0f, 1f);
						Vector2 pos = start.Lerp(end, baseT);
						pos.X += Mathf.Sin(baseT * Mathf.Pi * 2f * waves) * waveAmp;
						petal.GlobalPosition = pos;
					}),
					0f,
					1f,
					fallSeconds
				).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
				tween.Parallel().TweenProperty(petal, "rotation", startRot + rotDelta, fallSeconds);
				tween.Parallel().TweenProperty(petal, "modulate", endColor, fallSeconds * 0.35f).SetDelay(fallSeconds * 0.65f);
				tween.TweenCallback(Callable.From(petal.QueueFreeSafely));
			}
		}
	}
}
