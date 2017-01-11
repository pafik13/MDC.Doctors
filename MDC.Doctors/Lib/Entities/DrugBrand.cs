using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class DrugBrand : RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }
	}
}

