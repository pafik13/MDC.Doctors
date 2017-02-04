using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using Realms;

using MDC.Doctors.Lib.Entities;
using System.Globalization;

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
	}

	// TODO: ForDisplay -> from List to Dictionary, because need change visibility
	public class DoctorsForRouteAdapter : BaseAdapter<DoctorHolder>
	{
		readonly Activity Context;
		readonly public DoctorHolder[] Doctors;
		List<DoctorHolder> ForDisplay;
		Dictionary<string, bool> DoneItems;
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

			if (DoneItems != null && !string.IsNullOrEmpty(item.MainWorkPlace) && DoneItems[item.MainWorkPlace]) {
				return new View(Context);
			}

			var isValidView = (convertView is LinearLayout);
			View view;
			if (isValidView)
			{
				view = convertView as LinearLayout;
			}
			else {
				view = Context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorTableItem, parent, false);
			}

			view.FindViewById<TextView>(Resource.Id.rdtiDoctorNameTV).Text = item.Name;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalNameTV).Text = item.HospitalName;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalAddressTV).Text = item.HospitalAddress;

			//Finally return the view
			return view;
		}

		public void SwitchVisibility(int position)
		{
			var item = Doctors[position];

			item.IsVisible = !item.IsVisible;

			NotifyDataSetChanged();
		}

		public void ChangeVisibility(int position, bool isVisible)
		{
			var item = Doctors[position];

			item.IsVisible = isVisible;

			NotifyDataSetChanged();
		}

		public void Search(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				ForDisplay = null;
				return;
			}

			if (item.IsVisible)
			{
				//item.Subway = null;
				if (culture.CompareInfo.IndexOf(item.Subway, text, CompareOptions.IgnoreCase) >= 0)
				{
					item.Match = string.Format(matchFormat, @"метро=" + item.Subway);
					SearchedItems.Add(item);
					//if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Region, text, CompareOptions.IgnoreCase) >= 0)
				{
					item.Match = string.Format(matchFormat, @"район=" + item.Region);
					SearchedItems.Add(item);
					//if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Brand, text, CompareOptions.IgnoreCase) >= 0)
				{
					item.Match = string.Format(matchFormat, @"бренд=" + item.Brand);
					SearchedItems.Add(item);
					//if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}

				if (culture.CompareInfo.IndexOf(item.Address, text, CompareOptions.IgnoreCase) >= 0)
				{
					item.Match = string.Format(matchFormat, @"адрес");
					SearchedItems.Add(item);
					//if (SearchedItems.Count > C_ITEMS_IN_RESULT) break;
					continue;
				}
			}

			var list = new List<DoctorHolder>();
			for (int i = 0; i < Doctors.Length; i++)
			{
				var item = Doctors[i];
				if (item.Name.Contains(text))
				{
					list.Add(item);
				}
				else if (item.HospitalAddress.Contains(text))
				{
					list.Add(item);
				}
				else if (item.HospitalName.Contains(text))
				{
					list.Add(item);
				}
			}

			ForDisplay = list;
			NotifyDataSetChanged();
		}

		void SetDoneItems(List<RouteItem> doneItems)
		{
			DoneItems = new Dictionary<string, bool>(doneItems.Count);
			for (int i = 0; i < doneItems.Count; i++)
			{
				DoneItems.Add(doneItems[i].WorkPlace, true);
			}
		}
	}
}

