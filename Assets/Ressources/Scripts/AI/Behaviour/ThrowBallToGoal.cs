using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBallToGoal : AbstractAIBehaviour
{
    [SerializeField]
    private float _waitingDuration = 2;

    private float _startWaitTime = 0;

    public override int GetBehaviourHash()
    {
        return BehaviourHashes.THROW_BALL_TO_GOAL;
    }


    public override void onEnterState()
    {
        base.onEnterState();
        agentController.startThrowBall();
        _startWaitTime = Time.time;

    }

    public override void onExitState()
    {
        base.onExitState();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > _startWaitTime + _waitingDuration)
        {
            agentController.EndAction();
        }
    }
}