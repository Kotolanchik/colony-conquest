# Art Content Root

Папки для production-контента:

- `Units/` — модели юнитов, риги, анимации, материалы.
- `Buildings/` — модели зданий, LOD, разрушенные варианты.
- `UI/Icons/` — иконки интерфейса (atlas sources и экспорт).
- `VFX/` — префабы и ресурсы эффектов.

Правило: игровые системы не ссылаются на эти ассеты напрямую.  
Привязка идёт через ScriptableObject-каталоги в `Assets/_Project/Catalogs/Presentation/`.
