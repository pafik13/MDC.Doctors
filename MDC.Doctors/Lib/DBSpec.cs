using System;
using System.Linq;
using System.Collections.Generic;

using Realms;

using MDC.Doctors.Lib.Interfaces;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib;

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
		public struct DoctorInfoHolder
		{
			Doctor Doctor;
			WorkPlace MainWorkPlace;
			IHospital Hospital;

			public override int GetHashCode()
			{
				return Doctor.UUID.GetHashCode();
			}
		}

		public static Dictionary<string, List<DoctorHolder>> GetDoctorsForRoute(Realm db, DateTimeOffset selectedDate)
		{
			var date = selectedDate.UtcDateTime.Date;
			var result = new Dictionary<string, List<DoctorHolder>>();
			var emptyGUID = Guid.Empty.ToString();
			foreach (var doctor in db.All<Doctor>())
			{
				DoctorHolder holder;

				//if (string.IsNullOrEmpty(doctor.MainWorkPlace))
				//{
				//	holder = new DoctorHolder
				//	{
				//		UUID = string.Copy(doctor.UUID),
				//		Name = doctor.Name == null ? string.Empty : string.Copy(doctor.Name),
				//		MainWorkPlace = doctor.MainWorkPlace == null ? string.Empty : string.Copy(doctor.MainWorkPlace),
				//		HospitalName = string.Empty,
				//		HospitalAddress = string.Empty
				//	};
				//}
				//else
				//{
				//	var workPlace = DBHelper.Get<WorkPlace>(DB, doctor.MainWorkPlace);
				//	var hospital = DBHelper.GetHospital(DB, workPlace.Hospital);
				//	holder = new DoctorHolder
				//	{
				//		UUID = string.Copy(doctor.UUID),
				//		Name = doctor.Name == null ? string.Empty : string.Copy(doctor.Name),
				//		MainWorkPlace = doctor.MainWorkPlace == null ? string.Empty : string.Copy(doctor.MainWorkPlace),
				//		HospitalName = hospital.GetName() == null ? string.Empty : string.Copy(hospital.GetName()),
				//		HospitalAddress = hospital.GetAddress() == null ? string.Empty : string.Copy(hospital.GetAddress()),
				//	};
				//}

				string hospitalUUID = string.Empty;

				if (string.IsNullOrEmpty(doctor.MainWorkPlace))
				{
					hospitalUUID = emptyGUID;
					holder = new DoctorHolder
					{
						UUID = string.Copy(doctor.UUID),
						Name = doctor.Name == null ? string.Empty : string.Copy(doctor.Name),
						MainWorkPlace = string.Empty,
						HospitalName = string.Empty,
						HospitalAddress = string.Empty
					};
				}
				else
				{
					var workPlace = DBHelper.Get<WorkPlace>(db, doctor.MainWorkPlace);
					var hospital = DBHelper.GetHospital(db, workPlace.Hospital);
					hospitalUUID = hospital.GetUUID();
					holder = new DoctorHolder
					{
						UUID = string.Copy(doctor.UUID),
						Name = doctor.Name == null ? string.Empty : string.Copy(doctor.Name),
						MainWorkPlace = string.Copy(doctor.MainWorkPlace),
						workPlace.
						HospitalName = hospital.GetName() == null ? string.Empty : string.Copy(hospital.GetName()),
						HospitalAddress = hospital.GetAddress() == null ? string.Empty : string.Copy(hospital.GetAddress()),
					};
				}
				if (!result.ContainsKey(hospitalUUID))
				{
					result.Add(hospitalUUID, new List<DoctorHolder>());
				}

				result[hospitalUUID].Add(holder);
			}

			return result;
		}


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

