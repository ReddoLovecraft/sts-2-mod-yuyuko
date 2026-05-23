using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Yuyuko.Scripts.Main;
using TH_Yuyuko.Scripts.VFX;

namespace TH_Yuyuko.Scripts.Cards
{
	[Pool(typeof(YuyukoCardPool))]
	public sealed class OtherworldKasuka : YuyukoCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust),HoverTipFactory.FromCard<Soul>()];



		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];

		public OtherworldKasuka() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).WithHitVfxNode(target => YuyukoVfxManager.
			CreateProjectileToTarget("spirit2", Owner.Creature, target, new Vector2(0f, -180f),  new Vector2(0f, -40f)))
		    .Execute(choiceContext);
		}

		public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, MegaCrit.Sts2.Core.Models.AbstractModel? source)
		{
			if (base.Owner == null || card.Owner != base.Owner)
			{
				return;
			}
			if (base.CombatState == null || base.Pile == null)
			{
				return;
			}
			if (card is OtherworldKasuka || card is Soul)
			{
				return;
			}
			PileType? newPile = card.Pile?.Type;
			if (newPile != PileType.Discard && newPile != PileType.Exhaust)
			{
				return;
			}

			if (base.IsUpgraded)
			{
				ulong localId = LocalContext.NetId ?? base.Owner.NetId;
				var ctx = new HookPlayerChoiceContext(this, localId, base.CombatState, GameActionType.Combat);
				await CardPileCmd.Add(this, PileType.Play);
				await CardCmd.AutoPlay(ctx, this, null);
			}
			else
			{
				await CardPileCmd.Add(this, PileType.Hand);
			}
		}
	}
}
