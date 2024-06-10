using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using SkillsVR.TimelineTool;
using System.Linq;


public class RuntimeAnimationClipConverter : MonoBehaviour
{
    TimelineAsset timelineAsset;
    PlayableDirector director;
    List<TimelineDetailsSaved> savedDetails = new List<TimelineDetailsSaved>();

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.played += ScanAndConvertAnimationTracks;
        director.stopped += RevertClip;
    }

    private void OnDisable()
    {
        director.played -= ScanAndConvertAnimationTracks;
        director.stopped -= RevertClip;
    }


    void ScanAndConvertAnimationTracks(PlayableDirector director)
    {
        timelineAsset = (TimelineAsset)director.playableAsset;
   
        // Access the clips in the TimelineAsset
        foreach (TrackAsset track in timelineAsset.GetOutputTracks())
        {         
            // Check if the track is a playable track
            if (track is AnimationTrack animationTrack)
            {
                animationTrack.trackOffset = TrackOffset.ApplySceneOffsets;

                // Access the clips within the playable track
                foreach (TimelineClip clip in animationTrack.GetClips())
                {
                    savedDetails.Add(new TimelineDetailsSaved(clip.easeInDuration, clip.easeOutDuration, clip.postExtrapolationMode, clip.preExtrapolationMode));

                    clip.easeInDuration = clip.easeInDuration <= 0.0f ? 0.2f : clip.easeInDuration;
                    clip.easeOutDuration = clip.easeOutDuration <= 0.0f ? 0.2f : clip.easeOutDuration;
                    clip.SetPostExtrapolationMode(TimelineClip.ClipExtrapolation.None);
                    clip.SetPretExtrapolationMode(TimelineClip.ClipExtrapolation.None);
                }
            }
        }
    }

    void RevertClip(PlayableDirector director)
    {
        foreach (TrackAsset track in timelineAsset.GetOutputTracks())
        {

            // Check if the track is a playable track
            if (track is AnimationTrack animationTrack)
            {
                animationTrack.trackOffset = TrackOffset.ApplySceneOffsets;

                int index = 0;
                // Access the clips within the playable track
                foreach (TimelineClip clip in animationTrack.GetClips())
                {
                    clip.easeInDuration = savedDetails[index].EaseIn;
                    clip.easeOutDuration = savedDetails[index].EaseOut;
                    clip.SetPostExtrapolationMode(savedDetails[index].PostExtrapolation);
                    clip.SetPretExtrapolationMode(savedDetails[index].PretExtrapolation);
                    index++;
                }
            }
        }

        savedDetails = new List<TimelineDetailsSaved>();
    }

    class TimelineDetailsSaved
    {
        public TimelineDetailsSaved(double eIn, double eout, TimelineClip.ClipExtrapolation post, TimelineClip.ClipExtrapolation pret)
        {
            easeIn = eIn;
            easeOut = eout;
            postExtrapolation = post;
            pretExtrapolation = pret;
        }

        double easeIn, easeOut;
        TimelineClip.ClipExtrapolation postExtrapolation, pretExtrapolation;

        public double EaseOut { get => easeOut; set => easeOut = value; }
        public double EaseIn { get => easeIn; set => easeIn = value; }
        public TimelineClip.ClipExtrapolation PostExtrapolation { get => postExtrapolation; set => postExtrapolation = value; }
        public TimelineClip.ClipExtrapolation PretExtrapolation { get => pretExtrapolation; set => pretExtrapolation = value; }
    }

}

