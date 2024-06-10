using System;
using UnityEngine;

namespace DialogExporter
{
	[Serializable]
	public class LocalisedString
	{
		[SerializeReference]
		public ILocalisationSource LocalisationSource = new DefaultLocalisationSource();

		public LocalisedString(string text = "New dialog")
		{
			LocalisationSource = new DefaultLocalisationSource(text);
		}
		
		public LocalisedString(ILocalisationSource localisationSource)
		{
			LocalisationSource = localisationSource;
		}

		public override string ToString() => LocalisationSource?.ToString() ?? "";
		
		public static implicit operator string(LocalisedString source)
		{
			return source.LocalisationSource?.Translation() ?? "";
		}
		
		public static implicit operator LocalisedString(string source)
		{
			return new LocalisedString(source);
		}

	}
}