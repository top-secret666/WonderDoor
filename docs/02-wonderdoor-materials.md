# Scene 02 WonderDoor — материалы (CC0 + генерация)

Скачай в папку: `D:\HUH\dream\_art_inbox\`  
В Unity потом перенесём в `Assets/_Project/Art/`.  
Бери **2K-JPG** (хватит для демки; 8K тяжело).

Лицензия везде **CC0** — можно в портфолио без атрибуции (в README всё равно перечислим — выглядит профессионально).

---

## 1. Скачать готовое (обязательно)

### Коридор A (Alice / двери)

| Что | Ссылка | Что жать |
|-----|--------|----------|
| Шахматный / клетчатый пол | https://polyhaven.com/a/checkered_pavement_tiles | Download → **2K** → ZIP (Color + Roughness + Normal GL) |
| Запасной «мраморный» checkered | https://polyhaven.com/a/floor_tiles_06 | 2K ZIP |
| Дерево для панелей / рам | https://ambientcg.com/a/Wood026 | **2K-JPG** |
| Готовая текстура двери (фото) | https://ambientcg.com/a/Door001 | **2K-JPG** (база; цвет дверей красим материалом) |
| Простые 3D-двери (mesh) | https://kenney.nl/assets/furniture-kit | Download → двери / door в пакете |
| Или Building Kit с дверями | https://kenney.nl/assets/modular-buildings | CC0 modular + doors |
| Пинстрайп / обои — лучше сгенерировать | см. промпты ниже | — |

### Комната B (трава + окна + небо)

| Что | Ссылка | Что жать |
|-----|--------|----------|
| Трава (пол) | https://ambientcg.com/a/Grass001 | **2K-JPG** |
| Белая плитка стен | https://ambientcg.com/a/Tiles100 | **2K-JPG** |
| Запас белой плитки | https://ambientcg.com/a/Tiles107 | **2K-JPG** (если есть в каталоге) |
| Небо / HDRI в окна | https://polyhaven.com/a/meadow_2 | HDRI **2K** `.exr` или `.hdr` |
| Ясное небо (альт.) | https://polyhaven.com/a/hausdorf_clear_sky | 2K HDR |
| Полдень над травой | https://polyhaven.com/a/noon_grass | 2K HDR |

### Потолок / свет (опционально)

| Что | Ссылка |
|-----|--------|
| Кабинетный потолок / пластик | https://ambientcg.com/a/Plastic008 или поиск `Ceiling` на https://ambientcg.com/list?type=Material |

---

## 2. Сгенерировать (то, чего нет в CC0 «как на рефе»)

Используй Midjourney / Leonardo / Flux / ChatGPT Images — **seamless / tileable** где написано.

### Промпт A — обои коридора (пинстрайп)

```
seamless tileable wallpaper texture, white background, thin irregular vertical black hand-drawn lines, dreamcore liminal hotel, soft paper grain, flat lighting, no shadows, no furniture, 4k
```

Negative: `perspective, door, furniture, text, watermark`

### Промпт B — шахматный глянцевый пол (если Poly Haven не зайдёт)

```
seamless tileable black and white checkerboard floor texture, high gloss marble, sharp squares, subtle reflections baked out, PBR albedo only, flat top-down, 4k
```

### Промпт C — цветная дверь (сделай 4 варианта: blue/yellow, purple/pink, red, orange)

```
front view game asset, single closed wooden door with ornate frame and triangular pediment, painted {COLOR} door with contrasting {TRIM} trim, Alice in Wonderland dreamcore, clean studio lighting, white background, centered, no floor, no hallway, 4k
```

Подставь:
- `{COLOR}=pastel blue`, `{TRIM}=yellow`
- `{COLOR}=lavender purple`, `{TRIM}=pink`
- `{COLOR}=bright red`, `{TRIM}=dark burgundy`
- `{COLOR}=orange`, `{TRIM}=cream`

Экспорт: PNG с прозрачным фоном если можешь; иначе вырежи в Photopea.

### Промпт D — рамка/наличник (модульный)

```
game ready door frame molding only, ornate victorian trim, painted gold yellow, front orthographic view, transparent background, no door leaf, 4k
```

### Промпт E — куст травы / цветок (billboard или mesh-замена)

```
small grass tuft with white daisy flowers, isolated on transparent background, soft daylight, game sprite style, no ground plane shadow stretch, 2k
```

```
single small purple wildflower plant, transparent background, game asset, soft light, 2k
```

### Промпт F — EXIT табличка (мелочь)

```
green illuminated EXIT sign, front view, rectangular, soft glow, transparent background, game UI prop, 1k
```

---

## 3. Чеклист перед созданием проекта

Положи файлы так (имена свободные):

```
D:\HUH\dream\_art_inbox\
  floor_checker\     ← Poly Haven checkered
  wood\              ← Wood026
  door_tex\          ← Door001
  doors_mesh\        ← Kenney (если скачала)
  grass\             ← Grass001
  tiles_white\       ← Tiles100
  hdri\              ← meadow_2 2K
  gen\               ← твои сгенерённые PNG
```

Минимум для старта greybox: **пол + плитка + трава + HDRI**. Двери можно сначала цветными кубами.
