using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SoulTemptationSpiritEmergence : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(20)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DeathPower>(),
			HoverTipFactory.FromPower<IntangiblePower>()
		];
		protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<DeathPower>()&&e.GetPowerAmount<DeathPower>()>=DynamicVars.Cards.IntValue) ?? false;
		public SoulTemptationSpiritEmergence() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			int deathDesire = cardPlay.Target.GetPowerAmount<DeathPower>();
			if (deathDesire <= 0)
			{
				return;
			}

			await PowerCmd.Remove<DeathPower>(cardPlay.Target);

			int threshold = Math.Max(1, base.DynamicVars.Cards.IntValue);
			int intangible = deathDesire / threshold;
			if (intangible > 0)
			{
				Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
				if (vfxContainer != null)
				{
					for (int i = 0; i < intangible; i++)
					{
						Node2D? vfx = YuyukoVfxManager.CreateProjectileToTarget(
							"soulbody_effect",
							cardPlay.Target,
							base.Owner.Creature,
							new Vector2(0f, -40f),
							new Vector2(0f, -180f)
						);
						if (vfx != null)
						{
							float s = 1f + 0.10f * i;
							vfx.Scale = Vector2.One * s;
							vfxContainer.AddChildSafely(vfx);
						}
						await Task.Delay(30);
					}
				}

				await PowerCmd.Apply<IntangiblePower>(base.Owner.Creature, intangible, base.Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(-10);
		}
	}
}
