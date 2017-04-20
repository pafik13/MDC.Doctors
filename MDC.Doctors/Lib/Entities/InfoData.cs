using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class InfoData : RealmObject, IEntityFromClient, IAttendanceData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор презентации. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		public string Brand { get; set; }

		public string WorkTypes { get; set; }

		public string Callback { get; set; }

		public string Resume { get; set; }

		public string Goal { get; set; }

		#region ISync
		public string DataSource { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

