using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// Специальность врача/медработника (например: ... и т.д.).
	/// </summary>
	public class Specialty: RealmObject, IEntityFromServer
	{
		/// <summary>
		/// Уникальный идентификатор специальности. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string uuid { get; set; }

		/// <summary>
		/// Название специальности.
		/// </summary>
		/// <value>The name.</value>
		public string name { get; set; }
	}
}

