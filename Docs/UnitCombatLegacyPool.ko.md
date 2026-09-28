# 기존 오브젝트 풀을 유지한 유닛 행동 분리

## 그대로 유지한 것

ObjectPoolManager, 인스펙터 등록 목록, BattleUnitFactory는 변경하지 않았습니다.

```csharp
// 유닛: 기존 팩토리 사용
GameObject unit = BattleUnitFactory.instance.CreateBattleUnit(data, spawnPosition);

// 일반 객체: 기존 풀 사용
GameObject obj = ObjectPoolManager.instance.GetObject(poolName);
// 위치와 필요한 데이터를 설정합니다.
ObjectPoolManager.instance.ReturnObject(poolName, obj);
```

새 Spawn 함수나 IPoolable 인터페이스를 요구하지 않습니다. 풀의 중복 반환 등 기존 풀 자체의 제약도 이번 수정에서 바꾸지 않았습니다.

## 구조

```text
Unit_Base_Test: 체력·이동·사망·외부 이벤트·기존 풀과의 연결
├─ UnitCombatController: 역할 선택·탐색·타깃 수명 관리
│  └─ UnitCombatAction: 공통 추상 부모
│     ├─ MeleeCombatAction: 직접 피해
│     ├─ RangedCombatAction: 공격 투사체
│     └─ HealCombatAction: 회복 투사체
└─ UnitStatusEffects: 버프·빙결 타이머
```

행동 부모와 자식은 일반 C# 클래스입니다. 기존 프리팹의 Unit_Base_Test를 제거하거나 새 컴포넌트를 추가하지 않습니다. 선택기와 탐색 배열은 유닛 객체마다 한 번 준비하고 재사용합니다. 공유 행동에는 유닛별 변경 상태를 저장하지 않습니다.

## 데이터별 동작

| 데이터 | 동작 |
|---|---|
| Can Melee | Attack Range 안의 적에게 직접 피해 |
| Can Ranged | Projectile Pool Name의 투사체 발사 |
| Can Heal | 부상당한 아군 우선, Heal Range와 Heal Projectile Pool Name 사용 |
| 근접과 원거리 모두 켜기 | 기존처럼 근접 우선 |
| 힐러의 회복 대상 없음 | 기존처럼 적에게 원거리 공격 가능; 공격 투사체도 등록 필요 |
| 세 항목 모두 끄기 | 전투 타깃 선택하지 않음 |

역할은 Init에서 반영합니다. 실행 중 데이터의 역할을 바꾼 경우에는 Combat.Configure()로 다시 반영해야 합니다. 같은 버프·빙결 재적용은 최신 배율과 시간으로 갱신합니다. 공격력 강화와 일시 버프는 별도로 계산하므로 강화 도중 버프가 덮어써지지 않습니다.

## 기존 풀에서의 실행 순서

1. GetObject가 객체를 활성화합니다.
2. 기존 BattleUnitFactory가 위치를 지정하고 Init(data)를 호출합니다.
3. Init이 역할·타깃·체력·타이머를 초기화합니다. 준비 전에는 전투 Update를 실행하지 않습니다.
4. 반환 시 기존 풀의 SetActive(false)가 OnDisable을 호출합니다.
5. OnDisable에서 타깃·버프·이동 상태를 정리합니다.

OnEnable에서 전투를 시작하지 않으므로 미리 생성하는 과정에서 스킬이나 공격을 발동하지 않습니다. 투사체는 GetObject 직후 Setup을 실행하며, Setup 전에는 이동하지 않습니다. 투사체 반환도 기존 ReturnObject(풀 이름, 객체)를 그대로 사용합니다.

## 메모리 할당과 탐색

- OverlapCircleAll 대신 재사용 배열을 받는 OverlapCircle을 사용합니다. 기존 콜라이더 기반 반경과 레이어 판정을 유지합니다.
- Target Search Capacity는 초기 배열 크기이며 기본 64입니다. 주변 콜라이더 수보다 여유 있게 잡으면 전투 중 배열 확장을 줄일 수 있습니다.
- 배열이 가득 차면 확장하여 재조회합니다. 후보를 임의로 잘라내지는 않지만 이때는 메모리 할당이 발생합니다.
- 선택한 타깃의 참조와 생성 회차를 기억하므로 이동 중 매 프레임 컴포넌트를 다시 찾지 않습니다.
- 공격과 회복은 캡처 람다 없이 기존 풀을 호출합니다.
- 버프·빙결의 코루틴과 대기 객체 대신 숫자 타이머를 사용합니다.
- 애니메이션의 반복 열거형 문자열 변환을 제거했습니다.

최초 초기화, 탐색 배열 확장, 풀 부족 시 객체 추가 생성, 외부 UI·이벤트·오디오·애니메이터는 할당이 발생할 수 있습니다. 게임 전체 GC 0을 보장하지 않습니다. 풀 개수와 탐색 배열을 충분히 준비한 뒤 프로파일러에서 GC Alloc을 확인하세요.

## 확인 방법

재생을 중지한 뒤 **도구 → 검증 → 기존 풀 유닛 행동 검사**를 실행하면 임시 씬에서 역할 선택·사거리·대상 재생성·버프 계산을 확인합니다. 이 검사는 물리 탐색과 실제 풀 재사용 테스트를 대신하지 않습니다.

실제 플레이에서 근접·원거리·힐러를 배치하고 다음을 확인하세요.

- 근접 직접 피해, 공격/회복 투사체의 두 번째 이후 발사
- 회복 대상이 없을 때 힐러의 적 공격
- 대상 사망 직후 같은 풀 객체 재사용 시 이전 추적 중단
- 반환 후 재생성 시 타깃·빙결·버프·이동 초기화
- 버프 중 강화와 버프 만료 후 공격력
- 주변 콜라이더가 많은 전투의 탐색 CPU 비용과 GC Alloc
