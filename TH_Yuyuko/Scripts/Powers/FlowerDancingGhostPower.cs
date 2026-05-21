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
	public sealed class FlowerDancingGhostPower : TH_Yuyuko.Scripts.Main.YuyukoPowerModel
	{
		private const int SnowFrameCount = 150;
		private const float SnowFps = 24f;

		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.ForEnergy(this),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
		];

		private static SpriteFrames? _snowFrames;
		private AnimatedSprite2D? _snowSprite;

		public override Task AfterApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
		{
			TryStartSnowOverlay();
			return Task.CompletedTask;
		}

		public override Task AfterRemoved(MegaCrit.Sts2.Core.Entities.Creatures.Creature oldOwner)
		{
			StopSnowOverlay();
			return Task.CompletedTask;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != base.Owner.Player)
			{
				return;
			}
			if (!cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal))
			{
				return;
			}
			if ( base.Owner.Player == null)
			{
				return;
			}

			Flash();
			await CardPileCmd.Draw(context, base.Amount, base.Owner.Player);
			await PlayerCmd.GainEnergy(base.Amount, base.Owner.Player);
		}

		private void TryStartSnowOverlay()
		{
			if (TestMode.IsOn || NCombatRoom.Instance == null)
			{
				return;
			}
			if (_snowSprite != null && GodotObject.IsInstanceValid(_snowSprite))
			{
				return;
			}

			if (_snowFrames == null)
			{
				SpriteFrames frames = new SpriteFrames();
				if (!frames.HasAnimation("default"))
				{
					frames.AddAnimation("default");
				}
				frames.SetAnimationLoop("default", true);
				frames.SetAnimationSpeed("default", SnowFps);

				for (int i = 0; i < SnowFrameCount; i++)
				{
					string path = $"res://TH_Yuyuko/Artworks/VFX/snow_frames/frame_{i:D3}.png";
					Texture2D frameTex = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
					if (frameTex == null)
					{
						return;
					}
					frames.AddFrame("default", frameTex);
				}

				_snowFrames = frames;
			}

			Material additive = ResourceLoader.Load<Material>("res://TH_Yuyuko/Artworks/VFX/canvas_item_material_additive_shared.tres", null, ResourceLoader.CacheMode.Reuse);

			Node parent = NCombatRoom.Instance.CombatVfxContainer;
			Viewport viewport = parent.GetViewport();
			Vector2 viewSize = viewport.GetVisibleRect().Size;

			Texture2D first = _snowFrames.GetFrameTexture("default", 0);
			float texW = Mathf.Max(1f, first.GetWidth());
			float texH = Mathf.Max(1f, first.GetHeight());
			float scale = Mathf.Max(viewSize.X / texW, viewSize.Y / texH);

			_snowSprite = new AnimatedSprite2D
			{
				SpriteFrames = _snowFrames,
				ZAsRelative = false,
				ZIndex = 800,
				Position = viewSize * 0.5f,
				Scale = Vector2.One * scale,
				Material = additive,
				Modulate = new Color(1f, 1f, 1f, 0.9f)
			};
			parent.AddChildSafely(_snowSprite);
			_snowSprite.Play("default");
		}

		private void StopSnowOverlay()
		{
			if (_snowSprite != null && GodotObject.IsInstanceValid(_snowSprite))
			{
				_snowSprite.QueueFreeSafely();
			}
			_snowSprite = null;
		}
	}
}
