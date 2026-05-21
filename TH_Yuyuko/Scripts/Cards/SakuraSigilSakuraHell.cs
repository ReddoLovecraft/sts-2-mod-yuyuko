using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraSigilSakuraHell : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

		protected override bool ShouldGlowGoldInternal => PileType.Hand.GetPile(base.Owner).Cards.Any(c => c is Sakura);

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Sakura>()];

		public SakuraSigilSakuraHell() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			List<CardModel> hand = PileType.Hand.GetPile(base.Owner).Cards.ToList();
			if (hand.Count == 0)
			{
				return;
			}

			int exhaustedSakuraCount = hand.Count(c => c is Sakura);
			foreach (CardModel card in hand)
			{
				await CardCmd.Exhaust(choiceContext, card);
			}

			decimal damage = base.DynamicVars.Damage.BaseValue;
			if (exhaustedSakuraCount > 0)
			{
				decimal multiplier = 1m;
				for (int i = 0; i < exhaustedSakuraCount; i++)
				{
					multiplier *= 2m;
				}
				damage *= multiplier;
			}
			for(int i=0;i<hand.Count;i++)
			{
				foreach(Creature mos in base.CombatState.HittableEnemies.ToList())
				{
					NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(mos, VfxColor.Purple));
				}
				await DamageCmd.Attack(damage)
				.FromCard(this)
				.TargetingAllOpponents(base.CombatState)
				.Execute(choiceContext);
			}
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Sakura.Create(base.Owner, hand.Count, base.CombatState), PileType.Hand, addedByPlayer: true));
		}

		protected override void OnUpgrade()
		{
			base.DynamicVars.Damage.UpgradeValueBy(3m);
		}
	}
}
