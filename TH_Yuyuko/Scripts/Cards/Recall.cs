using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(CurseCardPool))]
	public sealed class Recall : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Eternal];
		public override bool CanBeGeneratedInCombat => false;
		public override int MaxUpgradeLevel => 0;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		public Recall() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, showInCardLibrary: false)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (card != this)
			{
				return;
			}

			await Cmd.Wait(0.1f);

			CardPile hand = PileType.Hand.GetPile(base.Owner);
			foreach (CardModel c in hand.Cards)
			{
				if (!c.Keywords.Contains(CardKeyword.Ethereal))
				{
					c.AddKeyword(CardKeyword.Ethereal);
				}
			}

			PlayerCmd.EndTurn(base.Owner, canBackOut: false);
		}
	}
}
