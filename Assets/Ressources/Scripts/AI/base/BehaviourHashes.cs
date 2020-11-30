using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourHashes
{
    static public readonly int INITIAL = Animator.StringToHash("INITIAL");
    static public readonly int RUN_TO_TARGET = Animator.StringToHash("RUN_TO_TARGET");
    static public readonly int TACKLE_CHARACTER = Animator.StringToHash("TACKLE_CHARACTER");
    static public readonly int RUN_TO_BALL = Animator.StringToHash("RUN_TO_BALL");
    static public readonly int RUN_TO_GOAL = Animator.StringToHash("RUN_TO_GOAL");
    static public readonly int THROW_BALL_TO_GOAL = Animator.StringToHash("THROW_BALL_TO_GOAL");
    static public readonly int WAIT_DURING_TACKLE = Animator.StringToHash("WAIT_DURING_TACKLE");
}