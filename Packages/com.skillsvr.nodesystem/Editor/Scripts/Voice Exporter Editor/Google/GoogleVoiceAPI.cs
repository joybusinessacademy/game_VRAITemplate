using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace DialogExporter.Google
{
	[InitializeOnLoad]
	public static class GoogleAuthenticate
	{
		public static string AccessToken = string.Empty;
		private const string projectOpened = "ProjectOpened";
		static GoogleAuthenticate()
		{
			if (!SessionState.GetBool(projectOpened, false))
			{
				SessionState.SetBool(projectOpened, true);
#if UNITY_EDITOR
				EditorCoroutineUtility.StartCoroutineOwnerless(AuthenticateCoroutine());
#else
				Authenticate();
#endif
			}
		}

		static IEnumerator AuthenticateCoroutine()
		{
			yield return new EditorWaitForSeconds(1);
			Authenticate();
		}

		public static void Authenticate()
		{
			string email = "vrcore@vrcoretexttospeech.iam.gserviceaccount.com";
			var obj = Resources.Load<TextAsset>("vrcore");
			EditorCoroutineUtility.StartCoroutineOwnerless(AuthenticateServiceAccount(email, "https://www.googleapis.com/auth/drive.readonly https://www.googleapis.com/auth/drive",
			obj.bytes,
			(x) => { AccessToken = x.access_token; },
				(s) => { Debug.LogError(s); }));
		}

		public static IEnumerator AuthenticateServiceAccount(string serviceAccountEmail, string scope, byte[] p12bytes, Action<AccessTokenResponse> onRequestCompleted, Action<string> onRequestFailed)
		{
			var certificate = new X509Certificate2(p12bytes, "notasecret");
			var jwtToken = new GoogleOAuthJsonWebToken(certificate);

			Dictionary<string, string> content = new Dictionary<string, string>();
			//Fill key and value
			content.Add("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer");
			content.Add("assertion", jwtToken.CreateSignedTokenString(serviceAccountEmail, scope));

			using (UnityWebRequest www = UnityWebRequest.Post("https://oauth2.googleapis.com/token", content))
			{
				yield return www.SendWebRequest();

				if (www.result == UnityWebRequest.Result.ConnectionError)
				{
					onRequestFailed?.Invoke(www.error);
				}
				else
				{
					if (www.result == UnityWebRequest.Result.ProtocolError)
					{
						onRequestFailed?.Invoke(www.downloadHandler.text);
					}
					else
					{
						string resultContent = www.downloadHandler.text;
						var result = JsonUtility.FromJson<AccessTokenResponse>(resultContent);

						onRequestCompleted?.Invoke(result);
					}
				}
			};
		}

	}
	public class GoogleVoiceAPI : AbstractVoiceAPIConfig<GoogleTextToSpeechRequestData>, ITextToSpeechAPI
	{
		public static string GoogleApiKey {
			get => EditorPrefs.GetString("GoogleApiKey", "AIzaSyAEVyL5jDBdYjL6181I5i7ipiK7V5k_3bY");
			set => EditorPrefs.SetString("GoogleApiKey", value);
		}

		protected override void AddCustomHeaders(UnityWebRequest request, GoogleTextToSpeechRequestData data)
		{

		}
		internal override string GetApiUrl()
		{
			return "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=" + GoogleApiKey;
		}

		public void ProcessRequest(UnityWebRequest webRequest, TextToSpeechTerm termData)
		{
			TextToSpeechResponse response = JsonUtility.FromJson<TextToSpeechResponse>(webRequest.downloadHandler.text);
			response.voiceApi = VoiceAPI.GOOGLE;
			response.format = AudioType.WAV;
			VoiceExporter.OnVoiceRequestSuccessful(webRequest, response, termData.localizedDialog);
			VoiceExporter.CleanWebRequest(webRequest);
		}

		public UnityWebRequest CreateWebRequest(TextToSpeechTerm textToSpeechTerm)
		{
			bool useSSML = !string.IsNullOrEmpty(textToSpeechTerm.ssmlAlternateText);
			string text = useSSML ? textToSpeechTerm.ssmlAlternateText : textToSpeechTerm.Text;

			text = text.Replace("[", string.Empty);
			text = text.Replace("]", string.Empty);
			text = text.Replace("(", string.Empty);
			text = text.Replace(")", string.Empty);


			GoogleTextToSpeechRequestData data = new GoogleTextToSpeechRequestData();
			data.audioConfig.audioEncoding = "LINEAR16";
			data.audioConfig.pitch = 0;
			data.audioConfig.speakingRate = 1;

			if (useSSML)
			{
				data.input.ssml = text;
			}
			else
			{
				data.input.text = text;
			}


			data.voice.languageCode = "en-GB";
			data.voice.name = "en-GB-Wavenet-A";

			UnityWebRequest webRequest = RequestVoiceFile(textToSpeechTerm.TermName, data);

			if (string.IsNullOrEmpty(GoogleAuthenticate.AccessToken) == false)
				webRequest.SetRequestHeader("Authentication", string.Format("Bearer {0}", GoogleAuthenticate.AccessToken));

			return webRequest;
		}
	}

	[Serializable]
	public struct AccessTokenResponse
	{
		public string access_token;
	}

	[Serializable]
	public class GoogleTextToSpeechRequestData
	{
		[Serializable]
		public struct AudioConfig
		{
			public string audioEncoding;
			public float pitch;
			public float speakingRate;
		}

		[Serializable]
		public class Input
		{
			public string text;
			public string ssml;
		}


		[Serializable]
		public struct Voice
		{
			public string languageCode;
			public string name;
		}

		public AudioConfig audioConfig;
		public Input input = new Input();
		public Voice voice;


	}

	public class GoogleOAuthJsonWebToken
	{
		private X509Certificate2 certificate;

		public GoogleOAuthJsonWebToken(X509Certificate2 p12Certificate)
		{
			certificate = p12Certificate;
		}

		struct JWTHeader
		{
			public string alg;
			public string typ;

		}
		struct JWTClaimSet
		{
			public string iss;
			public string scope;
			public string aud;
			public int iat;
			public int exp;
		}

		public static string Encode(byte[] arg)
		{
			if (arg == null)
			{
				throw new ArgumentNullException("arg");
			}

			var s = Convert.ToBase64String(arg);
			return s
				.Replace("=", "")
				.Replace("/", "_")
				.Replace("+", "-");
		}

		public static string ToBase64(string arg)
		{
			if (arg == null)
			{
				throw new ArgumentNullException("arg");
			}

			var s = arg
					.PadRight(arg.Length + (4 - arg.Length % 4) % 4, '=')
					.Replace("_", "/")
					.Replace("-", "+");

			return s;
		}

		public static byte[] Decode(string arg)
		{
			var decrypted = ToBase64(arg);

			return Convert.FromBase64String(decrypted);
		}

		public string CreateSignedTokenString(string serviceAccountEmail, string scope)
		{
			string jwtString = "";
			var header = new JWTHeader();
			header.alg = "RS256";
			header.typ = "JWT";

			var claimSet = new JWTClaimSet();
			claimSet.iss = serviceAccountEmail;
			claimSet.scope = scope;
			claimSet.aud = "https://oauth2.googleapis.com/token";
			claimSet.iat = (int)ConvertToUnixTime(DateTime.Now);
			claimSet.exp = claimSet.iat + 60 * 60;

			var headerURLEncoded = Encode(Encoding.UTF8.GetBytes(JsonUtility.ToJson(header)));
			jwtString += headerURLEncoded + ".";


			var claimSetURLEncoded = Encode(Encoding.UTF8.GetBytes(JsonUtility.ToJson(claimSet)));
			jwtString += claimSetURLEncoded;

			RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
			byte[] privateKeyBlob = rsa.ExportCspBlob(true);
			var key = new RSACryptoServiceProvider();
			key.ImportCspBlob(privateKeyBlob);

			// signiture
			var inputBytes = Encoding.UTF8.GetBytes(jwtString);
			var signatureBytes = rsa.SignData(inputBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			var signatureEncoded = Encode(signatureBytes);

			jwtString += "." + signatureEncoded;

			return jwtString;
		}

		public double ConvertToUnixTime(DateTime time)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
			TimeSpan span = (time.ToLocalTime() - epoch);
			return span.TotalSeconds;
		}
	}
}

