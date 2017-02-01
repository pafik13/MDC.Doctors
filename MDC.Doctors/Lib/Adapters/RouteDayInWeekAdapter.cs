using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Adapters
{
	public class RouteDayInWeekAdapter : BaseAdapter<RouteItem>
	{
		readonly Activity Context;
		readonly IList<RouteItem> RouteItems;
		readonly string DoctorNotFoundText;

		public RouteDayInWeekAdapter(Activity context, IList<RouteItem> routeItems)
		{
			Context = context;
			RouteItems = routeItems;
			DoctorNotFoundText = Context.Resources.GetString(Resource.String.doctor_not_found);
		}

		public override RouteItem this[int position] {
			get {
				return RouteItems[position];
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count {
			get {
				return RouteItems.Count;
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.RouteWeekTableItem, parent, false)
					   ) as LinearLayout;

			var item = RouteItems[position];

			var DB = Realms.Realm.GetInstance();
			var workPlace = DBHelper.Get<WorkPlace>(DB, item.WorkPlace);
			var doctor = DBHelper.Get<Doctor>(DB, workPlace.Doctor);
			var hospital = DBHelper.GetHospital(DB, workPlace.Hospital);

			if (workPlace == null) {
				view.FindViewById<TextView>(Resource.Id.rwtiDoctorNameTV).Text = DoctorNotFoundText;
				view.FindViewById<TextView>(Resource.Id.rwtiHospitalNameTV).Text = string.Empty;
				view.FindViewById<TextView>(Resource.Id.rwtiHospitalAddressTV).Text = string.Empty;
			} else {
				view.FindViewById<TextView>(Resource.Id.rwtiDoctorNameTV).Text = doctor.Name;
				view.FindViewById<TextView>(Resource.Id.rwtiHospitalNameTV).Text = hospital.GetName();
				view.FindViewById<TextView>(Resource.Id.rwtiHospitalAddressTV).Text = hospital.GetAddress();
			}
            return view;
		}
	}
}

