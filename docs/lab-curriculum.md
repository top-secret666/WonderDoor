# Lab: маленькие сцены → Junior Unity (Минск)

Формат портфолио: **несколько коротких playable-сцен**, не одна недоделанная игра.  
Репозиторий-эталон: [top-secret666/backrooms](https://github.com/top-secret666/backrooms)

Каждая сцена закрывает кластер из `Unity_Junior_Minsk_checklist.docx` (🔴 → 🟡 → 🟢).

## Как работаем (ритм 6 часов)

| Блок | Кто | Что |
|------|-----|-----|
| **3 ч Editor** | Ты руками в Unity | Сцена, свет, префабы, UI в иерархии, материалы. Я — шаги, хоткеи, «почему так» |
| **3 ч Code** | **Ты печатаешь код** | Я диктую архитектуру / даю скелет / ревью. На собесе спросят *твой* код |

Правило портфолио: в README у каждой сцены блок **What I learned** + что сделано именно тобой.

Алгоритм сессии:
1. Я называю цель дня и **что искать на Pinterest**
2. Ты кидаешь 1–3 рефа (скрин/видео/пин)
3. Материалы: готовые CC0 / твои / промпты под генерацию
4. Greybox → системы → атмосфера → коммит + 20 сек гиф

## Карта сцен ↔ чеклист

| Scene | Фокус (требования) | Статус |
|-------|--------------------|--------|
| **01 Level 0** | Rigidbody FPS, SO configs, events (noise bus), prefabs, light, flashlight | ✅ [backrooms](https://github.com/top-secret666/backrooms) |
| **02 Interact + UI** | uGUI / TMP, prompt, Canvas Scaler, Triggers, coroutines, Animator (дверь), New Input Actions | 🔜 сегодня |
| **03 Atmosphere polish** | Particle System, простой Shader Graph, пост/URP volume, Profiler snapshot | planned |
| **04 Save + State** | PlayerPrefs/JSON, State pattern, простой progress | planned |
| **05 Mobile slice** | Touch / safe area, Android build, UI под телефон | planned |
| **06 Optional** | DOTween / Cinemachine / простой NavMesh AI — под конкретную вакансию | later |

Незакрытое 🔴 после 01→02: углубить векторы/Quat на практике, Object Pool (сцена 03 или мини-drill), рассказ о проекте 2–3 мин.

## Scene 02 — бриф (зафиксировано)

**Название:** `02_WonderDoor`  
**Рефы:** Alice-двери (коридор) → травяной коридор с окнами в небо  
**Длина:** 60–90 секунд playable  

**Луп:** идёшь по короткому «wonder»-коридору → одна дверь работает (остальные locked / shake) → UI «E — открыть» → Animator двери → fade (coroutine) → травяная комната → одна строка текста на экране.

**Скоуп (чтобы доделать):**
| Делаем | Не делаем в Scene 02 |
|--------|----------------------|
| Прямой коридор ~8–12 м (не S-кривая) | Полная кривая как на рефе |
| 4–5 дверных префабов (цвет/рамка), **1 интерактивная** | 20 уникальных дверей + кролик |
| Шахматная плоскость + Reflection Probe | Идеальный mirror floor |
| Комната 2: короткая трава (плоскость + 2–3 tuft-префаба + цветы), 3–4 окна, skybox | Бесконечный dense grass / terrain |
| UI prompt + fade + Animator | Диалоги, инвентарь, несколько концовок |

**Закрывает из чеклиста:** UI, Animator, coroutines, OnTrigger, Input System Actions, RectTransform/якоря, префабы модульных дверей.
