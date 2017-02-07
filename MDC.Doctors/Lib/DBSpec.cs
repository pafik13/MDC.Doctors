using System;
using System.Linq;
using System.Collections.Generic;

using Realms;

using MDC.Doctors.Lib.Interfaces;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors
{
	public static class DBSpec
	{
		public static int CountItemsToSync<T>(Realm db) where T : RealmObject, ISync
		{
			return db.All<T>().Count(item => !item.IsSynced);
		}

		public static int CountItemsToSyncAll(Realm db)
		{
			int result = 0;

			result += CountItemsToSync<Attendance>(db);
			result += CountItemsToSync<Doctor>(db);
			result += CountItemsToSync<EntityDelete>(db);
			result += CountItemsToSync<GPSData>(db);
			result += CountItemsToSync<HospitalInputed>(db);
			result += CountItemsToSync<InfoData>(db);
			result += CountItemsToSync<PotentialData>(db);
			result += CountItemsToSync<RouteItem>(db);
			result += CountItemsToSync<WorkPlace>(db);

			return result;
		}

		//public static Dictionary<string, Dictionary<int, int>> GetProfileReportData(Realm db, int[] weekKeys)
		//{
		//	var result = new Dictionary<string, Dictionary<int, int>>();
		//	foreach (var workPlace in db.All<WorkPlace>())
		//	{
		//		result.Add(workPlace.UUID, new Dictionary<int, int>());
		//		for (int i = 0; i < ProfileActivity.WeeksCount; i++)
		//		{
		//			result[workPlace.UUID].Add(weekKeys[i], 0);
		//		}
		//	}

		//	foreach (var attendance in db.All<Attendance>())
		//	{
		//		int key = Helper.GetWeekKey(attendance.When);
		//		if (result.ContainsKey(attendance.WorkPlace))
		//		{
		//			if (result[attendance.WorkPlace].ContainsKey(key))
		//			{
		//				result[attendance.WorkPlace][key]++;
		//			}
		//		}
		//	}

		//	return result;
		//}
		
		public static List<RouteItem> GetRouteItems(Realm db, DateTimeOffset selectedDate)
		{
			var date = selectedDate.UtcDateTime.Date;
			var result = new List<RouteItem>();
			foreach(var item in db.All<RouteItem>())
			{
				if (item.Date.Date == date)
				{
					result.Add(item);
				}
			}
			
			return result;
		}
		
		public static Dictionary<string, Dictionary<int, int>> GetProfileReportData(Realm db, int[] weekKeys)
		{			
			var result = new Dictionary<string, Dictionary<int, int>>();
			foreach (var doctor in db.All<Doctor>())
			{
				result.Add(doctor.UUID, new Dictionary<int, int>());
				for (int i = 0; i < ProfileActivity.WeeksCount; i++)
				{
					result[doctor.UUID].Add(weekKeys[i], 0);
				}
			}


			foreach (var attendance in db.All<Attendance>())
			{
				if (result.ContainsKey(attendance.Doctor))
				{
					int key = Helper.GetWeekKey(attendance.When);
					if (result[attendance.Doctor].ContainsKey(key))
					{
						result[attendance.Doctor][key]++;
					}
				}
			}

			return result;
		}
	}
}

