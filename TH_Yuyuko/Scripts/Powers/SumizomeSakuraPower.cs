using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class SumizomeSakuraPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>(), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (fromHandDraw || base.Owner?.Player == null || card.Owner != base.Owner.Player || card is not Sakura)
			{
				return;
			}
			Flash();
			for(int i=0;i<Amount;i++)
			{
			    CardModel clone = card.CreateClone();
				await CardPileCmd.Add(clone, PileType.Hand);
				CardCmd.Preview(clone);
			}
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (base.Owner?.Player == null || card.Owner != base.Owner.Player || card is not Sakura)
			{
				return;
			}
			Flash();
			for (int i = 0; i < Amount; i++)
			{
				CardModel clone = card.CreateClone();
				await CardPileCmd.Add(clone, PileType.Hand);
				CardCmd.Preview(clone);
			}
		}

		public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			if (base.Owner?.Player == null || card.Owner != base.Owner.Player || card is not Sakura || originalCost < 0m)
			{
				modifiedCost = originalCost;
				return false;
			}

			modifiedCost = originalCost + 1m;
			return (int)modifiedCost != (int)originalCost;
		}
	}
}
