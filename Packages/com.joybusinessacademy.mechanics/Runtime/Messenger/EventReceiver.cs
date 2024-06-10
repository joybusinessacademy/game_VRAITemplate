using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using SkillsVR.UnityExtenstion;

namespace SkillsVR.Messeneger
{
	public class EventKey
	{
		public string eventType;
		public string groupName;
		public EventKey(string eventType, string groupName = null)
		{
			this.eventType = eventType;
			this.groupName = groupName;
		}

		public static implicit operator string(EventKey eventKey)
		{
			return eventKey.eventType;
		}

		public static implicit operator EventKey(string eventType)
		{
			return new EventKey(eventType);
		}

		public override bool Equals(object obj)
		{
			var other = obj as EventKey;
			return other != null ? other.eventType == this.eventType && other.groupName == this.groupName : base.Equals(obj);
		}
	}
	public class EventReceiver : MonoBehaviour
	{

		#region EVENTS

		public Dictionary<string, List<Delegate>> eventTable = new Dictionary<string, List<Delegate>>();
		public Dictionary<string, List<Delegate>> groupEventTable = new Dictionary<string, List<Delegate>>();

		public Dictionary<string, List<EventKey>> groupEventKeysMap = new Dictionary<string, List<EventKey>>();

		[Header("Strict mode enforces type safety when broadcasting events - only listeners with the same signature will be fired")]
		public bool useStrictMode = false;

		[ContextMenu("Print Events")]
		public void PrintEventTable()
		{
			Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

			foreach (KeyValuePair<string, List<Delegate>> pair in eventTable)
			{
				Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
			}

			Debug.Log("\n");
		}

		public string[] GetRegisteredGroupNames()
		{
			return groupEventTable.Keys.ToArray();
		}

		public string[] GetRegisteredEventNames(string groupName = null)
		{
			return (string.IsNullOrEmpty(groupName) == false && groupEventKeysMap.ContainsKey(groupName)) ? groupEventKeysMap[groupName].Select((tmp) => tmp.eventType).ToArray() : eventTable.Keys.ToArray();
		}

		public bool EventExists(string eventName)
		{
			return eventTable.ContainsKey(eventName);
		}

		public void Cleanup()
		{
			List<string> messagesToRemove = new List<string>();

			foreach (KeyValuePair<string, List<Delegate>> pair in eventTable)
			{
				messagesToRemove.Add(pair.Key);
			}

			foreach (string message in messagesToRemove)
			{
				eventTable.Remove(message);
			}
		}


		public void AddGroupListener(string groupName, Callback handler)
		{
			AddGroupListenerInternal(groupName, handler);
		}
		public void AddGroupListener<T>(string groupName, Callback<T> handler)
		{
			AddGroupListenerInternal(groupName, handler);
		}

		public void AddGroupListener<T, U>(string groupName, Callback<T, U> handler)
		{
			AddGroupListenerInternal(groupName, handler);
		}

		public void AddGroupListener<T, U, V>(string groupName, Callback<T, U, V> handler)
		{
			AddGroupListenerInternal(groupName, handler);
		}

		public void RemoveGroupListener(string groupName, Callback handler)
		{
			RemoveGroupListenerInternal(groupName, handler);
		}
		public void RemoveGroupListener<T>(string groupName, Callback<T> handler)
		{
			RemoveGroupListenerInternal(groupName, handler);
		}

		public void RemoveGroupListener<T, U>(string groupName, Callback<T, U> handler)
		{
			RemoveGroupListenerInternal(groupName, handler);
		}

		public void RemoveGroupListener<T, U, V>(string groupName, Callback<T, U, V> handler)
		{
			RemoveGroupListenerInternal(groupName, handler);
		}

		private void AddGroupListenerInternal(string groupName, Delegate handler)
		{
			if (string.IsNullOrEmpty(groupName) == false)
			{
				if (groupEventTable.ContainsKey(groupName) == false)
				{
					groupEventTable[groupName] = new List<Delegate>();
				}
				var delegateList = groupEventTable[groupName];
				if (delegateList.Contains(handler) == false)
				{
					delegateList.Add(handler);
				}
			}
		}

		private void RemoveGroupListenerInternal(string groupName, Delegate handler)
		{
			if (string.IsNullOrEmpty(groupName) == false)
			{
				if (groupEventTable.ContainsKey(groupName))
				{
					var delegateList = groupEventTable[groupName];
					if (delegateList.Contains(handler))
					{
						delegateList.Remove(handler);
					}
				}
			}
		}

		public void AddListener(EventKey eventType, Callback handler)
		{
			AddListenerInternal<Callback>(eventType, handler);
		}

		/// <summary>
		/// Eg.
		/// Messenger<int>.AddListener("Dialog Next", NextChoice );
		/// </summary>
		/// <param name="eventType">Event type.</param>
		/// <param name="handler">Handler.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void AddListener<T>(EventKey eventType, Callback<T> handler)
		{
			AddListenerInternal(eventType, handler);
		}

		//Two parameters
		public void AddListener<T, U>(EventKey eventType, Callback<T, U> handler)
		{
			AddListenerInternal(eventType, handler);
		}

		//Three parameters
		public void AddListener<T, U, V>(EventKey eventType, Callback<T, U, V> handler)
		{
			AddListenerInternal(eventType, handler);
		}

		private void AddListenerInternal<T>(EventKey eventType, T handler) where T : Delegate
		{
			if (eventTable.ContainsKey(eventType) == false)
			{
				eventTable.Add(eventType, new List<Delegate>());
			}

			var groupName = eventType.groupName ?? "";
			if (groupEventKeysMap.ContainsKey(groupName) == false)
				groupEventKeysMap.Add(groupName, new List<EventKey>());

			var groupList = groupEventKeysMap[groupName];
			if (groupList.Contains(eventType) == false)
				groupList.Add(eventType);

			var list = eventTable[eventType];
			if (list.Contains(handler) == false)
			{
				list.Add(handler);
			}
		}

		public void RemoveListener(EventKey eventType, Callback handler)
		{
			RemoveListenerInternal(eventType, handler);
		}

		//Single parameter
		public void RemoveListener<T>(EventKey eventType, Callback<T> handler)
		{
			RemoveListenerInternal(eventType, handler);
		}

		//Two parameters
		public void RemoveListener<T, U>(EventKey eventType, Callback<T, U> handler)
		{
			RemoveListenerInternal(eventType, handler);
		}

		//Three parameters
		public void RemoveListener<T, U, V>(EventKey eventType, Callback<T, U, V> handler)
		{
			RemoveListenerInternal(eventType, handler);
		}

		private void RemoveListenerInternal<T>(EventKey eventType, T handler) where T : Delegate
		{
			if (!this.EventExists(eventType) || Application.isPlaying == false)
				return;


			if (eventTable.ContainsKey(eventType))
			{
				List<Delegate> delegateList = eventTable[eventType];

				var groupName = eventType.groupName ?? "";
				if (groupEventKeysMap.ContainsKey(groupName))
				{
					var groupList = groupEventKeysMap[groupName];
					if (groupList.Contains(eventType))
						groupList.Remove(eventType);

					if (groupList.Count == 0)
						groupEventKeysMap.Remove(eventType);
				}

				if (delegateList.Contains(handler) == false)
				{
					Debug.LogWarning(string.Format("Attempting to remove listener for event type \"{0}\" but current listener is null.", eventType));
				}
				else
				{
					delegateList.Remove(handler);
				}

			}
			else
			{
				throw new UnityException(string.Format("Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.", eventType));
			}

		}

		public class CallbackParameterTypes
		{
			public class CallbackParameter
			{
				public readonly Type parameterType;
				public readonly string parameterName;
				public readonly object parameterDefaultValue;
				public CallbackParameter(ParameterInfo parameter)
				{
					parameterType = parameter.ParameterType;
					parameterName = parameter.Name;
					if (parameter.HasDefaultValue)
						parameterDefaultValue = parameter.DefaultValue;
					else
						parameterDefaultValue = GetDefault(parameterType);
				}
				public static object GetDefault(Type type)
				{
					if (type.IsValueType)
					{
						return Activator.CreateInstance(type);
					}
					return null;
				}

				public override string ToString()
				{
					return string.Format("{0}({1}):{2}", parameterName, parameterType.Name, parameterDefaultValue);
				}
			}


			public readonly CallbackParameter[] parameters;
			public bool HasParameters => parameters.Length > 0;



			public CallbackParameterTypes(Delegate callback)
			{
				var parameterInfos = callback.Method.GetParameters();

				var parameterList = new List<CallbackParameter>();
				foreach (var parameterInfo in parameterInfos)
				{
					parameterList.Add(new CallbackParameter(parameterInfo));
				}
				parameters = parameterList.ToArray();
			}



			public override string ToString()
			{
				return parameters.Length > 0 ? string.Format("{{{0}}}", parameters.ToStringOfObjectsWithSeparator(",")) : "";
			}

			public bool ContainsParameters(string[] parameterNames)
			{
				foreach (var parameterName in parameterNames)
				{
					var hasParameterName = false;
					foreach (var param in parameters)
					{
						if (param.parameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
						{
							hasParameterName = true;
							break;
						}
					}
					if (hasParameterName == false)
					{
						return false;
					}

				}
				return true;
			}
		}

		public List<CallbackParameterTypes> GetCallbackParameterTypesForEvent(string eventType)
		{
			if (eventTable.TryGetValue(eventType, out List<Delegate> callbackList))
			{
				var list = new List<CallbackParameterTypes>();
				foreach (var callback in callbackList)
				{
					var callbackParameters = new CallbackParameterTypes(callback);
					var parameterString = callbackParameters.ToString();
					var hasParameterType = list.FirstOrDefault((tmp) => parameterString.Equals(tmp.ToString())) != null;
					if (hasParameterType == false)
					{
						list.Add(callbackParameters);
					}

				}
				return list;
			}
			return null;
		}

		public bool BroadcastWithDataType(EventKey eventType, object dataToSend, Type dataType)
		{
			Type elementType = typeof(Callback<>).MakeGenericType(dataType);
			return BroadcastInteral(elementType, eventType, new object[] { dataToSend });
		}


		public bool Broadcast(EventKey eventType)
		{
			return BroadcastInteral(typeof(Callback), eventType, null);
		}

		public bool BroadcastUntyped(EventKey eventType, object[] args)
		{
			return BroadcastInteral(typeof(Delegate), eventType, args);
		}

		//Single parameter
		public bool Broadcast<T>(EventKey eventType, T value)
		{
			return BroadcastInteral(typeof(Callback<T>), eventType, new object[] { value });
		}

		//Two parameters
		public bool Broadcast<T, U>(EventKey eventType, T arg1, U arg2)
		{
			return BroadcastInteral(typeof(Callback<T, U>), eventType, new object[] { arg1, arg2 });
		}

		//Three parameters
		public bool Broadcast<T, U, V>(EventKey eventType, T arg1, U arg2, V arg3)
		{
			return BroadcastInteral(typeof(Callback<T, U, V>), eventType, new object[] { arg1, arg2, arg3 });
		}

		private bool BroadcastInteral(Type callbackType, EventKey eventType, object[] args, bool forceTypeConvert = false)
		{
			if (string.IsNullOrEmpty(eventType))
			{
				Debug.LogError("Invalid eventType null; can't broadcast event");
				return false;
			}

			bool invokedSuccessfully = false;
			if (string.IsNullOrEmpty(eventType.groupName) == false && groupEventTable.TryGetValue(eventType.groupName, out List<Delegate> groupDelegateList))
			{
				invokedSuccessfully |= IterateDelegateListAndInvokeWithArgs(callbackType, eventType, groupDelegateList, args);
			}
			if (eventTable.TryGetValue(eventType, out List<Delegate> eventDelegateList))
			{
				invokedSuccessfully |= IterateDelegateListAndInvokeWithArgs(callbackType, eventType, eventDelegateList, args);
			}
			return invokedSuccessfully;
		}

		private List<object> parameterHelperList = new List<object>();
		private bool IterateDelegateListAndInvokeWithArgs(Type callbackType, EventKey eventType, List<Delegate> delegateList, object[] args)
		{
			bool invokedSuccessfully = false;
			for (var i = 0; i < delegateList.Count; i++)
			{
				var possibleCallback = delegateList[i];
#if !UNITY_EDITOR
			try
			{
#endif
				parameterHelperList.Clear();
				var parameters = possibleCallback.Method.GetParameters();
				if ((callbackType == possibleCallback.GetType() || callbackType == typeof(Delegate) || useStrictMode == false))
				{
					object[] parameterArgs = null;
					if (useStrictMode)
					{
						if (args != null && parameters.Length == args.Length)
						{
							var index = 0;
							foreach (var parameter in parameters)
							{
								var value = args[index];
								if (value != null && parameter.ParameterType != value.GetType())
								{
									value = parameter.ParameterType.IsEnum ? Enum.Parse(parameter.ParameterType, value.ToString()) : typeof(IConvertible).IsAssignableFrom(value.GetType()) ? Convert.ChangeType(value, parameter.ParameterType) : value;
								}
								parameterHelperList.Add(value);
								index++;
							}
							parameterArgs = parameterHelperList.ToArray();
						}
					}
					else
					{
						parameterArgs = args;
					}


					if (parameters.Length == 0 && (parameterArgs == null || parameterArgs.Length == 0))
					{
						possibleCallback.DynamicInvoke();
						invokedSuccessfully = true;
					}
					else if (parameterArgs != null && parameters.Length == parameterArgs.Length)
					{
						possibleCallback.DynamicInvoke(parameterArgs);
						invokedSuccessfully = true;
					}

					parameterHelperList.Clear();
				}

#if !UNITY_EDITOR
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat("Error occured while broadcasting event. EventType:'{0}' GroupName:'{1}' Values: '[{2}]' Message: {3}, Method: {4}", eventType.eventType, eventType.groupName, args != null ? args.ToStringOfObjectsWithSeparator(",") : "", e.Message, possibleCallback.Method.Name);
				return false;
			}
#endif
			}

			if (invokedSuccessfully == false)
			{
				Debug.LogWarningFormat("EventType:'{0}' GroupName:'{1}' Values: '[{2}]' No matching listener signature found to deliver the event - did you miss a parameter?", eventType.eventType, eventType.groupName, args != null ? args.ToStringOfObjectsWithSeparator(",") : "");
			}

			return invokedSuccessfully;
		}

		#endregion
	}
}