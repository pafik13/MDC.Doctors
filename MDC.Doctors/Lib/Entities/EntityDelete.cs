using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class EntityDelete : RealmObject, IEntityFromClient, ISync
	{
		public string UUID { get; set; }

		public string Entity { get; set; }

		#region ISync
		public string DataType { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

