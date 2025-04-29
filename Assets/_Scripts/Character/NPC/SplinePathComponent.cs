using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowPathState : EnemyBaseState
{
    private readonly NavMeshAgent agent;
    private readonly SplinePathComponent path;

    private int currentIndex;
    
    private float currentDistance;
    private bool forward = true;
    private float moveSpeed = 1f;

    public EnemyFollowPathState(AITesting enemy, Animator animator, NavMeshAgent agent, SplinePathComponent splinePath) : base(enemy, animator)
    {
        this.agent = agent;
        this.path = splinePath;
    }

    public override void OnEnter()
    {
        Debug.Log($"Following path");
        //animator.CrossFade(WalkHash, crossFadeDuration);
        
        if (path == null || path.points.Length == 0) return;
        SetNextDestination();
        PlayAnimation(AnimationState.Walk);
    }

    public override void Update()
    {/*
        if (path == null) return;

        if (forward)
        {
            currentDistance += moveSpeed * Time.deltaTime;
            if (currentDistance >= 1f)
            {
                forward = false;
                currentDistance = 1f;
            }
        }
        else
        {
            currentDistance -= moveSpeed * Time.deltaTime;
            if (currentDistance <= 0f)
            {
                forward = true;
                currentDistance = 0f;
            }
        }

        Vector3 targetPosition = path.GetPointAtDistance(currentDistance);
        agent.SetDestination(targetPosition);*/

        if (HasReachedCurrentPoint())
        {
            currentIndex = (currentIndex + 1) % path.points.Length;
            SetNextDestination();
        }
    }

    private void SetNextDestination()
    {
        if (path.points.Length > currentIndex)
            agent.SetDestination(path.points[currentIndex].position);
    }

    private bool HasReachedCurrentPoint()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }
}

public class SplinePathComponent : MonoBehaviour
{
    [SerializeField] public Transform[] points;
    
    public Vector3 GetPointAtDistance(float distance)
    {
        if (points == null || points.Length < 2) return transform.position;
        
        float segmentLength = 1f / (points.Length - 1);
        int index = Mathf.FloorToInt(distance / segmentLength);
        //float segmentDistance = distance % segmentLength;
        float localDistance = (distance - index * segmentLength) / segmentLength;
        
        if (index >= points.Length - 1) return points[points.Length - 1].position;
        
        return Vector3.Lerp(points[index].position, points[index + 1].position, localDistance);
    }
}