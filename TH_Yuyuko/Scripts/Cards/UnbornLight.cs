using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class UnbornLight : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(7)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DeathPower>()
    });
	protected override bool ShouldGlowGoldInternal => base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<DeathPower>())  ?? false;
	public UnbornLight() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		YuyukoVfxManager.PlaySimple("laser_vfx",cardPlay.Target,VfxTargetPositionType.TargetSide);
		await CreatureCmd.Damage(choiceContext,base.CombatState.HittableEnemies,new DamageVar(base.DynamicVars.HpLoss.IntValue,ValueProp.Unpowered|ValueProp.Unblockable),null,null);
		foreach(Creature mos in base.CombatState.HittableEnemies.ToList())
		{
			if(mos.HasPower<DeathPower>())
			{
				await CreatureCmd.Damage(choiceContext,mos,new DamageVar(base.DynamicVars.HpLoss.IntValue,ValueProp.Unpowered|ValueProp.Unblockable),null,null);
			}
		}
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars.HpLoss.UpgradeValueBy(2);
	}
}

}
