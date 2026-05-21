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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostMelodyFakeSpirit : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		public GhostMelodyFakeSpirit() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (PileType.Hand.GetPile(base.Owner).Cards.Count == 0)
			{
				return;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1, 1)
			{
				Cancelable = true
			};
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			CardModel? selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).FirstOrDefault();
			if (selected == null)
			{
				return;
			}

			for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
			{
				CardModel copy = selected.CreateClone();
				copy.AddKeyword(CardKeyword.Ethereal);
				await CardPileCmd.Add(copy, PileType.Hand);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
