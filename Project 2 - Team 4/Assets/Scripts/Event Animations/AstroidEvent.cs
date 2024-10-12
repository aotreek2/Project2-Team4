using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidEvent : StateMachineBehaviour
{
    [SerializeField] GameObject asteroid;
    [SerializeField] GameObject fireParticle;
    [SerializeField] GameObject smokeParticle;
    GameObject asteroidClone;

    [SerializeField] Vector3 fireOffset;
    [SerializeField] Vector3 fireRotation;
    [SerializeField] Vector3 smokeOffset;
    [SerializeField] Vector3 smokeRotation;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        asteroidClone = Instantiate(asteroid, animator.rootPosition + new Vector3(11, 27.5f, 0), Quaternion.identity);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Destroy(asteroidClone);
        GameObject fireEngine = Instantiate(fireParticle, animator.rootPosition + fireOffset, Quaternion.Euler(fireRotation));
        if (smokeParticle != null)
        {
            GameObject smokeEngine = Instantiate(smokeParticle, animator.rootPosition + smokeOffset, Quaternion.Euler(smokeRotation));
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
