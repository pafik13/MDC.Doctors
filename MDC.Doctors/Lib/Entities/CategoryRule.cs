using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class CategoryRule : RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string desc { get; set; }

		/// <summary>
		/// (UUID) Gets or sets the brand.
		/// </summary>
		/// <value>The brand.</value>
		public string brand { get; set; }

		/// <summary>
		/// (UUID) Gets or sets the speciality.
		/// </summary>
		/// <value>The speciality.</value>
		public string speciality { get; set; }

		public int potential { get; set; }

		public float proportion { get; set; }
	}
}