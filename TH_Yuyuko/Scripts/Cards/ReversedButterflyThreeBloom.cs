using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
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
public class ReversedButterflyThreeBloom : YuyukoCardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];
			protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[4]
    {
		Tools.GetStaticKeyword("SummonButterfly"),
		Tools.GetStaticKeyword("Butterfly"),
        HoverTipFactory.FromPower<ButterflySoulPower>(),
		HoverTipFactory.FromPower<SpringPower>()
    });
	public ReversedButterflyThreeBloom() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		YuyukoVfxManager.PlaySimple("three",Owner.Creature,VfxTargetPositionType.TargetCreature,1.55f);
		await ToolBox.SummonButterflies<ButterflySoulPower>(choiceContext,Owner.Creature,3);
		await PowerCmd.Apply<SpringPower>(Owner.Creature,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
	}
	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (base.Owner == null || cardPlay.Card.Owner != base.Owner)
		{
				await Task.CompletedTask;
				return;
		}
		if(cardPlay.Card is Soul)
			await CardPileCmd.Add(this, PileType.Hand);
	}
	protected override void OnUpgrade()
	{
		this.EnergyCost.UpgradeBy(-1);
	}
}

}
