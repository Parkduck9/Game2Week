# 스토어 이미지

2026-10-06 Codex 8단계 자료. 생성 일러스트와 실제 게임 캡처를 구분한다.

- icon_source.png: 주인공 3면도를 참고하여 imagegen으로 생성한 정사각 아이콘 원본.
- hero_source.png: 주인공 3면도와 title_screen.png를 참고하여 imagegen으로 생성한 홍보 원본.
- icon_*.png, wide_310x150.png, hero_1920x1080.png: 원본을 Pillow로 크기/비율 변환. 제목 글자는 없음.
- screenshot_*.png: 전체 PlayMode 테스트의 SceneCapture 결과를 그대로 복사한 실제 게임 화면. 별도 합성·UI 추가 없음.
- Tools/msix/prepare_store_images.py로 같은 원본에서 다시 생성할 수 있다. 두 원본 경로를 인자로 전달한다.

홍보 일러스트는 실제 그래픽 품질을 보증하는 스크린샷으로 사용하지 않는다. 현재 캡처는 Codex 브랜치이며 최종 Claude 통합 후 다시 캡처한다.
