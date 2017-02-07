using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using Realms;

using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Adapters
{
	public struct DoctorHolder
	{
		public string UUID;
		public string Name;
		public string MainWorkPlace;
		public string HospitalName;
		public string HospitalAddress;
		public bool IsVisible;
		
		public override int GetHashCode()
		{
			return UUID.GetHashCode();
		}

		//public override bool Equals(object other)
		//{
			
		//	return UUID.Equals();
		//}
	}

	// TODO: ForDisplay -> from List to Dictionary, because need change visibility
	public class DoctorsForRouteAdapter : BaseAdapter<DoctorHolder>
	{
		readonly Activity Context;
		readonly public DoctorHolder[] Doctors;
		List<DoctorHolder> ForDisplay;

		string SearchText;

		DateTimeOffset? SelectedDate;

		Dictionary<string, bool> DoneItems;
		Dictionary<string, bool> CurrItems;
		readonly CultureInfo Culture;

		public DoctorsForRouteAdapter(Activity context)
		{
			Culture = CultureInfo.GetCultureInfo("ru-RU");

			Context = context;

			var DB = Realm.GetInstance();
			var doctors = DBHelper.GetAll<Doctor>(DB);
			Doctors = new DoctorHolder[doctors.Count()];

			int i = 0;

			foreach (var doctor in doctors)
			{
				DoctorHolder holder;

				if (string.IsNullOrEmpty(doctor.MainWorkPlace))
				{
					holder = new DoctorHolder
					{
						UUID = string.Copy(doctor.UUID),
						Name = string.Copy(doctor.Name),
						MainWorkPlace = string.Copy(doctor.MainWorkPlace),
						HospitalName = string.Empty,
						HospitalAddress = string.Empty
					};
				}
				else
				{
					var workPlace = DBHelper.Get<WorkPlace>(DB, doctor.MainWorkPlace);
					var hospital = DBHelper.GetHospital(DB, workPlace.Hospital);
					holder = new DoctorHolder
					{
						UUID = string.Copy(doctor.UUID),
						Name = string.Copy(doctor.Name),
						MainWorkPlace = string.Copy(doctor.MainWorkPlace),
						HospitalName = string.Copy(hospital.GetName()),
						HospitalAddress = string.Copy(hospital.GetAddress()),
					};
				}

				Doctors[i] = holder;

				i++;
			}

			ForDisplay = null;
			DoneItems = null;
			CurrItems = new Dictionary<string, bool>();
		}

		public override DoctorHolder this[int position] {
			get
			{
				return ForDisplay == null ? Doctors[position] : ForDisplay[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get
			{
				return ForDisplay == null ? Doctors.Length : ForDisplay.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = ForDisplay == null ? Doctors[position] : ForDisplay[position];

			if (string.IsNullOrEmpty(item.MainWorkPlace) || CurrItems[item.MainWorkPlace] || (DoneItems != null && DoneItems[item.MainWorkPlace])) {
				return new View(Context);
			}

			var isValidView = (convertView is LinearLayout);
			View view;
			if (isValidView)
			{
				view = convertView as LinearLayout;
			}
			else 
			{
				view = Context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorTableItem, parent, false);
			}

			view.FindViewById<TextView>(Resource.Id.rdtiDoctorNameTV).Text = item.Name;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalNameTV).Text = item.HospitalName;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalAddressTV).Text = item.HospitalAddress;

			//Finally return the view
			return view;
		}
		
		public DoctorHolder Get(string workPlace)
		{
			return Doctors.FirstOrDefault(d => d.MainWorkPlace == workPlace);
		}
		
		public void ClearCurrentRoute()
		{
			CurrItems = new Dictionary<string, bool>();
			
			NotifyDataSetChanged();
		}
		
		public void AddCurrentRouteItem(RouteItem routeItem)
		{
			CurrItems.Add(routeItem.WorkPlace, true);

			NotifyDataSetChanged();
		}

		public void RemoveCurrentRouteItem(RouteItem routeItem)
		{
			CurrItems.Remove(routeItem.WorkPlace);

			NotifyDataSetChanged();
		}

		public void SetSearchText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				ForDisplay = null;
				SearchText = null;
				return;
			}

			var list = new List<DoctorHolder>();
			if (DoneItems == null)
			{
				for (int i = 0; i < Doctors.Length; i++)
				{
					var item = Doctors[i];

					if (Culture.CompareInfo.IndexOf(item.Name, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}

					if (Culture.CompareInfo.IndexOf(item.HospitalAddress, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}

					if (Culture.CompareInfo.IndexOf(item.HospitalName, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}
				}
			}
			else 
			{
				for (int i = 0; i < Doctors.Length; i++)
				{
					var item = Doctors[i];
					if (Culture.CompareInfo.IndexOf(item.Name, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}

					if (Culture.CompareInfo.IndexOf(item.HospitalAddress, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}

					if (Culture.CompareInfo.IndexOf(item.HospitalName, text, CompareOptions.IgnoreCase) >= 0)
					{
						list.Add(item);
						continue;
					}
				}
			}

			ForDisplay = list;
			NotifyDataSetChanged();
		}

		public void SetSelectedDate(DateTimeOffset selectedDate)
		{
			SelectedDate = selectedDate;

			if (Helper.WeeksInRoute < 2)
			{
				DoneItems = null;
			}

			var db = Realm.GetInstance();
			var doneItems = new Dictionary<string, bool>();

			var lowDate = selectedDate.AddDays(-7 * Helper.WeeksInRoute + 8).UtcDateTime.Date;
			var highDate = selectedDate.AddDays(-1).UtcDateTime.Date;
			foreach (var item in db.All<RouteItem>())
			{
				if (highDate >= item.Date.Date && item.Date.Date >= lowDate && item.IsDone)
				{
					DoneItems.Add(item.WorkPlace, true);
				}
			}

			if (doneItems.Count > 0)
			{
				DoneItems = doneItems;
			}
			else
			{
				DoneItems = null;
			}
		}
	}
}

