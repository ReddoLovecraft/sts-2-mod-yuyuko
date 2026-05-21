using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scrpits.Powers
{
	public sealed class SpiritButterflyButterflyWindPower : YuyukoPowerModel
	{
		public override PowerType Type => PowerType.Buff;
		public override PowerStackType StackType => PowerStackType.Counter;
		public override Godot.Color AmountLabelColor => PowerModel._normalAmountLabelColor;

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromKeyword(CardKeyword.Retain),
			HoverTipFactory.ForEnergy(this)
		];
		public override bool ShouldPlayerResetEnergy(Player player)
		{
			if (player != base.Owner.Player)
			{
				return true;
			}
			return false;
		}
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
		  public override bool ShouldFlush(Player player)
	    {
		if (player != base.Owner.Player)
		{
			return true;
		}
		return false;
		}
		 public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner.Player)
            {
                return;
            }
            Flash();
            await PowerCmd.Decrement(this);
        }
	}
}
