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

namespace MDC.Doctors.Lib.Adapters
{
	public class AttendanceByWeekAdapter : BaseAdapter<Dictionary<int, int>>
	{
		readonly Realm DB;
		readonly Activity Context;
        readonly int[] WeekKeys;
		readonly Dictionary<string, Dictionary<int, int>> Items;

		string Text;

		public AttendanceByWeekAdapter(Activity context, Dictionary<string, Dictionary<int, int>> data, int[] weekKeys)
		{
			DB = Realm.GetInstance();

			Context = context;

			WeekKeys = weekKeys;

			Items = data;
		}

		public override Dictionary<int, int> this[int position] {
			get {
				return Items[Items.Keys.ElementAt(position)];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return Items.Keys.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var key = Items.Keys.ElementAt(position);
			var item = Items[key];
			var workPlace = DBHelper.Get<WorkPlace>(DB, key);
			var doctor = DBHelper.Get<Doctor>(DB, workPlace.Doctor);
			var hospital = DBHelper.GetHospital(DB, workPlace.Hospital);

            var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableItem, parent, false)
			           ) as LinearLayout;

			view.FindViewById<TextView>(Resource.Id.rdtiDoctorNameTV).Text = item.Name;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalNameTV).Text = item.HospitalName;
			view.FindViewById<TextView>(Resource.Id.rdtiHospitalAddressTV).Text = item.HospitalAddress;

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

		public void SetSearchText(string text)
		{
			Text = text;
		}
	}
}

