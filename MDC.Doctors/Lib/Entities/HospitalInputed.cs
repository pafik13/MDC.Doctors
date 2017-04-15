using System;

using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// ЛПУ - Лечебно-профилактические учреждение.
	/// </summary>
	public class HospitalInputed : RealmObject, IEntityFromClient, IHospital, ISync
	{
		/// <summary>
		/// Уникальный идентификатор ЛПУ. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		/// <summary>
		/// Название ЛПУ.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Ключ/краткое наименование ЛПУ.
		/// </summary>
		/// <value>The key.</value>
		public string Key { get; set; }

		/// <summary>
		/// Адрес ЛПУ.
		/// </summary>
		/// <value>The address.</value>
		public string Address { get; set; }

		/// <summary>
		/// Округ ЛПУ.
		/// </summary>
		/// <value>The area.</value>
		public string Area { get; set; }

		/// <summary>
		/// Краткая записть о телефонах.
		/// </summary>
		/// <value>The area.</value>
		public string Phone { get; set; }

		#region ISync
		public string DataSource { get; set; }

		public string CreatedBy { get; set; }

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public bool IsSynced { get; set; }
		#endregion

		#region IHospital
		public string GetUUID() { return UUID; }
		
		public string GetName() { return Key; }

		public string GetAddress() { return Address; }

		public string GetArea() { return Area; }

		public string GetPhone() { return Phone; }
		#endregion
	}
}

