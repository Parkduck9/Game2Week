"""생성된 홍보 원본을 보관하고 요청된 스토어/패키지 크기로 변환한다."""
from pathlib import Path
import shutil
import sys
from PIL import Image, ImageOps

root = Path(__file__).resolve().parents[2]
store = root / "Reference/store"
logos = root / "Tools/msix/Images"
store.mkdir(parents=True, exist_ok=True)
logos.mkdir(parents=True, exist_ok=True)
icon_source, hero_source = map(Path, sys.argv[1:3])
shutil.copy2(icon_source, store / "icon_source.png")
shutil.copy2(hero_source, store / "hero_source.png")
icon = Image.open(icon_source).convert("RGB")
hero = Image.open(hero_source).convert("RGB")
for size in (300, 150, 71, 44):
    converted = ImageOps.fit(icon, (size, size), method=Image.Resampling.LANCZOS)
    converted.save(store / f"icon_{size}x{size}.png")
    converted.save(logos / f"Square{size}x{size}Logo.png")
ImageOps.fit(icon, (50, 50), method=Image.Resampling.LANCZOS).save(logos / "StoreLogo.png")
ImageOps.fit(hero, (1920, 1080), method=Image.Resampling.LANCZOS).save(store / "hero_1920x1080.png")
wide = ImageOps.fit(hero, (310, 150), method=Image.Resampling.LANCZOS)
wide.save(store / "wide_310x150.png")
wide.save(logos / "Wide310x150Logo.png")
for name, capture in {"title": "main_fresh", "stages": "stage_select", "jump": "heroine_v2_jump", "dodge": "heroine_v2_dodge", "brace": "heroine_v2_brace"}.items():
    shutil.copy2(root / f"Logs/scene_{capture}.png", store / f"screenshot_{name}.png")
print("스토어 이미지와 실제 PlayMode 캡처 5장 저장 완료")
