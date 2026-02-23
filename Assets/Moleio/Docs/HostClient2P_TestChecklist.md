# Host-Client 2인 접속 테스트 체크리스트

## 사전 준비
- [ ] `Assets/Moleio/Scenes/MoleioGame.unity`가 Build Settings에 포함되어 있다.
- [ ] 씬에 `MoleFusionBootstrap` 컴포넌트가 붙은 GameObject가 있다.
- [ ] `MoleFusionBootstrap.playerPrefab`이 할당되어 있고, Validate 결과가 통과한다.
- [ ] `MoleFusionBootstrap.gameMode`는 `AutoHostOrClient` 또는 테스트 목적에 맞는 모드다.
- [ ] `MoleGameManager.autoSpawnLocalPlayer`는 네트워크 테스트 시 `false`로 둔다(중복 스폰 방지).

## 플레이어 프리팹 필수 컴포넌트
- [ ] `NetworkObject`
- [ ] `MoleFusionInputAdapter`
- [ ] `MolePlayerController`
- [ ] `Rigidbody2D`
- [ ] `Collider2D` (2D 콜라이더 1개 이상)
- [ ] `MoleBodySegment`
- [ ] `MoleBodyTrail`

## 실행 절차 (에디터 + 빌드 1개 권장)
1. [ ] 클라이언트 실행 전, Host 인스턴스를 먼저 실행한다.
2. [ ] Host 로그에 `StartGame` 성공(오류 없음)을 확인한다.
3. [ ] 두 번째 인스턴스(Client)를 실행한다.
4. [ ] Host 로그에서 `OnPlayerJoined`가 2회 기준(Host 자신 + Client)으로 확인된다.
5. [ ] 양쪽 화면에서 플레이어가 각각 스폰되고 이동 입력이 동기화된다.
6. [ ] 한쪽에서 대시 입력 시 반대쪽에서도 이동 변화가 반영된다.

## 기대 결과
- [ ] Host/Client 모두 플레이어 2명이 보인다.
- [ ] 플레이어 이동/대시가 양쪽에서 동일하게 관찰된다.
- [ ] 접속/종료 시 치명적 에러나 예외 로그가 없다.

## 실패 시 점검
- [ ] 콘솔에 `playerPrefab is not assigned` 또는 `missing:` 로그가 없는지 확인
- [ ] Fusion Project Config의 Network Prefab 등록 상태 확인
- [ ] 방 이름(`sessionName`)과 포트/방화벽 정책 확인
- [ ] Host와 Client가 동일한 빌드/리소스 버전을 사용하는지 확인
