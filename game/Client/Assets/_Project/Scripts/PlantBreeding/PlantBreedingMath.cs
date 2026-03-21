using Unity.Mathematics;

namespace ColonyConquest.PlantBreeding
{
    /// <summary>Формулы селекции: наследование, мутации, устойчивость линий и экориск ГМО.</summary>
    public static class PlantBreedingMath
    {
        public enum MutationKind : byte
        {
            Neutral = 0,
            Positive = 1,
            Negative = 2,
            Major = 3,
            Lethal = 4
        }

        public static void GetMutationProbabilities(byte labTier, out float positive, out float negative, out float major,
            out float lethal)
        {
            switch (labTier)
            {
                case 1:
                    positive = 0.08f;
                    negative = 0.12f;
                    major = 0.03f;
                    lethal = 0.06f;
                    return;
                case 3:
                    positive = 0.12f;
                    negative = 0.08f;
                    major = 0.06f;
                    lethal = 0.02f;
                    return;
                default:
                    positive = 0.10f;
                    negative = 0.10f;
                    major = 0.05f;
                    lethal = 0.04f;
                    return;
            }
        }

        public static MutationKind RollMutationKind(byte labTier, ref Random random)
        {
            GetMutationProbabilities(labTier, out var pos, out var neg, out var major, out var lethal);
            var r = random.NextFloat();
            if (r < lethal)
                return MutationKind.Lethal;
            r -= lethal;
            if (r < major)
                return MutationKind.Major;
            r -= major;
            if (r < pos)
                return MutationKind.Positive;
            r -= pos;
            if (r < neg)
                return MutationKind.Negative;
            return MutationKind.Neutral;
        }

        public static PlantGenomeTraits BuildOffspring(in PlantGenomeTraits a, in PlantGenomeTraits b, float wA, float wB,
            bool hasHeterosisBonus, MutationKind mutationKind, ref Random random, out float mutationLoad)
        {
            var traits = default(PlantGenomeTraits);
            var heterosisBonus = hasHeterosisBonus ? 5f : 0f;
            var loadSum = 0f;

            traits.Yield = ComputeTrait(PlantTraitAxisId.Yield, a.Yield, b.Yield, wA, wB, heterosisBonus, mutationKind,
                ref random, ref loadSum);
            traits.GrowthSpeed = ComputeTrait(PlantTraitAxisId.GrowthSpeed, a.GrowthSpeed, b.GrowthSpeed, wA, wB,
                heterosisBonus, mutationKind, ref random, ref loadSum);
            traits.DroughtResistance = ComputeTrait(PlantTraitAxisId.DroughtResistance, a.DroughtResistance,
                b.DroughtResistance, wA, wB, heterosisBonus, mutationKind, ref random, ref loadSum);
            traits.ColdResistance = ComputeTrait(PlantTraitAxisId.ColdResistance, a.ColdResistance, b.ColdResistance, wA,
                wB, heterosisBonus, mutationKind, ref random, ref loadSum);
            traits.PestResistance = ComputeTrait(PlantTraitAxisId.PestResistance, a.PestResistance, b.PestResistance, wA,
                wB, heterosisBonus, mutationKind, ref random, ref loadSum);
            traits.NutritionalValue = ComputeTrait(PlantTraitAxisId.NutritionalValue, a.NutritionalValue, b.NutritionalValue,
                wA, wB, heterosisBonus, mutationKind, ref random, ref loadSum);
            traits.Taste = ComputeTrait(PlantTraitAxisId.Taste, a.Taste, b.Taste, wA, wB, heterosisBonus, mutationKind,
                ref random, ref loadSum);

            mutationLoad = loadSum / 7f;
            return traits;
        }

        public static float ComputeStabilityScore(float parentAStability, float parentBStability, float mutationLoad,
            byte generation, bool isGmo, float editDepth)
        {
            var parentBaseline = (parentAStability + parentBStability) * 0.5f;
            var generationBonus = generation * 2f;
            var mutationPenalty = mutationLoad * 1.5f;
            var gmoPenalty = isGmo ? editDepth * 3f : 0f;
            return math.clamp(parentBaseline + generationBonus - mutationPenalty - gmoPenalty, 0f, 100f);
        }

        public static float ComputeEcologyRisk(float editDepth, float plantationAreaFactor, byte bioSafetyTier,
            float isolationLevel01)
        {
            return editDepth * 12f + plantationAreaFactor * 8f - bioSafetyTier * 10f - isolationLevel01 * 6f;
        }

        private static float ComputeTrait(PlantTraitAxisId axis, float a, float b, float wA, float wB, float heterosisBonus,
            MutationKind mutationKind, ref Random random, ref float mutationLoadSum)
        {
            var totalW = math.max(0.001f, wA + wB);
            var baseValue = (a * wA + b * wB) / totalW;
            var mutation = NextMutationDelta(mutationKind, ref random);
            mutationLoadSum += math.abs(mutation);
            return ClampAxisPercent(axis, baseValue + mutation + heterosisBonus);
        }

        private static float NextMutationDelta(MutationKind mutationKind, ref Random random)
        {
            switch (mutationKind)
            {
                case MutationKind.Positive:
                    return math.abs(random.NextFloat(-10f, 10f));
                case MutationKind.Negative:
                    return -math.abs(random.NextFloat(-10f, 10f));
                case MutationKind.Major:
                    return math.abs(random.NextFloat(10f, 20f));
                default:
                    return 0f;
            }
        }

        private static float ClampAxisPercent(PlantTraitAxisId axis, float value)
        {
            PlantTraitAxisTuning.GetPercentRange(axis, out var minP, out var maxP);
            return math.clamp(value, minP, maxP);
        }
    }
}
