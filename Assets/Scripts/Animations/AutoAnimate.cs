using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAnimate : MonoBehaviour
{
    public List<string> animations = new List<string> {
        "Standing_Neutral_ApprehensiveGesture_02",
        "Standing_Neutral_DeepHeadTilt_02",
        "Standing_Neutral_RightHandOut_02"
    };

    private Animator animator;
    private AudioSource audioSource;

    private bool wasAudioPlaying = false;
    
    public string currentClipName = string.Empty;

    // Update is called once per frame
    void Update()
    {
        animator = animator ?? GetComponent<Animator>();
        audioSource = audioSource ?? GetComponent<AudioSource>();

        
        if (audioSource.isPlaying && wasAudioPlaying == false && string.IsNullOrEmpty(currentClipName))
        {
            currentClipName = audioSource.clip.name;
            StartCoroutine(NewClipPlayed(audioSource.clip));
        }

        var target = audioSource.isPlaying ? 1 : 0;
        var current = Mathf.Lerp(animator.GetLayerWeight(1), target, target == 0 ? Time.deltaTime * 2 : Time.deltaTime * 10);
        
        animator.SetLayerWeight(1, Mathf.Clamp01(current));
        wasAudioPlaying = audioSource.isPlaying;
    }

    IEnumerator NewClipPlayed(AudioClip clip)
    {
        float polledAnimation = 0;
        while (!string.IsNullOrEmpty(currentClipName))
        {
            var randomHash = animations[Random.Range(0, animations.Count)];
            animator.CrossFade(randomHash, 0.15f, 1, 0);

            var currentClipInfo = animator.GetCurrentAnimatorClipInfo(1);
            var clipLenghth = currentClipInfo[0].clip.length;

            float randomGap = Random.Range(0.5f, 2f);
            polledAnimation += clipLenghth + randomGap;

            if (polledAnimation > clip.length)
                break;

            yield return new WaitForSeconds(clipLenghth * .85f);

            yield return new WaitForSeconds(randomGap);

        }

        currentClipName = string.Empty;

    }
}
