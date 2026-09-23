/// <summary>풀에서 꺼낼 때와 반환할 때 실행할 동작입니다. 비활성 상태의 데이터 준비 후 활성화됩니다.</summary>
public interface IPoolable
{
    /// <summary>활성화 후 매 사용마다 실행합니다. 스킬 발동과 반환 타이머를 시작하세요.</summary>
    void OnSpawned();
    /// <summary>비활성화 전에 실행합니다. 타깃·예약 호출·사용 상태를 정리하세요. 준비 취소 시에도 호출될 수 있습니다.</summary>
    void OnDespawned();
}
