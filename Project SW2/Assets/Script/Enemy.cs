using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    // ���� (�⺻, �߰�, ����)
    public enum State { Idle, Chasing, Attacking };
    State currentState;

    NavMeshAgent pathfinder;
    Transform target;
    Material skinMaterial;

    Color originColor;

    float attackDistanceThreshold = 0.5f; // ���� �����Ÿ�
    float timeBetweenAttacks = 1; // ���� ������

    float nextAttackTime; // ���� ������ ������ �ð�
    float myCollisionRadius; // �ڽ��� �浹 ����
    float targetCollisionRadius; // ��ǥ�� �浹 ����

    protected override void Start()
    {
        base.Start();
        pathfinder = GetComponent();
        skinMaterial = GetComponent().material;
        originColor = skinMaterial.color;

        currentState = State.Chasing;
        target = GameObject.FindGameObjectWithTag("Player").transform;

        myCollisionRadius = GetComponent().radius;
        targetCollisionRadius = target.GetComponent().radius;
        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        if (Time.time > nextAttackTime)
        {
            // (��ǥ ��ġ - �ڽ��� ��ġ) ������ �� ��
            float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

            if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                StartCoroutine(Attack());
            }
        }
    }
    // �� ����
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false; // �׺���̼� ���� ����

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;

        while (percent <= 1)
        {
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }
        skinMaterial.color = originColor;
        currentState = State.Chasing;
        pathfinder.enabled = true; // �׺���̼� ���� ����
    }
    // �� ����
    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (target != null)
        {
            if (currentState == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
