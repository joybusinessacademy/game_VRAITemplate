using SkillsVR.Messeneger;

public class TrueFalseTelemetry : ITelemetry
{
	public string id { get; set; }
	public string data { get; set; }
	public bool isCompleted { get; set; }

	private bool innerResult { get; set; }
	public bool hasResult { get; set; } = false;
	public bool result
	{
		get => innerResult;
		set
		{
			innerResult = value; 
			hasResult = true;
		}
	}

	public bool IsValidated()
	{
		return hasResult;
	}

	public void SendEvents()
	{
		TelemetryData telemetryData = new TelemetryData();
		telemetryData.mechanicID = id;
		telemetryData.id = "TrueFalseTelemetry";
		telemetryData.data.Add("Result", result.ToString());

		GlobalMessenger.Instance?.Broadcast("TelemetryData", telemetryData);

		isCompleted = true;
	}
}
