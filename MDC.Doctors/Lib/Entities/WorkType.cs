using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// Вид работы (например: листовка, презентация и т.д.).
	/// </summary>
	public class WorkType : RealmObject, IEntityFromServer
	{
		/// <summary>
		/// Уникальный идентификатор вида работы. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string uuid { get; set; }

		/// <summary>
		/// Название вида работы.
		/// </summary>
		/// <value>The name.</value>
		public string name { get; set; }
	}
}

