using SkillsVR.Mechanic.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.Mechanic.Events
{
	#region Generation Area
    [Serializable] public class MechanicEventBool : SerializableEventT<bool> {[Serializable] public class TypedUnityEvent : UnityEvent<bool> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(bool data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventInt : SerializableEventT<int> {[Serializable] public class TypedUnityEvent : UnityEvent<int> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(int data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventFloat : SerializableEventT<float> {[Serializable] public class TypedUnityEvent : UnityEvent<float> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(float data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventString : SerializableEventT<string> {[Serializable] public class TypedUnityEvent : UnityEvent<string> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(string data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventObject : SerializableEventT<object> {[Serializable] public class TypedUnityEvent : UnityEvent<object> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(object data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventVector2 : SerializableEventT<Vector2> {[Serializable] public class TypedUnityEvent : UnityEvent<Vector2> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Vector2 data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventVector3 : SerializableEventT<Vector3> {[Serializable] public class TypedUnityEvent : UnityEvent<Vector3> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Vector3 data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventVector4 : SerializableEventT<Vector4> {[Serializable] public class TypedUnityEvent : UnityEvent<Vector4> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Vector4 data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventColor : SerializableEventT<Color> {[Serializable] public class TypedUnityEvent : UnityEvent<Color> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Color data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventGameObject : SerializableEventT<GameObject> {[Serializable] public class TypedUnityEvent : UnityEvent<GameObject> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(GameObject data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventComponent : SerializableEventT<Component> {[Serializable] public class TypedUnityEvent : UnityEvent<Component> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Component data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventTransform : SerializableEventT<Transform> {[Serializable] public class TypedUnityEvent : UnityEvent<Transform> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Transform data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventAudioClip : SerializableEventT<AudioClip> {[Serializable] public class TypedUnityEvent : UnityEvent<AudioClip> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(AudioClip data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventMaterial : SerializableEventT<Material> {[Serializable] public class TypedUnityEvent : UnityEvent<Material> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Material data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventTextAsset : SerializableEventT<TextAsset> {[Serializable] public class TypedUnityEvent : UnityEvent<TextAsset> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(TextAsset data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventException : SerializableEventT<Exception> {[Serializable] public class TypedUnityEvent : UnityEvent<Exception> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(Exception data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventIMechanicSystem : SerializableEventT<IMechanicSystem> {[Serializable] public class TypedUnityEvent : UnityEvent<IMechanicSystem> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(IMechanicSystem data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventIMechanicSystemEvent : SerializableEventT<IMechanicSystemEvent> {[Serializable] public class TypedUnityEvent : UnityEvent<IMechanicSystemEvent> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(IMechanicSystemEvent data){ eventHandle.Invoke(data);}}
    [Serializable] public class MechanicEventIMechanicSystemResult : SerializableEventT<IMechanicSystemResult> {[Serializable] public class TypedUnityEvent : UnityEvent<IMechanicSystemResult> { } public TypedUnityEvent eventHandle = new TypedUnityEvent(); public override void Invoke(IMechanicSystemResult data){ eventHandle.Invoke(data);}}


    [Serializable] public class MechanicEventListBool : MechanicEventListBase<bool, MechanicEventBool> { }
    [Serializable] public class MechanicEventListInt : MechanicEventListBase<int, MechanicEventInt> { }
    [Serializable] public class MechanicEventListFloat : MechanicEventListBase<float, MechanicEventFloat> { }
    [Serializable] public class MechanicEventListString : MechanicEventListBase<string, MechanicEventString> { }
    [Serializable] public class MechanicEventListObject : MechanicEventListBase<object, MechanicEventObject> { }
    [Serializable] public class MechanicEventListVector2 : MechanicEventListBase<Vector2, MechanicEventVector2> { }
    [Serializable] public class MechanicEventListVector3 : MechanicEventListBase<Vector3, MechanicEventVector3> { }
    [Serializable] public class MechanicEventListVector4 : MechanicEventListBase<Vector4, MechanicEventVector4> { }
    [Serializable] public class MechanicEventListColor : MechanicEventListBase<Color, MechanicEventColor> { }
    [Serializable] public class MechanicEventListGameObject : MechanicEventListBase<GameObject, MechanicEventGameObject> { }
    [Serializable] public class MechanicEventListComponent : MechanicEventListBase<Component, MechanicEventComponent> { }
    [Serializable] public class MechanicEventListTransform : MechanicEventListBase<Transform, MechanicEventTransform> { }
    [Serializable] public class MechanicEventListAudioClip : MechanicEventListBase<AudioClip, MechanicEventAudioClip> { }
    [Serializable] public class MechanicEventListMaterial : MechanicEventListBase<Material, MechanicEventMaterial> { }
    [Serializable] public class MechanicEventListTextAsset : MechanicEventListBase<TextAsset, MechanicEventTextAsset> { }
    [Serializable] public class MechanicEventListException : MechanicEventListBase<Exception, MechanicEventException> { }
    [Serializable] public class MechanicEventListIMechanicSystem : MechanicEventListBase<IMechanicSystem, MechanicEventIMechanicSystem> { }
    [Serializable] public class MechanicEventListIMechanicSystemEvent : MechanicEventListBase<IMechanicSystemEvent, MechanicEventIMechanicSystemEvent> { }
    [Serializable] public class MechanicEventListIMechanicSystemResult : MechanicEventListBase<IMechanicSystemResult, MechanicEventIMechanicSystemResult> { }
	#endregion Generation Area

    [Serializable]
	public class MechanicEventsHandler : MechanicEventHandlerBase
	{
		#region List Properties Generation Area
        public MechanicEventListBool listEventBool = new MechanicEventListBool();
        public MechanicEventListInt listEventInt = new MechanicEventListInt();
        public MechanicEventListFloat listEventFloat = new MechanicEventListFloat();
        public MechanicEventListString listEventString = new MechanicEventListString();
        public MechanicEventListObject listEventObject = new MechanicEventListObject();
        public MechanicEventListVector2 listEventVector2 = new MechanicEventListVector2();
        public MechanicEventListVector3 listEventVector3 = new MechanicEventListVector3();
        public MechanicEventListVector4 listEventVector4 = new MechanicEventListVector4();
        public MechanicEventListColor listEventColor = new MechanicEventListColor();
        public MechanicEventListGameObject listEventGameObject = new MechanicEventListGameObject();
        public MechanicEventListComponent listEventComponent = new MechanicEventListComponent();
        public MechanicEventListTransform listEventTransform = new MechanicEventListTransform();
        public MechanicEventListAudioClip listEventAudioClip = new MechanicEventListAudioClip();
        public MechanicEventListMaterial listEventMaterial = new MechanicEventListMaterial();
        public MechanicEventListTextAsset listEventTextAsset = new MechanicEventListTextAsset();
        public MechanicEventListException listEventException = new MechanicEventListException();
        public MechanicEventListIMechanicSystem listEventIMechanicSystem = new MechanicEventListIMechanicSystem();
        public MechanicEventListIMechanicSystemEvent listEventIMechanicSystemEvent = new MechanicEventListIMechanicSystemEvent();
        public MechanicEventListIMechanicSystemResult listEventIMechanicSystemResult = new MechanicEventListIMechanicSystemResult();
		#endregion List Properties Generation Area
		protected override List<IMechanicEventListBase> allLists => new List<IMechanicEventListBase>() {
			#region List Name Generation Area
            listEventBool,
            listEventInt,
            listEventFloat,
            listEventString,
            listEventObject,
            listEventVector2,
            listEventVector3,
            listEventVector4,
            listEventColor,
            listEventGameObject,
            listEventComponent,
            listEventTransform,
            listEventAudioClip,
            listEventMaterial,
            listEventTextAsset,
            listEventException,
            listEventIMechanicSystem,
            listEventIMechanicSystemEvent,
            listEventIMechanicSystemResult,
			#endregion List Name Generation Area
		};
	}

#if UNITY_EDITOR
	namespace Gen
	{
		public class MechanicEventsFileMarker
		{
			public static readonly string[] typeList = new string[] {
				"bool", "int", "float", "string", "object", nameof(Vector2), nameof(Vector3), nameof(Vector4), nameof(Color),
				nameof(GameObject), nameof(Component), nameof(Transform), nameof(AudioClip), nameof(Material),
                nameof(TextAsset), nameof(Exception),
				nameof(IMechanicSystem), nameof(IMechanicSystemEvent), nameof(IMechanicSystemResult)
			};

			public const string dataTypeTemplate = "{#DataType}";
			public const string dataNameTemplate = "{#DataName}";
			public const string eventClassNameTemplate = "MechanicEvent"+ dataNameTemplate;
			public const string eventTemplate = 
				"    [Serializable] public class " + eventClassNameTemplate+ " " +
				": " + nameof(SerializableEventT) + "<" + dataTypeTemplate + ">" +
				" {" +
				"[Serializable] public class TypedUnityEvent : UnityEvent<" + dataTypeTemplate + "> { } " +
				"public TypedUnityEvent eventHandle = new TypedUnityEvent(); " +
                "public override void Invoke(" + dataTypeTemplate + " data){ eventHandle.Invoke(data);}" +
				"}";


			public const string eventListClassNameTemplate = "MechanicEventList" + dataNameTemplate;
			public const string eventListTemplate = 
				"    [Serializable] public class " + eventListClassNameTemplate + 
				" : MechanicEventListBase<" + dataTypeTemplate + ", " + eventClassNameTemplate + "> { }";

			public const string eventListPropertyNameTemplate = "listEvent" + dataNameTemplate;
			public const string eventListPropertyTemplate =
				"        [InlineProperty, HideLabel]  public " + eventListClassNameTemplate + " " + eventListPropertyNameTemplate + " = new " + eventListClassNameTemplate + "();";

			public const string genListStartMark = "#region List Properties Generation Area";
			public const string genListEndMark = "#endregion List Properties Generation Area";

			public const string listNameTemplate = "            " + eventListPropertyNameTemplate + ",";
			public const string genListNamesStartMark = "#region List Name Generation Area";
			public const string genListNamesEndMark = "#endregion List Name Generation Area";

			public const string genAreaStartMark = "#region Generation Area";
			public const string genAreaEndMark = "#endregion Generation Area";
			public static string GetFilePath()
			{
				return GetFile();
			}

			private static string GetFile([CallerFilePath] string filePath = "")
			{
				return filePath;
			}
		}
	}
#endif
}


