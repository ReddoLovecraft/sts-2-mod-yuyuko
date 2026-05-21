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

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class GorgeousDespairDancerPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (base.Owner?.Player == null || card.Owner != base.Owner.Player || base.Amount <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies, base.Amount, base.Owner, null);
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (base.Owner?.Player == null || cardPlay.Card.Owner != base.Owner.Player || base.Amount <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies, base.Amount, base.Owner, null);
		}
	}
}
