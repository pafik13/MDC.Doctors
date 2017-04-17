using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class Category : RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		public int order { get; set; }
	}
}