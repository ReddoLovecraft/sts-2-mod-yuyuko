using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostMelodyIllusionSpirit : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		public GhostMelodyIllusionSpirit() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (handCount == 0)
			{
				return;
			}
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, handCount)
			{
				Cancelable = true
			};
			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).ToList();
			if (selected.Count == 0)
			{
				return;
			}
            foreach(CardModel card in selected)
			{
				card.EnergyCost.SetUntilPlayed(0);
				if (!card.Keywords.Contains(CardKeyword.Ethereal))
				{
					card.AddKeyword(CardKeyword.Ethereal);
				}
				await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
