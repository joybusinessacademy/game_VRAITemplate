using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "VideoPlayerItem/VideoPlayerItem")]
public class VideoPlayerItem : ScriptableObject
{
	public enum VideoLocation
	{
        AbsolutePathOrURL,
        RelativeToProjectFolder,
        RelativeToStreamingAssetsFolder,
        RelativeToDataFolder,
        RelativeToPersistentDataFolder,
    }

	public VideoLocation videoLocationType = VideoLocation.RelativeToPersistentDataFolder;

	[Serializable]
	public class VideoPlayerSettings
	{
		public string name;
		public Material videoSkyboxMaterial;
		public RenderTexture videoRenderTexture;
		public GameObject sphereVideoPrefab;

		[Tooltip("The zero value of north may mean different positions in shaders.\r\n" +
			"i.e. The persion may stand at 166 in AvPro shader (for runtime) but 266 at video player shader (for editor).\r\n" +
			"This alignment can tweak and align the diff between shaders.\r\n" +
			"Setup your own value if the shader is not facing the 'north'.")]
		public int virtualNorthAlignment;

    }

	public RenderTexture videoRenderAlternateTexture;
    public List<VideoPlayerSettings> settings = new List<VideoPlayerSettings>();

	public VideoPlayerSettings GetSettingByName(string settingName)
	{
		var data = settings.FirstOrDefault(x => null != x && x.name == settingName);
		if (null == data)
		{
            Debug.LogError($"Cannot find video setting by name {settingName}, add setting to {this.name} (t:{this.GetType().Name}) first.");
        }
		return data;
	}
}
