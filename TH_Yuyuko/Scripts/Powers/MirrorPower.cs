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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;


namespace TH_Yuyuko.Scrpits.Powers
{
    public sealed class MirrorPower :YuyukoPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;
        //public override string? CustomPackedIconPath => "res://TH_Yuyuko/Artworks/Powers/DP32.png";
        //public override string? CustomBigIconPath => "res://TH_Yuyuko/Artworks/Powers/DP64.png";
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DeathPower>()];
        public MirrorPower() { }
      public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && dealer != null && props.IsPoweredAttack())
		{
			YuyukoVfxManager.PlaySimple("mirror",Owner,VfxTargetPositionType.TargetCreature);
			this.Flash();
			await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies, base.Amount, base.Owner,null);
		}
	}

	public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (base.Owner.Side != side)
		{
			await PowerCmd.Remove(this);
		}
	}
    }
}
