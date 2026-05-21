using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class DeadReturnEarth : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12,ValueProp.Move),new CardsVar(1)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DeathPower>()
    });
	public DeadReturnEarth() : base(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
		foreach (Creature item in enemies)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSpikeSplashVfx.Create(item));
			decimal calculateDamage=base.DynamicVars.Damage.BaseValue+(item.HasPower<DeathPower>()?item.GetPowerAmount<DeathPower>()*base.DynamicVars.Cards.BaseValue:0);
			await DamageCmd.Attack(calculateDamage).FromCard(this).Targeting(item)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase()
			.Execute(choiceContext);
		}
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars.Damage.UpgradeValueBy(1);
		this.DynamicVars.Cards.UpgradeValueBy(1);
	}
}

}
