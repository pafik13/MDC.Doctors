using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	public class Material: RealmObject, IEntityFromServer
	{
		[PrimaryKey]
		public string uuid { get; set; }

		public string name { get; set; }

		public string fullPath { get; set; }

		public string s3ETag { get; set; }

		public string s3Location { get; set; }

		public string s3Key { get; set; }

		public string s3Bucket { get; set; }
	}
}

