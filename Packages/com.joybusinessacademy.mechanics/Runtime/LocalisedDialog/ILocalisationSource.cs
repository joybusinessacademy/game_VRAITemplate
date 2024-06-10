namespace DialogExporter
{
	public partial interface ILocalisationSource
	{
		bool CanBeEdited();
		string Translation(string term = "en");
		void EditDialog(string newDialog);
		void GetLocalisationItems();

		public string ToString()
		{
			return Translation();
		}


	}
}