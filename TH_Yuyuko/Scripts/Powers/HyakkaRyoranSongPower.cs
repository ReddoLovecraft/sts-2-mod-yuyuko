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
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class HyakkaRyoranSongPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
		{
			if (side != base.Owner.Side || !base.Owner.IsPlayer)
			{
				return;
			}

			int draw = base.Amount;
			if (draw > 0)
			{
				Flash();
				await CardPileCmd.Draw(choiceContext, draw, base.Owner.Player);
			}

			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner.Player, 1, combatState), PileType.Draw, addedByPlayer: true, position: CardPilePosition.Random));
		}
	}
}
