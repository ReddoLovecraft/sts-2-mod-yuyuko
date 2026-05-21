using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
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
	public sealed class SnowDeathSpiritPower : TH_Yuyuko.Scripts.Main.YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Single;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (!causedByEthereal)
			{
				return;
			}
			if (card.Owner != base.Owner.Player)
			{
				return;
			}

			Flash();
			TryPlayFrostScreenVfx();
			await CardPileCmd.Add(card, PileType.Play);
			await CardCmd.AutoPlay(choiceContext, card, null);
		}

		private static void TryPlayFrostScreenVfx()
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null)
			{
				return;
			}

			Texture2D tex = ResourceLoader.Load<Texture2D>("res://TH_Yuyuko/Artworks/VFX/FrostEffect/Ice.tga", null, ResourceLoader.CacheMode.Reuse);
			if (tex == null)
			{
				return;
			}

			Node parent = NCombatRoom.Instance.CombatVfxContainer;
			Viewport viewport = parent.GetViewport();
			Vector2 viewSize = viewport.GetVisibleRect().Size;

			float texW = Mathf.Max(1f, tex.GetWidth());
			float texH = Mathf.Max(1f, tex.GetHeight());
			float scale = Mathf.Max(viewSize.X / texW, viewSize.Y / texH);

			Sprite2D sprite = new Sprite2D
			{
				Texture = tex,
				Centered = true,
				ZAsRelative = false,
				ZIndex = 999,
				Position = viewSize * 0.5f,
				Scale = Vector2.One * scale,
				Rotation = GD.Randf() * 0.24f - 0.12f,
				Modulate = new Color(0.92f, 0.96f, 1.0f, 0f)
			};
			parent.AddChildSafely(sprite);

			Tween tween = sprite.CreateTween();
			tween.TweenProperty(sprite, "modulate:a", 0.65f, 0.12).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
			tween.TweenInterval(0.22);
			tween.TweenProperty(sprite, "modulate:a", 0f, 0.38).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);
			tween.TweenCallback(Callable.From(sprite.QueueFreeSafely));
		}
	}
}
