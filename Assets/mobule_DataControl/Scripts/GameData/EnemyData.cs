using UnityEngine;

/// <summary>
/// 적의 개별 능력치와 데이터를 정의하는 ScriptableObject입니다.
/// 이 에셋을 통해 코드 수정 없이 다양한 종류의 적을 생성할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "New EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Visuals")]
    [Tooltip("이 적 유형에 사용될 스프라이트입니다.")]
    public Sprite enemySprite;

    [Header("Core Stats")]
    [Tooltip("이 적 유형의 기본 이동 속도입니다.")]
    public float moveSpeed = 5f;
    [Tooltip("이 적을 처치했을 때 얻는 점수입니다.")]
    public int scoreValue = 10;

    [Header("Pooling & Effects")]
    [Tooltip("이 적 프리팹에 해당하는 오브젝트 풀 유형입니다.")]
    public PoolObjectType poolType = PoolObjectType.Enemy;
    [Tooltip("이 적의 화면 밖 표시기에 해당하는 오브젝트 풀 유형입니다.")]
    public PoolObjectType indicatorType = PoolObjectType.DistanceIndicator;
    [Tooltip("이 적이 파괴될 때 생성될 이펙트의 오브젝트 풀 유형입니다.")]
    public PoolObjectType destructionEffectType = PoolObjectType.DestructionEffect;

    [Header("Indicator Color")]
    [Tooltip("적의 거리가 멀 때 표시될 색상입니다.")]
    public Color farColor = Color.green;
    [Tooltip("적의 거리가 가까울 때 표시될 색상입니다.")]
    public Color nearColor = Color.red;
    [Tooltip("색상 변경이 '가까운 색상'으로 완전히 끝나는 최소 거리입니다.")] 
    public float minColorDistance = 2f;
}
