using Unity.Entities;

namespace ColonyConquest.Economy
{
    /// <summary>Вспомогательные операции над буфером склада без аллокаций.</summary>
    public static class ResourceStockpileOps
    {
        public static float GetAmount(ref DynamicBuffer<ResourceStockEntry> buffer, ResourceId id)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Id == id)
                    return buffer[i].Amount;
            }

            return 0f;
        }

        public static bool HasAtLeast(ref DynamicBuffer<ResourceStockEntry> buffer, ResourceId id, float amount)
        {
            if (id == ResourceId.None || amount <= 0f)
                return true;
            return GetAmount(ref buffer, id) >= amount;
        }

        /// <summary>Списание одного типа; <c>false</c> если не хватает.</summary>
        public static bool TryConsume(ref DynamicBuffer<ResourceStockEntry> buffer, ResourceId id, float amount)
        {
            if (id == ResourceId.None || amount <= 0f)
                return true;

            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Id != id)
                    continue;
                if (buffer[i].Amount < amount)
                    return false;

                var e = buffer[i];
                e.Amount -= amount;
                buffer[i] = e;
                if (e.Amount <= 1e-4f)
                    buffer.RemoveAt(i);
                return true;
            }

            return false;
        }

        /// <summary>Атомарное списание по рецепту (все входы есть — снимаем все).</summary>
        public static bool TryConsumeRecipe(ref DynamicBuffer<ResourceStockEntry> buffer, in ProductionRecipeDefinition def)
        {
            if (def.Id == ProductionRecipeId.None)
                return false;

            if (!HasAtLeast(ref buffer, def.In0, def.Amount0))
                return false;
            if (def.In1 != ResourceId.None && def.Amount1 > 0f && !HasAtLeast(ref buffer, def.In1, def.Amount1))
                return false;
            if (def.In2 != ResourceId.None && def.Amount2 > 0f && !HasAtLeast(ref buffer, def.In2, def.Amount2))
                return false;

            TryConsume(ref buffer, def.In0, def.Amount0);
            if (def.In1 != ResourceId.None && def.Amount1 > 0f)
                TryConsume(ref buffer, def.In1, def.Amount1);
            if (def.In2 != ResourceId.None && def.Amount2 > 0f)
                TryConsume(ref buffer, def.In2, def.Amount2);
            return true;
        }

        public static void Add(ref DynamicBuffer<ResourceStockEntry> buffer, ResourceId id, float delta)
        {
            if (id == ResourceId.None || delta <= 0f)
                return;

            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Id != id)
                    continue;
                var e = buffer[i];
                e.Amount += delta;
                buffer[i] = e;
                return;
            }

            buffer.Add(new ResourceStockEntry { Id = id, Amount = delta });
        }
    }
}
