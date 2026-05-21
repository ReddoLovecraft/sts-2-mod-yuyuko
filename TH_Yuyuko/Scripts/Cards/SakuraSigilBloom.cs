using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraSigilBloom : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<SpringPower>(),
			HoverTipFactory.FromCard<Sakura>()
		];

		public SakuraSigilBloom() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, 4, base.CombatState), PileType.Draw, addedByPlayer: true, position: CardPilePosition.Random));

			int sakuraCount = CountSakuraEverywhere(base.Owner);
			int springPer = base.DynamicVars.Cards.IntValue;
			if (sakuraCount > 0 && springPer > 0)
			{
				await PowerCmd.Apply<SpringPower>(base.Owner.Creature, sakuraCount * springPer, base.Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Cards.UpgradeValueBy(1);
		}

		private static int CountSakuraEverywhere(Player owner)
		{
			int count = 0;
			foreach (PileType pileType in new[] { PileType.Deck, PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust, PileType.Play })
			{
				foreach (CardModel card in pileType.GetPile(owner).Cards)
				{
					if (card is Sakura)
					{
						count++;
					}
				}
			}
			return count;
		}
	}
}
