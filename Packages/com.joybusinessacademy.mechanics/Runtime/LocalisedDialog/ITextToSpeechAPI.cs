using UnityEngine.Networking;

namespace DialogExporter
{
	public interface ITextToSpeechAPI
	{
		/// <summary>
		/// Will process the data, save it to a file and set it to the audio source of term data
		/// </summary>
		/// <param name="webRequest">The current web request</param>
		/// <param name="termData">The term data to be saved</param>
		void ProcessRequest(UnityWebRequest webRequest, TextToSpeechTerm termData);

		/// <summary>
		/// Creates the web request to be sent off based on the input data
		/// </summary>
		/// <param name="textToSpeechTerm">The text to speech term to be processed</param>
		/// <returns></returns>
		UnityWebRequest CreateWebRequest(TextToSpeechTerm textToSpeechTerm);
	}
}