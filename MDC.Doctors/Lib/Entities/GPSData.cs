using System;
using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class GPSData : RealmObject, IAttendanceData, IEntityFromClient, ISync
	{
		/// <summary>
		/// Уникальный идентификатор информации о местоположении. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Attendance { get; set; }

		public float Accuracy { get; set; }

		public double Altitude { get; set; }

		public float Bearing { get; set; }

		public long ElapsedRealtimeNanos { get; set; }

		public bool IsFromMockProvider { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public string Provider { get; set; }

		public float Speed { get; set; }

		#region ISync
		public string DataSource { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion
	}
}

