using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;

namespace GroupObjectCleaner
{
	public class Map<TKey, TValue> : Dictionary<TKey, TValue>
	{
		public new TValue this[TKey key]
		{
			get
			{
				TValue value;
				if (this.TryGetValue(key, out value))
				{
					return value;
				}
				else
				{
					return default(TValue);
				}
			}
			set 
			{
				base[key] = value;
			}
		}
	}

	public static class ExProp
	{
		private static ConditionalWeakTable<IGH_DocumentObject, Map<string, Guid>> values { get; }
			= new ConditionalWeakTable<IGH_DocumentObject, Map<string, Guid>>();

		public static Guid getExProp(this IGH_DocumentObject self, string prop_name)
		{
			return values.GetOrCreateValue(self)[prop_name];
		}

		public static void setExProp(this IGH_DocumentObject self, string prop_name, Guid value)
		{
			if (self == null) return;
			values.GetOrCreateValue(self)[prop_name] = value;
		}

		public static void clearExProp(this IGH_DocumentObject self)
		{
			values.Remove(self);
		}
	}
}
