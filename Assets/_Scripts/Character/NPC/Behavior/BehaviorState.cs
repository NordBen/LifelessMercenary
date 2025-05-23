using Unity.Behavior;

[BlackboardEnum, System.Serializable]
public enum EnemyState
{
    Idle,
	Patrol,
	Chase,
	Attack,
	Flee,
	Skill,
	Heal,
	Summon,
	Dead,
	Confused,
	AttackHeavy,
	SpecialAttack,
	Special
}