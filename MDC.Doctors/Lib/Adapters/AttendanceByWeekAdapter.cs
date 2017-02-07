using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using Android.App;
using Android.Views;
using Android.Widget;

using Realms;

using MDC.Doctors.Lib.Entities;
using System.Threading.Tasks;

namespace MDC.Doctors.Lib.Adapters
{
	public class AttendanceByWeekAdapter : BaseAdapter<Dictionary<int, int>>
	{
		readonly Realm DB;
		readonly Activity Context;
        readonly int[] WeekKeys;
		readonly Dictionary<string, Dictionary<int, int>> Items;
		
		readonly public DoctorHolder[] Doctors;
		List<DoctorHolder> ForDisplay;
		
		string Text;

		readonly CultureInfo Culture;


		public AttendanceByWeekAdapter(Activity context, Dictionary<string, Dictionary<int, int>> data, int[] weekKeys)
		{
			Culture = CultureInfo.GetCultureInfo("ru-RU");

			DB = Realm.GetInstance();

			Context = context;

			WeekKeys = weekKeys;

			Items = data;
			
			Doctors = new DoctorHolder[data.Keys.Count];
			int i = 0;
			foreach (var doctorUUID in data.Keys)
			{
				var doctor = DBHelper.Get<Doctor>(DB, doctorUUID);
				
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
			
		}

		public override Dictionary<int, int> this[int position] {
			get {
				if (ForDisplay == null) return Items[Items.Keys.ElementAt(position)];
				
				return Items[ForDisplay[position].UUID];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return ForDisplay == null ? Items.Keys.Count : ForDisplay.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var doctorUUID = ForDisplay == null ? Items.Keys.ElementAt(position) : ForDisplay[position].UUID;
			var item = Items[doctorUUID];
			var doctor = Doctors.First(d => d.UUID == doctorUUID);

            var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableItem, parent, false)
			           ) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.abwtiDoctorNameTV).Text = doctor.Name;
			view.FindViewById<TextView>(Resource.Id.abwtiHospitalNameTV).Text = doctor.HospitalName;
			view.FindViewById<TextView>(Resource.Id.abwtiHospitalAddressTV).Text = doctor.HospitalAddress;
			
			if (string.IsNullOrEmpty(doctor.MainWorkPlace)) {
				return view;
			}
			
			view.FindViewById<TextView>(Resource.Id.abwtiWeek1).Text = item[WeekKeys[0]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek2).Text = item[WeekKeys[1]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek3).Text = item[WeekKeys[2]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek4).Text = item[WeekKeys[3]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek5).Text = item[WeekKeys[4]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek6).Text = item[WeekKeys[5]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek7).Text = item[WeekKeys[6]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek8).Text = item[WeekKeys[7]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek9).Text = item[WeekKeys[8]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek10).Text = item[WeekKeys[9]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek11).Text = item[WeekKeys[10]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek12).Text = item[WeekKeys[11]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek13).Text = item[WeekKeys[12]].ToString();
            view.FindViewById<TextView>(Resource.Id.abwtiWeek14).Text = item[WeekKeys[13]].ToString();

            return view;
		}
		// http://ru.stackoverflow.com/questions/316990/%D0%9A%D0%B0%D0%BA-%D1%80%D0%B0%D0%B1%D0%BE%D1%82%D0%B0%D1%8E%D1%82-await-async
		public Task SetSearchTextAsync(string text)
		{
			return Task.Run(() => {
				if (string.IsNullOrEmpty(text)) {
					ForDisplay = null;
					Text = text;
					return;
				}
				
				var list = new List<DoctorHolder>();
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
				
				ForDisplay = list;
			});
		}
	}
}

