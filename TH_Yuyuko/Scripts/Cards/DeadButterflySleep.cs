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
public class DeadButterflySleep : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(33),new EnergyVar(1)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DeathPower>()
    });
	protected override bool ShouldGlowGoldInternal => Owner.HasPower<ButterflyDeathPower>();
	public DeadButterflySleep() : base(4, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if(Owner.Character is YuyukoCharacter)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Summon", base.Owner.Character.CastAnimDelay);
		}
		YuyukoRadialButterflyBurstVfx.PlayOnCreature(base.Owner.Creature, base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies,this.DynamicVars.HpLoss.IntValue,base.Owner.Creature,null);
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars.HpLoss.UpgradeValueBy(11);
	}
}

}
