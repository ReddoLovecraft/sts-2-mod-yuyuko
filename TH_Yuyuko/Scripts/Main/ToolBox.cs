using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using TH_Yuyuko.Scrpits.Powers;

namespace TH_Yuyuko.Scripts.Main
{
    public static class ToolBox
    {
        public enum ButterflyKind
        {
            Death,
            Soul,
            Energy
        }
        public static int GetDebuffTotalCount(Creature target) 
        {
            int result = 0;
            foreach(PowerModel debuff in target.Powers) 
            {
                if(debuff.Type==PowerType.Debuff) 
                {
                    if (debuff.Amount > 0)
                        result += debuff.Amount;
                    else
                        result++;
                }
            }
            return result;

        }
        public static int GetDebuffKind(Creature target)
        {
            int result = 0;
            foreach (PowerModel debuff in target.Powers)
            {
                if (debuff.Type == PowerType.Debuff)
                {
                        result++;
                }
            }
            return result;
        }
        public static bool IsSpringEnhanced(Creature owner)
        {
            return owner.GetPower<SpringPower>()?.Amount >= 30;
        }

        public static int GetTotalButterflies(Creature owner)
        {
            return GetButterfliesByKind(owner, ButterflyKind.Death)
                + GetButterfliesByKind(owner, ButterflyKind.Soul)
                + GetButterfliesByKind(owner, ButterflyKind.Energy);
        }

        public static int GetButterfliesByKind(Creature owner, ButterflyKind kind)
        {
            return kind switch
            {
                ButterflyKind.Death => owner.GetPower<ButterflyDeathPower>()?.Amount ?? 0,
                ButterflyKind.Soul => owner.GetPower<ButterflySoulPower>()?.Amount ?? 0,
                ButterflyKind.Energy => owner.GetPower<ButterflyEnergyPower>()?.Amount ?? 0,
                _ => 0
            };
        }

        public static async Task<int> ConsumeRandomButterflies(PlayerChoiceContext choiceContext, Creature owner, int amount, Rng? rng = null)
        {
            if (amount <= 0)
            {
                return 0;
            }

            rng ??= Rng.Chaotic;
            int consumed = 0;

            for (int i = 0; i < amount; i++)
            {
                var options = GetCurrentButterflyPowers(owner);
                int total = options.Sum(p => p.amount);
                if (total <= 0)
                {
                    break;
                }

                int pick = rng.NextInt(0, total);
                ButterflyPowerModel? pickedPower = null;
                int running = 0;
                foreach (var (power, powerAmount) in options)
                {
                    running += powerAmount;
                    if (pick < running)
                    {
                        pickedPower = power;
                        break;
                    }
                }

                if (pickedPower == null)
                {
                    break;
                }

                await PowerCmd.ModifyAmount(pickedPower, -1m, owner, null);
                consumed++;
            }

            return consumed;
        }

        public static async Task SummonButterfliesRandomly(PlayerChoiceContext choiceContext, Creature owner, int count, Rng? rng = null)
        {
            if (count <= 0)
            {
                return;
            }

            rng ??= Rng.Chaotic;
            int death = 0;
            int soul = 0;
            int energy = 0;

            for (int i = 0; i < count; i++)
            {
                float roll = rng.NextFloat();
                if (roll < 0.5f)
                {
                    death++;
                }
                else if (roll < 0.8f)
                {
                    soul++;
                }
                else
                {
                    energy++;
                }
            }
            await TriggerSummonAnimIfNeeded(owner);
            if (death > 0)
            {
                SfxCmd.Play(YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/summon.wav"));
                await PowerCmd.Apply<ButterflyDeathPower>(owner, death, owner, null);
            }
            if (soul > 0)
            {
                SfxCmd.Play(YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/summon.wav"));
                await PowerCmd.Apply<ButterflySoulPower>(owner, soul, owner, null);
            }
            if (energy > 0)
            {
                SfxCmd.Play(YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/summon.wav"));
                await PowerCmd.Apply<ButterflyEnergyPower>(owner, energy, owner, null);
            }
			ButterflyPowerModel.EnsureOrbitVfx(owner);
			await TriggerAfterSummonButterfly(choiceContext, owner);
        }

        public static async Task SummonButterflies<TButterflyPower>(PlayerChoiceContext choiceContext, Creature owner, int count)
            where TButterflyPower : ButterflyPowerModel, new()
        {
            if (count <= 0)
            {
                return;
            }
            await TriggerSummonAnimIfNeeded(owner);
            SfxCmd.Play(YuyukoInit.ToModSfxPath("TH_Yuyuko/Artworks/SFX/summon.wav"));
            await PowerCmd.Apply<TButterflyPower>(owner, count, owner, null);
            ButterflyPowerModel.EnsureOrbitVfx(owner);
			await TriggerAfterSummonButterfly(choiceContext, owner);
        }

        private static async Task TriggerSummonAnimIfNeeded(Creature owner)
        {
            if (owner.Player.Character is YuyukoCharacter)
            {
                await CreatureCmd.TriggerAnim(owner, "Summon", owner.Player.Character.CastAnimDelay);
            }
        }

        private static List<(ButterflyPowerModel power, int amount)> GetCurrentButterflyPowers(Creature owner)
        {
            var list = new List<(ButterflyPowerModel power, int amount)>();

            var death = owner.GetPower<ButterflyDeathPower>();
            if (death?.Amount > 0)
            {
                list.Add((death, death.Amount));
            }

            var soul = owner.GetPower<ButterflySoulPower>();
            if (soul?.Amount > 0)
            {
                list.Add((soul, soul.Amount));
            }

            var energy = owner.GetPower<ButterflyEnergyPower>();
            if (energy?.Amount > 0)
            {
                list.Add((energy, energy.Amount));
            }

            return list;
        }

		private static async Task TriggerAfterSummonButterfly(PlayerChoiceContext choiceContext, Creature owner)
		{
			foreach (DeadButterflyFloatingMoonPower power in owner.GetPowerInstances<DeadButterflyFloatingMoonPower>())
			{
				await power.Trigger(choiceContext);
			}
		}
    }
}
