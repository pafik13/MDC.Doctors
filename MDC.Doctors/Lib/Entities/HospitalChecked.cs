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

		public string GetUUID()
		{
			//throw new NotImplementedException();
			return uuid;
		}
		
		public string GetName()
		{
			//throw new NotImplementedException();
			return name;
		}

		public string GetAddress()
		{
			//throw new NotImplementedException();
			return address;
		}

		public string GetArea()
		{
			//throw new NotImplementedException();
			return area;
		}

		public string GetPhone()
		{
			//throw new NotImplementedException();
			return phone;
		}
	}
}

