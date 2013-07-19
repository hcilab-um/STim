using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Collections;

namespace STim
{
	public class WagJointCollection : IEnumerable<Joint>, IEnumerable
	{

		private Dictionary<JointType, Joint> internalCollection = new Dictionary<JointType,Joint>();

		public Joint this[JointType jointType]
		{
			get { return internalCollection[jointType]; }
			set { internalCollection[jointType] = value; }
		}

		public WagJointCollection(JointCollection jointCollection)
		{
			foreach (JointType type in Enum.GetValues(typeof(JointType)))
			{
				internalCollection.Add(type, jointCollection[type]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalCollection.Values.GetEnumerator();
		}

		IEnumerator<Joint> IEnumerable<Joint>.GetEnumerator()
		{
			return internalCollection.Values.Cast<Joint>().GetEnumerator();
		}
	}
}
