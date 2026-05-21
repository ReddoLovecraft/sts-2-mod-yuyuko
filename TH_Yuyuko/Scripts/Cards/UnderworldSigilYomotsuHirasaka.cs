using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class UnderworldSigilYomotsuHirasaka : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Unpowered)];
		protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<DeathDesirePower>()) ?? false;
		public UnderworldSigilYomotsuHirasaka() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			while (true)
			{
				CardModel? drawn = await CardPileCmd.Draw(choiceContext, base.Owner);
				if (drawn == null)
				{
					break;
				}

				Node? vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
				foreach (Creature mos in base.CombatState.HittableEnemies.ToList())
				{
					Node2D? vfx = YuyukoVfxManager.CreateProjectileToTarget("spirit", Owner.Creature, mos, new Vector2(0f, -180f), new Vector2(0f, -40f));
					if (vfx != null && vfxContainer != null)
					{
						vfxContainer.AddChildSafely(vfx);
					}
				}

				await Task.Delay(80);

				foreach (Creature mos in base.CombatState.HittableEnemies.ToList())
				{
					await CreatureCmd.Damage(choiceContext, mos, base.DynamicVars.Damage, this);
				}
				
				if (!drawn.Keywords.Contains(CardKeyword.Ethereal))
				{
					break;
				}
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(4m);
		}
	}
}
