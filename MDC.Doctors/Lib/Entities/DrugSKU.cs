using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class DrugSKU : RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		[Indexed]
		public string brand { get; set; }
	}
}

