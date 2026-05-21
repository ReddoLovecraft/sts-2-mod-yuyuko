using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using static TH_Yuyuko.Scripts.VFX.YuyukoVfxManager;

namespace TH_Yuyuko.Scripts.Cards
{
[Pool(typeof(YuyukoCardPool))]
public class NoLifeDream : YuyukoCardModel,ITranscendenceCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move),new CardsVar(4)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => (new IHoverTip[1]
    {
        HoverTipFactory.FromPower<DemisePower>()
    });
	public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<NoLifeTicket>();
	public NoLifeDream() : base(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
	{
	}
	
	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		//YuyukoVfxManager.PlaySimple("BaseEffect",cardPlay.Target,VfxTargetPositionType.TargetCreature);
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
		.WithHitFx(null, null, "blunt_attack.mp3")
		.WithHitVfxNode(target => YuyukoVfxManager.CreateProjectileToTarget("BaseEffect", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f)))
		.Execute(choiceContext);
		if(cardPlay.Target!=null&&cardPlay.Target.IsAlive)
		await PowerCmd.Apply<DemisePower>(cardPlay.Target,this.DynamicVars.Cards.IntValue,Owner.Creature,this);
	}
	protected override void OnUpgrade()
	{
		this.EnergyCost.UpgradeBy(-1);
	}
}

}
