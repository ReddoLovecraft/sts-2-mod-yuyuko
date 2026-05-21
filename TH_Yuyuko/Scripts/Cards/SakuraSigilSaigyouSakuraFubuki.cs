using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraSigilSaigyouSakuraFubuki : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(13m, ValueProp.Move), new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public SakuraSigilSaigyouSakuraFubuki() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			foreach (Creature enemy in base.CombatState.HittableEnemies.ToList())
			{
				await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this)
				.TargetingAllOpponents(base.CombatState)
				.WithHitFx("vfx/vfx_giant_horizontal_slash")
				.Execute(choiceContext);
			}
			int sakuraCount = base.DynamicVars.Cards.IntValue;
			if (sakuraCount <= 0)
			{
				return;
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, sakuraCount, base.CombatState), PileType.Draw, addedByPlayer: true, position: CardPilePosition.Random));
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, sakuraCount, base.CombatState), PileType.Hand, addedByPlayer: true));
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, sakuraCount, base.CombatState), PileType.Discard, addedByPlayer: true));
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
