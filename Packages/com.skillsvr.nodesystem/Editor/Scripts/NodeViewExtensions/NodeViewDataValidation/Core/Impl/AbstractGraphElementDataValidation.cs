using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    public interface IVisualElementPathBinding
    {
        VisualElement GetVisualSourceFromPath(string path);
    }

    public abstract class AbstractGraphElementDataValidation<T> : IDataValidator, IVisualElementPathBinding where T : VisualElement
	{
        public abstract void OnValidate();
        public abstract VisualElement OnGetVisualSourceFromPath(string path);
        public abstract string GetTargetId();

        public virtual T TargetVisual { get; set; }

        public void SetValidateSourceObject(object dataSource)
        {
            TargetVisual = dataSource as T;
        }

        public List<IValidationResult> results { get; private set; } = new List<IValidationResult>();

        public IEnumerable<IValidationResult> Validate()
        {
            results.Clear();
            try
            {
                OnValidate();
            }
            catch (NotImplementedException e)
            {
                AddResult(e.GetType().Name, "Data validation not implemented for this node.", WarningLevelEnum.Warning);
            }
            catch (Exception e)
            {
                AddResult(e.GetType().Name, e.Message + "\r\n" + e.StackTrace, WarningLevelEnum.Error);
            }
            return results;
        }


        public VisualElement GetVisualSourceFromPath(string path)
        {
            try
            {
                return OnGetVisualSourceFromPath(path);
            }
            catch (NotImplementedException e)
            {
                AddResult(e.GetType().Name, "Data validation not implemented for this node.", WarningLevelEnum.Warning);
            }
            catch (Exception e)
            {
                AddResult(e.GetType().Name, e.Message + "\r\n" + e.StackTrace, WarningLevelEnum.Error);
            }
            return null;
        }

        public bool ErrorIf(bool condition, string path, string message)
        {
            if (condition)
            {
                AddResult(path, message, WarningLevelEnum.Error);
            }
            return condition;
        }

        public bool ErrorIf(bool condition, IEnumerable<string> pathCollection, string message)
        {
            if (condition)
            {
                foreach (var path in pathCollection)
                {
                    AddResult(path, message, WarningLevelEnum.Error);
                }
            }
            return condition;
        }

        public bool WarningIf(bool condition, string path, string message)
        {
            if (condition)
            {
                AddResult(path, message, WarningLevelEnum.Warning);
            }
            return condition;
        }

        public bool WarningIf(bool condition, IEnumerable<string> pathCollection, string message)
        {
            if (condition)
            {
                foreach (var path in pathCollection)
                {
                    AddResult(path, message, WarningLevelEnum.Warning);
                }
            }
            return condition;
        }

        public void AddResult(string path, string msg, WarningLevelEnum level)
        {
            results.Add(new ValidationResult()
            {
                Id = GetTargetId(),
                WarningLevel = level,
                Name = path,
                Message = msg
            });
        }

        public bool IsNull(object isNullobj)
        {
            return null == isNullobj;
        }



        public bool InvalidPath(string path)
        {
            return string.IsNullOrWhiteSpace(path);
        }
        public bool AssetNotExist(string assetPath)
        {
            return !string.IsNullOrWhiteSpace(assetPath) && !IsAssetExist(assetPath);
        }

        public bool IsInvalidAsset(UnityEngine.Object asset)
        {
            return null == asset || !IsAssetExist(AssetDatabase.GetAssetPath(asset));
        }

        public bool IsMissingAsset(UnityEngine.Object asset)
        {
            return null != asset && !IsAssetExist(AssetDatabase.GetAssetPath(asset));
        }

        protected virtual bool IsAssetExist(string assetPath)
        {
            string fullPath = Application.dataPath.Replace("Assets", "") + assetPath;
            return File.Exists(fullPath);
        }

        protected virtual bool IsInvalidName(string name)
        {
            return string.IsNullOrWhiteSpace(name) || "none" == name.ToLower() || "null" == name.ToLower();
        }

        protected virtual void CheckPropGuid<TPropType>(PropGUID<TPropType> guid, string name, string path) where TPropType : class, IBaseProp
        {
			if (ErrorIf(guid.IsNullOrEmpty(), path, name + " cannot be none. \r\nSelect or create a new one."))
            {
                return;
            }

            ErrorIf(!guid.HasProp(), path, name + " not found.\r\nSelect or create a new one.\r\nGuid: " + guid.propGUID.ToString());
        }

        [Obsolete("Scene object is obsoleted, use prop system and CheckPropGuid() instead.")]
		protected virtual T Check1V1NamedSceneObjectBinding<T>(string nameValue, System.Predicate<T> predicate, string path, string invalidNameMessage) where T : UnityEngine.Object
        {
            if (IsInvalidName(nameValue))
            {
                ErrorIf(true, path, invalidNameMessage);
                return null;
            }

            var items = FindObjectsInScene<T>(predicate);
            ErrorIf(0 == items.Count(), path, "Scene object not found.\r\nType: " + typeof(T).Name + "\r\nName: " + nameValue);
            ErrorIf(1 < items.Count(), path, "Multiple scene objects found. Should be only 1 in scene. Current:" + items.Count() + " \r\nType: " + typeof(T).Name + "\r\nName: " + nameValue);
            return items.FirstOrDefault();
        }

		[Obsolete("Scene object is obsoleted, use prop system and CheckPropGuid() instead.")]
		protected virtual T FindObjectInScene<T>(System.Predicate<T> predicate = null, bool includeInactive = true) where T : UnityEngine.Object
        {
            if (null != predicate)
            {
                return GameObject.FindObjectsOfType<T>(includeInactive).Where(x => predicate.Invoke(x)).FirstOrDefault();
            }
            else
            {
                return GameObject.FindObjectsOfType<T>(includeInactive).FirstOrDefault();
            }
        }

		[Obsolete("Scene object is obsoleted, use prop system and CheckPropGuid() instead.")]
		protected virtual IEnumerable<T> FindObjectsInScene<T>(System.Predicate<T> predicate = null, bool includeInactive = true) where T : UnityEngine.Object
        {
            if (null != predicate)
            {
                return GameObject.FindObjectsOfType<T>(includeInactive).Where(x => predicate.Invoke(x));
            }
            else
            {
                return GameObject.FindObjectsOfType<T>(includeInactive);
            }
        }


        

        protected virtual string GetAssetNotExistMsg(string assetPath, string operationInfo = "")
        {
            return GetNotExistMsg("Asset", assetPath, operationInfo);
        }
        protected virtual string GetSceneNotExistMsg(string assetPath, string operationInfo = "Get or create a new scene.")
        {
            return GetNotExistMsg("Scene", assetPath, operationInfo);
        }
        protected virtual string GetNotExistMsg(string assetTitle, string assetPath, string operationInfo = "")
        {
            return string.Format("{0} not found. {1}\r\nMissing {0}: {2}", assetTitle, operationInfo, assetPath);
        }

        
    }
}