using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITelemetry
{
	string id { get; set; }
	string data { get; set; }
	bool isCompleted { get; set; }

	bool IsValidated();
	void SendEvents();
}
