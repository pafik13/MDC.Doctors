using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class RouteItem : RealmObject, IEntityFromClient, ISync
	{
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string WorkPlace { get; set; }
		
		//[Indexed]
		//public string Doctor { get; set; }

		//[Indexed]
		//public string Hospital { get; set; }
		
		public DateTimeOffset Date { get; set; }

		public int Order { get; set; }

		public bool IsDone { get; set; }

		public string Comment { get; set; }

		#region ISync
		public string DataSource { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

