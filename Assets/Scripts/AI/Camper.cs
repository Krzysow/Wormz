using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camper : AIController
{
    Vector2 antiCenter = Vector2.zero;

    protected override void PickTarget(List<Character> viableTargets)
    {
        Vector3 center = Vector2.zero;
        float farthestTargetSqr = 0;
        foreach (Character character in viableTargets)
        {
            center += character.transform.position;
            float targetDistanceSqr = (character.transform.position - transform.position).sqrMagnitude;
            if (farthestTargetSqr < targetDistanceSqr)
            {
                farthestTargetSqr = targetDistanceSqr;
                target = character;
            }
        }
        center /= viableTargets.Count;
        antiCenter = -center;
    }

    protected override void AcquireTargetPosition()
    {
        targetPosition = antiCenter;
    }

    protected override void SpecificAttack()
    {
        Rocket(2 * Vector2.up * transform.localScale.y);
    }
}
