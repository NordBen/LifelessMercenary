using UnityEngine;
using UnityEngine.AI;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private float detectionAngle = 60f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float innerDetectionRange = 5f;
    [SerializeField] private float detectionCooldown = 1f;
    [SerializeField] private float attackRange = 2f;
    private Transform _player;
    //[SerializeField] private LayerMask detectionMask;

    public Transform Player
    {
        get
        {
            if (_player == null)
            {
                _player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            return _player;
        }
        private set => _player = value;

    }
    CountdownTimer detectionTimer;

    private IDetectionStrategy detectionStrategy;

    private void Start()
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRange, innerDetectionRange);
        Debug.Log($"Player: {Player}");
        Debug.Log($"Detection Strategy: {detectionStrategy}");
    }

    private void Update() => detectionTimer.Tick(Time.deltaTime);

    public bool CanDetectPlayer()
    {
        return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
    }

    public void SetDetectionStrategy(IDetectionStrategy strategy) => this.detectionStrategy = strategy;

    public bool InAttackingDistance()
    {
        var directionToTarget = Player.position - transform.position;
        return directionToTarget.magnitude <= detectionRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, innerDetectionRange);
        
        Vector3 forwardConeDirection = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        Vector3 backwardConeDirection = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward * detectionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
        Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
    }
}

public interface IDetectionStrategy
{
    bool Execute(Transform player, Transform detector, CountdownTimer timer);
}

public class ConeDetectionStrategy : IDetectionStrategy
{
    private readonly float detectionAngle;
    private readonly float detectionRange;
    private readonly float innerDetectionRange;

    public ConeDetectionStrategy(float detectionAngle, float detectionRange, float innerDetectionRange)
    {
        this.detectionAngle = detectionAngle;
        this.detectionRange = detectionRange;
        this.innerDetectionRange = innerDetectionRange;
    }

    public bool Execute(Transform player, Transform detector, CountdownTimer timer)
    {
        if (timer.IsRunning) return false;

        var directionToTarget = player.position - detector.position;
        var angleToTarget = Vector3.Angle(directionToTarget, detector.forward);

        if (!(angleToTarget < detectionAngle * 0.5f) || !(directionToTarget.magnitude < detectionRange)
            && !(directionToTarget.magnitude < innerDetectionRange)) return false;

        timer.Start();
        return true;
    }
}