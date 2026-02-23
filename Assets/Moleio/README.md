# Moleio MVP (Fusion Ready)

이 폴더(`Assets/Moleio`) 내부에서만 Moleio 기본 플로우를 구성합니다.

## 포함된 기본 플로우
- 터치패드/키보드 이동
- 먹이 섭취 시 몸 길이 증가
- 대시(게이지 소모/회복)
- 머리 vs 상대 몸통 충돌 시 사망
- 사망 시 몸 일부 드랍 먹이 생성 + 리스폰
- Fusion 입력 수집/플레이어 스폰 루프(패키지 활성 시)

## 스크립트 구조
- `Assets/Moleio/Scripts/Core/IMoleInput.cs`
- `Assets/Moleio/Scripts/Core/MolePlayerController.cs`
- `Assets/Moleio/Scripts/Core/MoleBodyTrail.cs`
- `Assets/Moleio/Scripts/Core/MoleBodySegment.cs`
- `Assets/Moleio/Scripts/Core/MoleFood.cs`
- `Assets/Moleio/Scripts/Core/MoleGameManager.cs`
- `Assets/Moleio/Scripts/Input/MoleVirtualJoystick.cs`
- `Assets/Moleio/Scripts/Input/MoleDashButton.cs`
- `Assets/Moleio/Scripts/Input/MoleInputRouter.cs`
- `Assets/Moleio/Scripts/Network/MoleFusionBridge.cs`
- `Assets/Moleio/Scripts/Network/MoleFusionInput.cs`
- `Assets/Moleio/Scripts/Network/MoleFusionInputAdapter.cs`
- `Assets/Moleio/Scripts/Network/MoleFusionBootstrap.cs`

## 씬 세팅 최소 체크리스트
1. `Assets/Moleio/Scenes/MoleioGame.unity` 생성
2. 빈 오브젝트 `MoleGameManager` 생성 후 `MoleGameManager` 컴포넌트 부착
3. `Player` 프리팹 구성
   - 필수 컴포넌트: `Rigidbody2D`, `CircleCollider2D (isTrigger=true)`,
     `MolePlayerController`, `MoleBodyTrail`, `MoleBodySegment`, `MoleInputRouter`
4. `BodySegment` 프리팹 구성
   - 스프라이트 + `Collider2D (isTrigger=true)` 권장
5. `Food` 프리팹 구성
   - `Collider2D (isTrigger=true)`, `MoleFood`
6. `MoleGameManager` 인스펙터에 `playerPrefab`, `foodPrefab` 연결
7. UI Canvas 아래 조이스틱/대시 버튼 오브젝트 생성 후
   - 조이스틱 오브젝트: `MoleVirtualJoystick`
   - 대시 버튼 오브젝트: `MoleDashButton`
   - `Player`의 `MoleInputRouter`에 두 참조 연결

## Fusion 연결
- `MoleFusionBootstrap`을 씬 오브젝트(예: `NetworkRunnerRoot`)에 붙이고 `playerPrefab`에 `NetworkObject` 프리팹을 연결하세요.
- 네트워크 플레이어 프리팹에는 `MolePlayerController` + `MoleFusionInputAdapter`를 함께 붙여서 입력 소스를 Fusion으로 전환합니다.
- 입력 소스는 `MoleInputRouter.LocalMove`, `MoleInputRouter.LocalDashHeld`를 통해 `OnInput`에서 전달됩니다.
- 코드 분기는 `FUSION_WEAVER` 기준입니다(Photon Fusion 패키지 설치 시 활성).

## MCP 예시(씬 작업)
- 씬 생성: `manage_scene(action="create", name="MoleioGame", path="Assets/Moleio/Scenes")`
- 씬 로드: `manage_scene(action="load", name="MoleioGame")`
- 오브젝트 생성: `manage_gameobject(action="create", name="MoleGameManager")`
- 컴포넌트 추가: `manage_components(action="add", target="MoleGameManager", component_type="MoleGameManager")`
- 씬 저장: `manage_scene(action="save", path="Assets/Moleio/Scenes/MoleioGame.unity")`

## 주의
- 현재 저장된 `Packages/manifest.json` 기준으로는 Fusion 패키지 항목이 보이지 않습니다.
- Unity 에디터에서 설치 직후라면 `manifest.json` 저장/리프레시가 완료됐는지 확인해 주세요.
