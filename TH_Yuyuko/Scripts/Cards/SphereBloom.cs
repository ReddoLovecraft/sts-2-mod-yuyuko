using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Powers;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class SphereBloom : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1),new DynamicVar("Power",9)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DeathPower>()
    });
	public SphereBloom() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Attack", base.Owner.Character.AttackAnimDelay);
		YuyukoVfxManager.PlaySimple("Scourge",null,VfxTargetPositionType.TargetSide);
		await PowerCmd.Apply<DeathPower>(base.CombatState.HittableEnemies,this.DynamicVars["Power"].IntValue,Owner.Creature,this);
		await CardPileCmd.Draw(choiceContext,this.DynamicVars.Cards.IntValue,Owner);
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars["Power"].UpgradeValueBy(4);
		this.DynamicVars.Cards.UpgradeValueBy(1);
	}
}

}
