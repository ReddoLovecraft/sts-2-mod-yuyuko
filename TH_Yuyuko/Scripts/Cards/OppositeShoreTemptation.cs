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
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
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
public class OppositeShoreTemptation : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(10),new DynamicVar("Power",15)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DeathPower>()
    });
	public OppositeShoreTemptation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.3f, 0.8f, 0.66f), new Color(0f, 0f, 4f, 0.33f));
		NGame.Instance.CurrentRunNode.GlobalUi.AddChildSafely(child);
		await Cmd.CustomScaledWait(0.1f, 0.25f);
		await PowerCmd.Apply<DeathPower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
		int addtional=0;
		if(cardPlay.Target.IsAlive&&cardPlay.Target.HasPower<DeathPower>())
		{
			addtional=cardPlay.Target.GetPowerAmount<DeathPower>()/DynamicVars["Power"].IntValue;
		}
		YuyukoPointingHandsVfx.Create(cardPlay.Target, addtional);
		for(int i=0;i<addtional;i++)
		{
			await PowerCmd.Apply<DeathPower>(cardPlay.Target, DynamicVars.Cards.IntValue,Owner.Creature,this);
		}
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars["Power"].UpgradeValueBy(-5);
	}
}

}
