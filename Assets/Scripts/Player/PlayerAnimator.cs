using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player animation control and provides interface for animation triggers
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Plays animation by setting the specified trigger
    /// </summary>
    /// <param name="triggerName">Name of the animation trigger to activate</param>
    public void PlayAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning("Animator not assigned!");
        }
    }

    /// <summary>
    /// Sets boolean parameter value in the animator
    /// </summary>
    /// <param name="paramName">Name of the boolean parameter</param>
    /// <param name="value">Value to set the parameter to</param>
    public void SetBool(string paramName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, value);
        }
        else
        {
            Debug.LogWarning("Animator not assigned!");
        }
    }
}