using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;


namespace TH_Yuyuko.Scrpits.Powers
{
    public sealed class DeathDesirePower :YuyukoPowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/DDP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/DDP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];
        public DeathDesirePower() { }
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
        bool flag=Owner.GetPower<DeathPower>() != null;
        if(target==Owner)
        {
            if(flag)
                return 1.5m;
            else
                return 1.25m;
        }
        if(dealer==Owner)
        {
            if(flag)
                return 0.5m;
            else
                return 0.75m;
        }
		return 1m;
	}
        public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	    {
		if (side != base.Owner.Side)
		{
			return;
		}
        Flash();
		await PowerCmd.Decrement(this);
        }
    }

}