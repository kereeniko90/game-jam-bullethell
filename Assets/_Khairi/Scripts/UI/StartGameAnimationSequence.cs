using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameAnimationSequence : MonoBehaviour
{
    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    public void StartGameAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("StartAnimation");
        }
        else
        {
            Debug.LogWarning("Animator component not found on this GameObject.");
        }
    }

    public void LoadGameScene()
    {
        
        SceneManager.LoadScene("MainGame");
        Debug.Log("Loading game scene...");
    }
}
