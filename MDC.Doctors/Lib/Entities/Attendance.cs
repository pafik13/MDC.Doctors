using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
 	// Attendancies
	public class Attendance: RealmObject, IEntityFromClient, ISync
	{
		/// <summary>
		/// Уникальный идентификатор посещения. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Doctor { get; set; }

		public string WorkPlace { get; set; }

		public DateTimeOffset When { get; set; }

		public double Duration { get; set; }

		public bool IsFinished { get; set; }

		#region ISync
		public string DataSource { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

