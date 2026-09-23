# 오브젝트 풀 사용 안내

## 1. 기존 프로젝트에서 할 일

기존 ObjectPoolManager의 `objList`, `canvasPools` 목록은 그대로 사용합니다. 추가 컴포넌트를 프리팹에 붙일 필요는 없습니다. 매니저가 객체의 소속 풀과 사용 상태를 기억합니다.

- 일반 객체: 풀 이름, 프리팹, 미리 생성할 개수를 등록합니다.
- 캔버스 객체: 위 항목에 대상 캔버스도 연결합니다.
- 풀 이름은 두 목록 전체에서 중복되지 않아야 합니다.
- 유닛 데이터의 `battlePoolName`은 등록한 풀 이름과 정확히 일치해야 합니다.
- 미리 생성할 개수는 최대 개수가 아닙니다. 부족하면 프리팹을 추가 생성합니다.
- 등록 목록은 플레이 전에 설정하세요. 초기화 이후 목록을 바꿔도 자동 재등록되지 않습니다.

## 2. 생성 순서

비활성 객체 확보 → 위치·데이터 준비 → 활성화 → `OnSpawned()` 실행

객체를 미리 만드는 동안에는 비활성 부모를 사용하므로 효과가 미리 발동하지 않습니다. 처음 사용할 때까지 `Awake()`가 실행되지 않을 수도 있습니다. 준비 함수에서 필요한 참조는 인스펙터로 연결하거나, 중복 호출해도 안전한 참조 준비 함수로 확보하세요.

### 위치만 지정할 때

```csharp
GameObject effect = ObjectPoolManager.instance.Spawn(
    "Fire", spawnPosition, Quaternion.identity);

if (effect == null)
    Debug.LogWarning("화염 스킬 생성에 실패했습니다. 풀 등록과 부모 활성 상태를 확인해 주세요.");
```

### 데이터까지 설정할 때

```csharp
GameObject unit = ObjectPoolManager.instance.Spawn(data.battlePoolName, obj =>
{
    // 이 함수는 비활성 상태에서 실행됩니다. 직접 SetActive(true)를 호출하지 않습니다.
    obj.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
    Unit_Base_Test unitBase = obj.GetComponent<Unit_Base_Test>();
    if (unitBase == null)
        throw new System.InvalidOperationException("전투 프리팹에 Unit_Base_Test가 없습니다.");
    unitBase.Init(data);
});
```

기존 전투 유닛 생성은 `BattleUnitFactory.instance.CreateBattleUnit(data, spawnPosition)`을 그대로 사용하면 됩니다. 내부에서 위 순서로 처리합니다.

초기화 콜백에서는 코루틴·스킬 효과를 시작하지 마세요. 활성화 후 실행할 동작은 `OnSpawned()`에서 처리합니다. 초기화 중 예외가 발생하면 로그를 남기고 반환을 시도하며 생성 결과는 `null`입니다. 준비 중 직접 풀로 반환하여 생성을 취소한 경우에도 `null`입니다.

## 3. 반환 방법

```csharp
ObjectPoolManager.instance.ReturnObject(gameObject);
```

반환 순서는 `OnDespawned()` → 비활성화 → 원래 풀 부모로 이동 → 보관입니다.

- 반환 전에 직접 `SetActive(false)`를 호출할 필요가 없습니다.
- 중복 반환 요청은 무시하여 큐에 한 번만 넣습니다.
- `SetActive(false)`만 호출하면 풀에 반환되지 않습니다.
- 이 매니저 소속이 아닌 객체는 경고만 표시하며 삭제하지 않습니다.
- 풀링 객체는 평소에 `Destroy()`하지 말고 반환하세요.
- 반환 이후에는 보관한 참조로 객체를 수정하지 마세요. 다른 용도로 재사용될 수 있습니다.

## 4. 새로운 스킬에 적용하기

기존 스킬 컴포넌트가 `IPoolable`을 구현하면 됩니다. 인터페이스 자체는 컴포넌트로 추가하지 않습니다.

```csharp
using UnityEngine;

public class ExamplePooledSkill : MonoBehaviour, IPoolable
{
    public void OnSpawned()
    {
        // 풀에서 꺼낼 때마다 실행됩니다. 이곳에서 스킬을 발동합니다.
        CancelInvoke();
        Invoke(nameof(ReturnToPool), 1f);
    }

    public void OnDespawned()
    {
        // 생성 준비가 취소되어 사용하지 못한 경우에도 호출될 수 있습니다.
        CancelInvoke();
        StopAllCoroutines();
        // 타깃 목록이나 매 사용마다 바뀌는 상태도 여기서 정리하세요.
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.instance.ReturnObject(gameObject);
    }
}
```

`Start()`는 객체마다 처음 한 번만 실행되므로 매 사용마다 필요한 스킬 발동이나 반환 예약을 넣지 않습니다. 콜백 대상은 객체를 만들 때 한 번 찾아 보관하므로 필요한 컴포넌트는 프리팹에 미리 붙여 주세요. `OnSpawned()`는 활성 상태이며 사용 가능한 컴포넌트에만 실행되고, `OnDespawned()`는 등록된 컴포넌트의 정리를 위해 비활성 컴포넌트에도 실행될 수 있습니다.

## 5. 이전 호출과의 호환

- `GetObject(poolName)`도 사용할 수 있지만 데이터를 준비하는 과정 없이 바로 활성화합니다. 기존 UI 호출은 이 호환 방식을 유지합니다. 새로 작성하는 데이터 의존 코드는 `Spawn(poolName, 초기화함수)`를 사용하세요.
- `ReturnObject(poolName, obj)`도 유지합니다. 이름이 다르면 경고를 표시하고 실제 소속 풀로 반환합니다. 새 코드는 `ReturnObject(obj)`를 사용하세요.
- `IsRented(obj)`는 매니저가 해당 객체를 사용 중으로 기록했는지 확인합니다. 활성 여부와 같은 의미는 아닙니다.
- 클래스명·함수명·필드명·풀 식별 문자열은 기존 연결을 위해 영어 이름을 유지합니다.

## 6. 검사 방법

Unity 재생을 중지한 뒤 상단 메뉴의 **도구 → 검증 → 오브젝트 풀 기본 동작 검사**를 실행합니다.

별도 임시 씬에서 초기화·재사용·중복 반환 방지·생성 취소·풀 확장·캔버스 반환을 검사합니다. 게임 씬이나 실제 저장 데이터를 읽지 않습니다. 성공하면 콘솔에 `[오브젝트 풀 검사 통과]`가 표시됩니다.

이 검사는 실제 전투 검사를 대체하지 않습니다. 플레이 모드에서 스킬을 두 번 이상 사용하고, 투사체 발사, 유닛 사망과 재시작, 보스 생성도 확인하세요.

## 7. 경고가 나왔을 때

| 메시지 내용 | 확인할 사항 |
|---|---|
| 풀이 등록되지 않음 | 요청 이름과 인스펙터의 풀 이름 일치 여부 |
| 풀 이름 중복 | 일반 풀과 캔버스 풀 전체의 이름 중복 여부 |
| 대상 캔버스 미연결 | 캔버스 전용 풀의 대상 캔버스 연결 |
| 부모 비활성 | 생성할 객체의 부모 계층 활성 상태 |
| 보관 부모 삭제 | 풀 매니저보다 씬·캔버스가 먼저 삭제되었는지 여부 |
| 요청 이름과 실제 소속이 다름 | 반환 호출을 ReturnObject(obj)로 변경 |
| 매니저 소속 객체가 아님 | 다른 매니저에서 생성했거나 직접 생성한 객체인지 여부 |

프로젝트가 직접 작성한 안내 문구는 한글입니다. Unity 또는 외부 라이브러리가 생성하는 예외의 형식·내부 메시지·스택 추적은 원본 진단 정보를 유지하므로 영어가 포함될 수 있습니다.
