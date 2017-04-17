using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class CategoryRule : RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		/// <summary>
		/// (UUID) Gets or sets the brand.
		/// </summary>
		/// <value>The brand.</value>
		public string brand { get; set; }

		public int potentialStart { get; set; }
		
		public int potentialEnd { get; set; }

		public float proportionStart { get; set; }
		
		public float proportionEnd { get; set; }

		/// <summary>
		/// (UUID) Gets or sets the category.
		/// </summary>
		/// <value>The category.</value>
		public string category { get; set; }
	}
}