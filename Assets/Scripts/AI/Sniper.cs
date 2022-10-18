using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : AIController
{
    float xTargetOffset = 25;
    Vector2 previousTargetPosition;
    float previousXToTarget;

    protected override void PickTarget(List<Character> viableTargets)
    {
        int lowestHealth = int.MaxValue;
        foreach (Character character in viableTargets)
        {
            int targetHealth = character.GetHealth();
            if (lowestHealth > targetHealth)
            {
                lowestHealth = targetHealth;
                target = character;
            }
        }
        previousTargetPosition = target.transform.position;
    }

    override protected void AcquireTargetPosition()
    {
        targetPosition = target.transform.position -= GetVectorXToTarget() * xTargetOffset;
    }

    protected override void SpecificAttack()
    {
        previousXToTarget = GetVectorXToTarget().x;
        Vector2 direction = Vector3.up + GetVectorXToTarget();
        Shoot(direction * maxShootForce);
    }

    public override void Learn(Vector2 hitPoint)
    {
        float missAmount = hitPoint.x - previousTargetPosition.x;
        float offsetChange = 2 * Mathf.Abs(missAmount);
        if (Mathf.Sign(missAmount) + Mathf.Sign(previousXToTarget) == 0)
        {
            xTargetOffset -= offsetChange;
        }
        else
        {
            xTargetOffset += offsetChange;
        }
    }

    Vector3 GetVectorXToTarget()
    {
        return target.transform.position.x - transform.position.x > 0 ? Vector2.right : Vector2.left;
    }
}
