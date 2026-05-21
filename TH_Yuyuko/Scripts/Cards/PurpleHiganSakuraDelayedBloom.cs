using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class PurpleHiganSakuraDelayedBloom : YuyukoCardModel
	{
		public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		public PurpleHiganSakuraDelayedBloom() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			IEnumerable<Creature> players = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
				where c != null && c.IsAlive && c.IsPlayer
				select c;

			foreach (Creature creature in players)
			{
				if (creature.Player == null)
				{
					continue;
				}

				foreach (Sakura sakura in Sakura.Create(creature.Player, base.DynamicVars.Cards.IntValue, base.CombatState))
				{
					await CardPileCmd.AddGeneratedCardToCombat(sakura, PileType.Hand, addedByPlayer: true);
				}

				var copy = (PurpleHiganSakuraDelayedBloom)base.CombatState.CreateCard(ModelDb.Card<PurpleHiganSakuraDelayedBloom>(), creature.Player);
				if (base.IsUpgraded)
				{
					CardCmd.Upgrade(copy);
				}
				await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, addedByPlayer: true);
				CardCmd.Preview(copy);
			}
		}

		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}
