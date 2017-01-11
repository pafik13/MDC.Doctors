using System;

using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// Место работы - связь врача/медработника с ЛПУ и дополнительная информация.
	/// </summary>
	public class WorkPlace: IEntityFromClient, ISync
	{
		/// <summary>
		/// Уникальный идентификатор. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		public string Doctor { get; set; }

		public string Hospital { get; set; }

		public bool IsMain { get; set; }

		public string Cabinet { get; set; }

		public string Timetable	{ get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public bool IsSynced { get; set; }
	}
}

