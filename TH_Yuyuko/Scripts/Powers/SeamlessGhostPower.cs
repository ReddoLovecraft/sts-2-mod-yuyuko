using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class SeamlessGhostPower : TH_Yuyuko.Scripts.Main.YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.Static(StaticHoverTip.Block),
			HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
		];

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Models.CardModel card, bool fromHandDraw)
		{
			if (card.Owner.Creature == base.Owner && card.Keywords.Contains(CardKeyword.Ethereal))
			{
				await TriggerBlock();
			}
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner.Creature == base.Owner && cardPlay.Card.Keywords.Contains(CardKeyword.Ethereal))
			{
				await TriggerBlock();
			}
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Models.CardModel card, bool causedByEthereal)
		{
			if (card.Owner.Creature == base.Owner && card.Keywords.Contains(CardKeyword.Ethereal))
			{
				await TriggerBlock();
			}
		}

		private async Task TriggerBlock()
		{
			if (base.Amount <= 0)
			{
				return;
			}
			Flash();
			await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null, fast: true);
		}
	}
}
