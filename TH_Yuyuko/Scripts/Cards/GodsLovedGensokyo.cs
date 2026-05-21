using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GodsLovedGensokyo : YuyukoCardModel
	{
		public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		public GodsLovedGensokyo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
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

				List<CardModel> canonicalLibraryCards = creature.Player.Character.CardPool.AllCards
					.Where(c => c.ShouldShowInCardLibrary && c.Type != CardType.Quest)
					.ToList();

				List<CardModel> selectableCards = canonicalLibraryCards
					.Select(c => base.CombatState.CreateCard(c, creature.Player))
					.ToList();

				CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
				CardModel? selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, selectableCards, creature.Player, prefs)).FirstOrDefault();
				if (selected != null)
				{
					await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, addedByPlayer: true);
				}
			}
		}

		protected override void OnUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}
