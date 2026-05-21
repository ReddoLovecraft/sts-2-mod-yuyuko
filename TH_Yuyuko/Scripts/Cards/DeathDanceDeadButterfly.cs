using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
public class DeathDanceDeadButterfly : YuyukoCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<ButterflyDeathPower>()
    });
	public DeathDanceDeadButterfly() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
	{
	}
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await ToolBox.SummonButterflies<ButterflyDeathPower>(choiceContext,Owner.Creature,this.DynamicVars.Cards.IntValue);
		await CardPileCmd.Draw(choiceContext,this.DynamicVars.Cards.IntValue,Owner);
	}
	protected override void OnUpgrade()
	{
		this.DynamicVars.Cards.UpgradeValueBy(1);
	}
}

}
