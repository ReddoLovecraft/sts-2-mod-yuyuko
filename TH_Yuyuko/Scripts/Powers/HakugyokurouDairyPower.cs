using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using TH_Yuyuko.Scripts.Cards;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class HakugyokurouDairyPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<YuyukoMilk>(), HoverTipFactory.FromCard<YoumuJuice>()];

		public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side != base.Owner.Side || base.Owner?.Player == null)
			{
				return;
			}
			Flash();
			for (int i = 0; i < base.Amount; i++)
			{
				CardModel milk = combatState.CreateCard(ModelDb.Card<YuyukoMilk>(), base.Owner.Player);
				CardModel juice = combatState.CreateCard(ModelDb.Card<YoumuJuice>(), base.Owner.Player);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(new[] { milk, juice }, PileType.Hand, addedByPlayer: true));
			}
		}
	}
}
