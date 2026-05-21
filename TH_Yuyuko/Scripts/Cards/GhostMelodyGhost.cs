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
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostMelodyGhost : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2),new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.ForEnergy(this),
			HoverTipFactory.FromPower<EnergyNextTurnPower>(),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
		];

		public GhostMelodyGhost() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (handCount <= 0)
			{
				return;
			}
			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.DynamicVars.Cards.IntValue)
			{
				Cancelable = true
			};

			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).ToList();
			if (selected.Count == 0)
			{
				return;
			}

			foreach (CardModel card in selected)
			{
				await CardCmd.Discard(choiceContext, card);
			}
			await PlayerCmd.GainEnergy(selected.Count, base.Owner);
			await PowerCmd.Apply<EnergyNextTurnPower>(base.Owner.Creature, selected.Count, base.Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
