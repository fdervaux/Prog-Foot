using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunToBall : AbstractAIBehaviour
{

    public override int GetBehaviourHash()
    {
        return BehaviourHashes.RUN_TO_BALL;
    }


    public override void onEnterState()
    {
        base.onEnterState();
        agentController.setTargetToBall();

    }

    public override void onExitState()
    {
        base.onExitState();
        agentController.setTargetToNull();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
