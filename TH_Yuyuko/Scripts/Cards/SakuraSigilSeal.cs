using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;
using TH_Yuyuko.Scrpits.Cards;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class SakuraSigilSeal : YuyukoCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(11)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromCard<Sakura>()
		];
		protected override bool ShouldGlowGoldInternal => PileType.Hand.GetPile(base.Owner).Cards.Any(c => c is Sakura);
		public SakuraSigilSeal() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int handCount = PileType.Hand.GetPile(base.Owner).Cards.Count;
			bool isSakura=false;
			if (handCount > 0)
			{
				CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
				CardModel? selected = (await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this)).FirstOrDefault();
				await CardCmd.Exhaust(choiceContext, selected);
				isSakura = selected!=null && selected is Sakura;
			}	
			int strengthLoss = base.DynamicVars.Cards.IntValue;
			YuyukoVfxManager.PlaySimple("lock_effect", cardPlay.Target, YuyukoVfxManager.VfxTargetPositionType.TargetCreature, 0.6f);
			if (isSakura)
			{
				await PowerCmd.Apply<StrengthPower>(cardPlay.Target, -strengthLoss, base.Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<PiercingWailPower>(cardPlay.Target, strengthLoss, base.Owner.Creature, this);
			}		
		}

		protected override void OnUpgrade()
		{
			AddKeyword(CardKeyword.Retain);
		}
	}
}
