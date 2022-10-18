using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kamikaze : AIController
{
    override protected void PickTarget(List<Character> viableTargets)
    {
        float closestTargetSqr = float.MaxValue;
        foreach (Character character in viableTargets)
        {
            float targetDistanceSqr = (character.transform.position - transform.position).sqrMagnitude;
            if (closestTargetSqr > targetDistanceSqr)
            {
                closestTargetSqr = targetDistanceSqr;
                target = character;
            }
        }
    }

    override protected void AcquireTargetPosition()
    {
        targetPosition = target.transform.position;
    }

    protected override void SpecificAttack()
    {
        Vector2 direction = target.transform.position - transform.position;
        Shoot(direction);
    }
}
