using Unity.Mathematics;

namespace ColonyConquest.Politics
{
    /// <summary>Политические модификаторы доктрин и форм правления.</summary>
    public static class PoliticalMath
    {
        public struct DoctrineModifiers
        {
            public float Economy;
            public float Happiness;
            public float Science;
            public float Defense;
            public float Crime;
            public float Stability;
        }

        public static DoctrineModifiers GetDoctrineModifiers(PoliticalDoctrineId doctrine)
        {
            return doctrine switch
            {
                PoliticalDoctrineId.Monarchy => new DoctrineModifiers
                {
                    Economy = 0.00f,
                    Happiness = -0.15f,
                    Science = -0.20f,
                    Defense = 0.15f,
                    Crime = -0.05f,
                    Stability = 0.10f
                },
                PoliticalDoctrineId.Conservatism => new DoctrineModifiers
                {
                    Economy = 0.10f,
                    Happiness = 0.00f,
                    Science = -0.15f,
                    Defense = 0.20f,
                    Crime = -0.20f,
                    Stability = 0.15f
                },
                PoliticalDoctrineId.Liberalism => new DoctrineModifiers
                {
                    Economy = 0.10f,
                    Happiness = 0.15f,
                    Science = 0.25f,
                    Defense = -0.15f,
                    Crime = 0.20f,
                    Stability = -0.10f
                },
                PoliticalDoctrineId.Communism => new DoctrineModifiers
                {
                    Economy = 0.05f,
                    Happiness = -0.05f,
                    Science = -0.20f,
                    Defense = 0.25f,
                    Crime = -0.10f,
                    Stability = 0.20f
                },
                PoliticalDoctrineId.Fascism => new DoctrineModifiers
                {
                    Economy = -0.20f,
                    Happiness = -0.15f,
                    Science = -0.30f,
                    Defense = 0.40f,
                    Crime = -0.15f,
                    Stability = 0.10f
                },
                PoliticalDoctrineId.Anarchism => new DoctrineModifiers
                {
                    Economy = -0.25f,
                    Happiness = 0.30f,
                    Science = 0.20f,
                    Defense = -0.30f,
                    Crime = 0.50f,
                    Stability = -0.40f
                },
                PoliticalDoctrineId.ConservativeLiberal => new DoctrineModifiers
                {
                    Economy = 0.15f,
                    Happiness = 0.00f,
                    Science = 0.10f,
                    Defense = 0.00f,
                    Crime = -0.05f,
                    Stability = 0.10f
                },
                PoliticalDoctrineId.SocialDemocracy => new DoctrineModifiers
                {
                    Economy = 0.15f,
                    Happiness = 0.20f,
                    Science = 0.05f,
                    Defense = -0.05f,
                    Crime = -0.05f,
                    Stability = 0.10f
                },
                _ => new DoctrineModifiers
                {
                    Economy = 0f,
                    Happiness = 0f,
                    Science = 0f,
                    Defense = 0f,
                    Crime = 0f,
                    Stability = 0f
                }
            };
        }

        public static short GetDecisionCycleDays(GovernmentFormId form)
        {
            return form switch
            {
                GovernmentFormId.AbsoluteMonarchy => 0,
                GovernmentFormId.ConstitutionalMonarchy => 1,
                GovernmentFormId.Dictatorship => 0,
                GovernmentFormId.Oligarchy => 3,
                GovernmentFormId.Republic => 7,
                GovernmentFormId.Democracy => 14,
                GovernmentFormId.DirectDemocracy => 30,
                _ => 7
            };
        }

        public static float GetDemocracyLevel01(GovernmentFormId form)
        {
            return form switch
            {
                GovernmentFormId.AbsoluteMonarchy => 0f,
                GovernmentFormId.ConstitutionalMonarchy => 0.30f,
                GovernmentFormId.Dictatorship => 0f,
                GovernmentFormId.Oligarchy => 0.10f,
                GovernmentFormId.Republic => 0.60f,
                GovernmentFormId.Democracy => 0.90f,
                GovernmentFormId.DirectDemocracy => 1f,
                _ => 0.5f
            };
        }

        public static float ClampModifier(float m)
        {
            return math.clamp(m, -0.9f, 1.5f);
        }
    }
}
