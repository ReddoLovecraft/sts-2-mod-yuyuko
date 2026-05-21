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
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SaigyouNihonZakura : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public SaigyouNihonZakura() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			if (handCount == 0)
			{
				return;
			}

			CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, handCount)
			{
				Cancelable = true
			};
			List<CardModel> selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).ToList();
			foreach (CardModel card in selected)
			{
				await CardCmd.TransformTo<Sakura>(card);
			}
		}

		protected override void OnUpgrade()
		{
			this.RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}
