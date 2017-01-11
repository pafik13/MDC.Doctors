using System;

using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// ЛПУ - Лечебно-профилактические учреждение.
	/// </summary>
	public class HospitalInputed: RealmObject, IEntityFromClient, IHospital, ISync
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

		public DateTimeOffset CreatedAt { get; set; }

		public DateTimeOffset UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public bool IsSynced { get; set; }

		public string GetName()
		{
			//throw new NotImplementedException();
			return Key;
		}

		public string GetAddress()
		{
			//throw new NotImplementedException();
			return Address;
		}

		public string GetArea()
		{
			//throw new NotImplementedException();
			return Area;
		}

		public string GetPhone()
		{
			//throw new NotImplementedException();
			return Phone;
		}
	}
}

