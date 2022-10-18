using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum State { Idle, Move, Attack, Dead }

public abstract class AIController : Character
{
    State state = State.Idle;
    float targetPositionTolerance = 5f;
    protected Character target;
    protected Vector2 targetPosition;
    bool isReadyToShoot = false;
    bool canShoot = false;
    int maxRayCastCount = 5;
    bool canJump = true;
    float jumpRecharge = 0.25f;
    float jumpTimer = 0;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
    {
        if (jumpTimer > jumpRecharge)
        {
            jumpTimer = 0;
            canJump = true;
        }
        jumpTimer += Time.deltaTime;

        if (health <= 0)
            state = State.Dead;
        else if (!hasTurn || hasTurn && !canShoot)
            state = State.Idle;
        else if (isReadyToShoot)
            state = State.Attack;
        else if (target)
        {
            if (targetPosition == Vector2.zero)
            {
                AcquireTargetPosition();
            }
            state = State.Move;
        }
        else
            AcquireTarget();

        switch (state)
        {
            case State.Move:
                if (targetPosition.x + targetPositionTolerance < transform.position.x)
                {
                    MoveLeft();
                    if (dynamicBody.GetIsColliding())
                    {
                        Vector3 endFloorRay = transform.position + Vector3.left + Vector3.down;
                        Vector3 endSightRay = transform.position + Vector3.left * 3;
                        Vector3 endCeilingRay = transform.position + Vector3.up * 2;
                        RayResult floorRay = physics.RayCast(transform.position, endFloorRay);
                        RayResult sightRay = physics.RayCast(transform.position, endSightRay);
                        RayResult ceilingRay = physics.RayCast(transform.position, endCeilingRay);
                        if (!ceilingRay.wasHit)
                        {
                            if (sightRay.wasHit)
                            {
                                endSightRay += Vector3.up;
                                Vector2 jumpVector = endSightRay - transform.position;
                                int rayCastCount = 0;
                                while (physics.RayCast(transform.position, endSightRay).wasHit && rayCastCount < maxRayCastCount)
                                {
                                    endSightRay += Vector3.up;
                                    jumpVector += Vector2.up;
                                    rayCastCount++;
                                }

                                jumpVector += 2 * Vector2.up;
                                JumpCheck(jumpVector);
                            }
                            else if (!floorRay.wasHit)
                            {
                                endFloorRay += Vector3.left;
                                Vector2 jumpVector = endFloorRay - transform.position;
                                int rayCastCount = 0;
                                while (!physics.RayCast(transform.position, endFloorRay).wasHit && rayCastCount < maxRayCastCount)
                                {
                                    endFloorRay += Vector3.left;
                                    jumpVector += Vector2.left + Vector2.down;
                                    rayCastCount++;
                                }

                                jumpVector.y = -jumpVector.y;
                                JumpCheck(jumpVector);
                            }
                        }
                        else if (sightRay.wasHit)
                        {
                            Vector2 newPosition = transform.position + 20 * Vector3.right;
                            targetPosition = newPosition;
                        }
                    }
                }
                else if (targetPosition.x - targetPositionTolerance > transform.position.x)
                {
                    MoveRight();
                    if (dynamicBody.GetIsColliding())
                    {
                        Vector3 endFloorRay = transform.position + Vector3.right + Vector3.down;
                        Vector3 endSightRay = transform.position + Vector3.right * 3;
                        Vector3 endCeilingRay = transform.position + Vector3.up * 2;
                        RayResult floorRay = physics.RayCast(transform.position, endFloorRay);
                        RayResult sightRay = physics.RayCast(transform.position, endSightRay);
                        RayResult ceilingRay = physics.RayCast(transform.position, endCeilingRay);
                        if (!ceilingRay.wasHit)
                        {
                            if (sightRay.wasHit)
                            {
                                endSightRay += Vector3.up;
                                Vector2 jumpVector = endSightRay - transform.position;
                                int rayCastCount = 0;
                                while (physics.RayCast(transform.position, endSightRay).wasHit && rayCastCount < maxRayCastCount)
                                {
                                    endSightRay += Vector3.up;
                                    jumpVector += Vector2.up;
                                    rayCastCount++;
                                }

                                jumpVector += Vector2.up;
                                JumpCheck(jumpVector);
                            }
                            else if (!floorRay.wasHit)
                            {
                                endFloorRay += Vector3.right;
                                Vector2 jumpVector = endFloorRay - transform.position;
                                int rayCastCount = 0;
                                while (!physics.RayCast(transform.position, endFloorRay).wasHit && rayCastCount < maxRayCastCount)
                                {
                                    endFloorRay += Vector3.right;
                                    jumpVector += Vector2.right + Vector2.down;
                                    rayCastCount++;
                                }

                                jumpVector.y = -jumpVector.y;
                                JumpCheck(jumpVector);
                            }
                        }
                        else if (sightRay.wasHit)
                        {
                            Vector2 newPosition = transform.position + 20 * Vector3.left;
                            targetPosition = newPosition;
                        }
                    }
                }
                else
                {
                    targetPosition = Vector2.zero;
                    isReadyToShoot = true;
                }
                //}
                break;
            case State.Attack:
                GenericAttack();
                break;
            case State.Dead:
                Die();
                break;
            case State.Idle:
            default:
                break;
        }

        base.Update();
    }

    void JumpCheck(Vector2 direction)
    {
        if (canJump)
            Jump(direction);

        canJump = false;
    }

    private void GenericAttack()
    {
        SpecificAttack();
        target = null;
        isReadyToShoot = false;
        canShoot = false;
        hasTurn = false;
    }

    override public void SetHasTurn(bool turn)
    {
        hasTurn = turn;
        canShoot = turn;
    }

    void AcquireTarget()
    {
        List<Character> viableTargets = new List<Character>();
        foreach (Character character in FindObjectsOfType<Character>())
        {
            if (character.GetIsOnPlayerTeam() != isOnPlayerTeam)
            {
                viableTargets.Add(character);
            }
        }
        PickTarget(viableTargets);
    }

    void FindParabola()
    {
        // get current position
        // get angle of throw
        // get gravity
        // get throw velocity
        // do equation => get y offset
    }

    void MoveLeft()
    {
        if (dynamicBody)
        {
            dynamicBody.AddVelocity(Vector2.left * speed * Time.deltaTime);
        }
    }

    void MoveRight()
    {
        if (dynamicBody)
            dynamicBody.AddVelocity(Vector2.right * speed * Time.deltaTime);
    }

    protected abstract void SpecificAttack();

    abstract protected void AcquireTargetPosition();

    abstract protected void PickTarget(List<Character> viableTargets);
}
