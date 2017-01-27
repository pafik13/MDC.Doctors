using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class HospitalChecked: RealmObject, IEntityFromServer, IHospital
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		public string key { get; set; }

		public string address { get; set; }

		public string area { get; set; }

		public string phone { get; set; }

		public float? latitude { get; set;}

		public float? longitude { get; set;}
		
		#region IHospital
		public string GetUUID() { return uuid; }
		
		public string GetName() { return name; }

		public string GetAddress() { return address; }

		public string GetArea() { return area; }

		public string GetPhone() { return phone; }
		#endregion
	}
}

