using System;
using Props.PropInterfaces;

namespace Props
{
	[Serializable]
	public class PropGUID<TPropType> where TPropType : class, IBaseProp
	{
		public PropGUID(bool isNull = false)
		{
			propGUID = isNull ? null : Guid.NewGuid().ToString();
		}
		
		public PropGUID(string guid)
		{
			propGUID = guid;
		}
		
		public PropGUID(PropGUID<IBaseProp> guid)
		{
			propGUID = guid.propGUID;
		}
		
		public PropGUID<T> Convert<T>() where T : class, IBaseProp
		{
			return new PropGUID<T>(propGUID);
		}
		
		public string propGUID;
		
		public TPropType GetProp()
		{
			return PropManager.GetProp(this);
		}
		
		public string GetPropName()
		{
			return PropManager.GetPropName(this);
		}

		
		
		protected bool Equals(PropGUID<TPropType> other)
		{
			return propGUID == other.propGUID;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.GetType() == GetType() && Equals((PropGUID<TPropType>)obj);
		}

		public override int GetHashCode()
		{
			return propGUID != null ? propGUID.GetHashCode() : 0;
		}
        
		public static implicit operator string(PropGUID<TPropType> a)
		{
			if (null == a)
			{
				return string.Empty;
			}
			return a.propGUID;
		}
		
		public static implicit operator PropGUID<TPropType>(string a)
		{
			return new PropGUID<TPropType>(a);
		}
        
	}

	public static class PropGUIDExtensions
	{
		public static bool IsNull<TPropType>(this PropGUID<TPropType> guid) where TPropType : class, IBaseProp
		{
			if (null == guid)
			{
				return true;
			}
			if (null == guid.propGUID)
			{
				return true;
			}

			return false;
		}

		public static bool IsNullOrEmpty<TPropType>(this PropGUID<TPropType> guid) where TPropType : class, IBaseProp
		{
			if (IsNull(guid))
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(guid.propGUID))
			{
				return true;
			}

			string leftoverGuid = guid.propGUID.Replace("0", "").Replace("-", "");

			if (string.IsNullOrWhiteSpace(leftoverGuid))
			{
				return true;
			}

			return false;
		}

		public static bool HasProp<TPropType>(this PropGUID<TPropType> guid) where TPropType : class, IBaseProp
		{
			if (guid.IsNullOrEmpty())
			{
				return false;
			}

			return null != guid.GetProp();
		}
	}
}