using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class PotentialData: RealmObject, IEntityFromClient, IAttendanceData, ISync
	{
		/// <summary>
		/// Уникальный идентификатор информации о потенциале врача. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		[Indexed]
		public string Attendance { get; set; }

		public string Brand { get; set; }

		public string Potential { get; set; }

		public float PrescriptionOfOur { get; set; }
		
		public float PrescriptionOfOther { get; set; }

		public float Portion { get; set; }

		public string Category { get; set; }

		#region ISync
		public string DataType { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

