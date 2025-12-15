/// <summary>
/// 오브젝트 풀에서 관리하는 모든 게임 오브젝트의 유형을 정의하는 열거형입니다.
/// 문자열 대신 사용하여 타입 안정성을 높이고 실수를 방지합니다.
/// </summary>
public enum PoolObjectType
{
    None, // 이펙트 없음 등을 표현하기 위함

    // 적 유형 (예시)
    Enemy, 

    // UI 및 효과
    DistanceIndicator,
    DestructionEffect,
    WarningEffect
}
