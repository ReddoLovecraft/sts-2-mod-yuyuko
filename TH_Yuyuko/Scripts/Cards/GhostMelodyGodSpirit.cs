using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Yuyuko.Scripts.Main;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class GhostMelodyGodSpirit : YuyukoCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			base.EnergyHoverTip
		];

		public GhostMelodyGodSpirit() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			List<CardModel> etherealCards = new List<CardModel>();
			foreach (PileType pileType in new[] { PileType.Deck, PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust, PileType.Play })
			{
				foreach (CardModel card in pileType.GetPile(base.Owner).Cards)
				{
					if (card.Keywords.Contains(CardKeyword.Ethereal))
					{
						etherealCards.Add(card);
					}
				}
			}
			foreach (CardModel card in etherealCards)
			{
				CardCmd.Upgrade(card);
				card.EnergyCost.AddThisCombat(-1, reduceOnly: true);
			}
		}

		protected override void OnUpgrade()
		{
			AddKeyword(CardKeyword.Innate);
		}
	}
}
