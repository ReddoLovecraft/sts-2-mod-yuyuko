using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
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
public class ReversedButterflyEightBloom : YuyukoCardModel
{
	 public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[4]
    {
		Tools.GetStaticKeyword("SummonButterfly"),
		Tools.GetStaticKeyword("Butterfly"),
        HoverTipFactory.FromPower<ButterflySoulPower>(),
        HoverTipFactory.FromPower<SpringPower>()
    });
	public ReversedButterflyEightBloom() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		YuyukoVfxManager.PlaySimple("eight",Owner.Creature,VfxTargetPositionType.TargetCreature,1.55f);
		await ToolBox.SummonButterflies<ButterflySoulPower>(choiceContext,Owner.Creature,8);
		await PowerCmd.Apply<SpringPower>(Owner.Creature,Owner.Creature.GetPowerAmount<SpringPower>(),Owner.Creature,this);
	}
	protected override void OnUpgrade()
	{
		this.AddKeyword(CardKeyword.Retain);
	}
}

}
