using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Powers;
namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public sealed class DeathSigilGhastlyDream : YuyukoCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,CardKeyword.Innate];
    	protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[3]
    {
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<DeathDesirePower>()
    });

    protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(1) };

    public DeathSigilGhastlyDream()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
       await PowerCmd.Apply<WeakPower>(base.CombatState.HittableEnemies,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
       await PowerCmd.Apply<VulnerablePower>(base.CombatState.HittableEnemies,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
       await PowerCmd.Apply<DeathDesirePower>(base.CombatState.HittableEnemies,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
       if (LocalContext.IsMe(base.Owner))
		{
			VfxCmd.PlayFullScreenInCombat("mod_vfx://vfx_adrenaline");
		}
       await CardPileCmd.Draw(choiceContext,2,Owner);
    }

    protected override void OnUpgrade()
    {
        //base.DynamicVars.Cards.UpgradeValueBy(1);
        this.EnergyCost.UpgradeBy(-1);
    }
}

}
