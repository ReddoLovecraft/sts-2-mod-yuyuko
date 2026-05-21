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
public class DeathDesire : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4),new DynamicVar("Power",1)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[2]
    {
        HoverTipFactory.FromPower<DeathPower>(),
        HoverTipFactory.FromPower<DeathDesirePower>()
    });
	public DeathDesire() : base(0, CardType.Skill, CardRarity.Basic, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		 if(Owner.Character is YuyukoCharacter)
		 await CreatureCmd.TriggerAnim(Owner.Creature, "Summon", Owner.Character.CastAnimDelay);
		 if(IsUpgraded)
		 {
			await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies, this.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
			await PowerCmd.Apply<DeathDesirePower>(base.CombatState.HittableEnemies, this.DynamicVars["Power"].IntValue, base.Owner.Creature, this);
		 }
		 else
		 {
			Creature creature = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
			if (creature != null)
			{
			await PowerCmd.Apply<DeathPower>(creature, this.DynamicVars.Cards.IntValue, base.Owner.Creature, this);
			await PowerCmd.Apply<DeathDesirePower>(creature, this.DynamicVars["Power"].IntValue, base.Owner.Creature, this);
			}
		 }
	}
	protected override void OnUpgrade()
	{
		
	}
}

}
