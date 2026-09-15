# MainTennis 사용법

## 실행

- `Assets/01_Scenes/MainTennis.unity`를 열고 Play를 누릅니다.
- 메뉴 `Tennis > Open Main Tennis` 또는 Ctrl+Shift+M으로 열 수 있습니다.
- 빌드 시작 씬도 MainTennis로 설정했습니다.
- 기존 MainScene의 코트 크기, 양쪽 캐릭터, 후방 카메라, 목표 착지점 기반 공 궤적 구성을 토대로 별도 씬을 만들었습니다.
- MainScene과 BalancePrototype 파일은 이번 작업에서 수정하지 않았습니다. 새 구현은 Prototype 스크립트에 의존하지 않습니다.

## 조작

| 키 | 기능 |
|---|---|
| WASD | 이동과 타구 조준. A/D: 좌우, W: 깊게, S: 짧게 |
| Space 누르기 / 떼기 | 차지 / 서브 또는 스윙 |
| 1 / 2 / 3 | 드라이브 / 로브 / 드롭 |
| 왼쪽 Shift | 스프린트. 스태미나 사용 |
| Esc | 일시정지 / 재개 |
| R | 점수를 초기화하고 재경기 |

공이 자기 코트에 있고 타격 반경·높이 안에 들어왔을 때 Space를 뗍니다. 아주 조금 일찍 떼어도 `swingBuffer` 동안 입력을 보관합니다. 서브 리턴은 첫 바운드 이후에만 가능합니다. 서버는 서브 전에 베이스라인 뒤에 고정됩니다. 창 포커스를 잃으면 일시정지됩니다.

Space를 떼는 순간 누르고 있는 WASD 방향으로 타구를 조준합니다. 대각선 조합도 가능하며, WASD를 누르지 않으면 중앙을 조준합니다. 입력 보관 중 이동 방향이 바뀌어도 스윙 방향은 유지됩니다. 플레이어의 최종 착지 목표는 타격 오차를 적용한 뒤 상대 코트 안쪽으로 제한하며, 서브는 대각선 서비스 박스 안으로 제한합니다. 네트에 걸릴 가능성은 남아 있습니다. 드롭샷은 짧은 깊이를 유지하며 A/D로 좌우를 정합니다.

## 애셋 교체

Hierarchy에 모든 오브젝트가 편집 모드부터 저장되어 있습니다.

```
MainCamera                 Camera + TennisCamera
Directional Light
Court                      TennisCourt (판정 크기)
  Visual
    Ground / CourtSurface / CourtLines / Net
Player                     TennisActor (이동 기준은 발밑)
  Visual                   이 안의 도형을 캐릭터 애셋으로 교체
Enemy                      TennisActor
  Visual                   이 안의 도형을 상대 애셋으로 교체
Ball                       TennisBall
  Visual                   이 안의 구를 공 애셋으로 교체
CourtIndicators            LandingMarker / AimMarker
GameManager                TennisMatch + 모든 참조
```

`Visual`과 그 부모의 게임 로직 컴포넌트는 유지하고, 자식 도형을 숨기거나 교체하세요. 캐릭터 모델은 발이 로컬 Y=0, 키 약 2m가 되도록 Visual 안에서 조절하세요. 코트 중심은 원점, 플레이어 쪽은 -Z입니다. 루트 스케일은 1을 유지하세요. 판정은 수학적으로 계산하므로 추가 메시 콜라이더가 필요하지 않습니다. 애니메이션 연결은 포함하지 않았습니다.

## 밸런스

`Assets/MainTennisBalance.asset`를 Inspector에서 편집합니다. GameManager의 Balance 필드에서 다른 설정 에셋으로 교체할 수도 있습니다.

| 필드 | 기본값 / 의미 |
|---|---|
| Player / Enemy Speed | 7.5 / 6.3 m/s |
| Charge Seconds | 0.85초 |
| Charge Move Multiplier | 차지 중 이동 60% |
| Stamina / Charge Drain / Sprint Drain / Recovery | 100 / 초당 14 / 24 / 20 |
| Shot / Serve Speed | 15 / 18 m/s, 수평 진행 속도 |
| Charged Power | 최대 차지 시 1.3배 |
| Shot Arc | 직선 경로 위로 추가되는 포물선 높이 2.2m |
| Hit Reach / Max Hit Height | 2.1m / 3.2m |
| Swing Buffer | 0.16초 |
| Bounce Speed / Arc / Distance | 0.65배 / 1.25m / 4.2m |
| Reaction Seconds / Aim Error | AI 반응 지연 0.25초 / 좌우 오차 0.45m |
| Games Per Set / Sets To Win | 짧은 테스트 경기: 3게임 기준, 1세트 승리 |

경기 길이는 R로 재시작하면 적용됩니다. 다른 수치는 경기 중에도 참조됩니다. ScriptableObject 에셋을 Play 중 수정한 값은 남을 수 있습니다. 원래 값 보존이 필요하면 먼저 복제하세요.

## 참고 코드에서 반영한 부분

사용자가 지정한 Assembly-CSharp.csproj 주변 C# 파일을 읽었습니다. 프로젝트 파일 자체에 밸런스 데이터가 모두 들어 있는 것은 아닙니다.

- `BaseCharacter.cs`, `ChargeInfo.cs`: 차지 중 감속, 스태미나 소모, 차지에 따른 파워.
- `TypeHit.cs`, `TypeTiming.cs`: BAD / NICE / PERFECT 구분. 여기서는 타격 반경 바깥쪽의 명중은 BAD, 안쪽에서 최대 차지는 PERFECT로 단순화했습니다.
- `Ball.cs`, `DataHitBall.cs`: 목표 지점·속도·높이를 분리한 궤적과 바운드 감쇠.
- `BotAIStatsConfig.cs`: AI 반응 시간과 조준 오차를 독립 설정으로 분리.

참고 게임의 외부 플러그인·상점·갤러리·장비·스킬 시스템은 가져오지 않았습니다. 위 기본 수치는 이 씬 크기에 맞춘 초기 튜닝값이며 원본 게임의 정확한 밸런스 복제는 아닙니다.

## 경기 규칙과 범위

15/30/40, 듀스/어드밴티지, 게임별 서버 교대, 대각선 서브, 첫 폴트와 더블 폴트, 아웃, 네트, 두 번째 바운드, 세트와 타이브레이크를 처리합니다. 세트는 2게임 차가 필요하고 양쪽이 Games Per Set에 도달하면 7점·2점 차 타이브레이크를 합니다. 기본 설정은 짧은 테스트 경기입니다. 6게임/2세트로 바꿀 수 있습니다.

간소화: 엔드 체인지 없이 플레이어는 계속 화면 아래쪽에 있습니다. 네트에 맞은 서브는 폴트이며 렛 재서브는 구현하지 않았습니다. 스태미나는 서브 준비 때 회복됩니다. 공은 Rigidbody 물리가 아닌 목표 기반 궤적입니다.

## 검증

구현 시 Unity 라이브러리 기준 컴파일 오류·경고 0개, 점수 규칙 검사 17개, Unity 내 씬·궤적 검사 15개 통과를 확인했습니다. Play 모드 진입과 HUD 렌더링도 확인했습니다. 장시간 수동 랠리 플레이를 통한 난이도 검증은 추가로 필요합니다.

- `Tennis > Validate Main Tennis` (Ctrl+Shift+J): 씬 참조, 교체용 Visual, 양쪽 대각 서비스 박스, 아웃, 두 번 바운드, 큰 프레임 간격에서 네트 충돌을 검사합니다. 결과: `Logs/MainTennisValidation.txt`, 카메라 이미지: `Logs/MainTennisPreview.png`.
- `Tennis > Capture Game View` (Ctrl+Shift+K): Play 중 Game View를 `Logs/MainTennisGame.png`로 저장합니다.
- `Tools/BuildMainTennisScene.ps1`은 최초 씬 생성 기록입니다. 재실행하면 새 씬·재질·밸런스의 사용자 편집을 덮어쓰므로 일반 작업에서는 실행하지 마세요.
