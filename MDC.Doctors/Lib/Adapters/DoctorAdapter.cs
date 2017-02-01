using System;
using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Adapters
{
	public class DoctorAdapter : BaseAdapter<Doctor>
	{
		readonly Activity Context;
		readonly IList<Doctor> Doctors;
		readonly string[] DoctorsInRoute;

		public DoctorAdapter(Activity context, IList<Doctor> doctors, string[] doctorsInRoute = null) : base()
		{
			Context = context;
			Doctors = doctors;
			DoctorsInRoute = doctorsInRoute;
		}

		public override Doctor this[int position]
		{
			get { 
				return Doctors[position]; 
			}
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override int Count
		{
			get { 
				return Doctors.Count; 
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for position
			var item = Doctors[position];

			
            var view = (convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.DoctorTableItem, parent, false)
			           ) as LinearLayout;
			view.FindViewById<TextView>(Resource.Id.dtiNoTV).Text = (position + 1).ToString();
            view.FindViewById<TextView>(Resource.Id.dtiStateTV).Text =
				    string.IsNullOrEmpty(item.State) ? @"<нет статуса>" : item.GetStateDesc();
			view.FindViewById<TextView>(Resource.Id.dtiNameTV).Text = 
                string.IsNullOrEmpty(item.Name) ? @"<нет ФИО>" : item.Name;
            view.FindViewById<TextView>(Resource.Id.dtiSpecialityTV).Text = 
                string.IsNullOrEmpty(item.Specialty) ? @"<нет специальности>" : item.Specialty;

			var nextAttendance = view.FindViewById<Button>(Resource.Id.dtiNextAttendanceB);
			nextAttendance.SetTag(Resource.String.DoctorUUID, item.UUID);
			nextAttendance.Text = item.NextAttendanceDate.HasValue ? 
				item.NextAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"Начать визит"; //DateTimeOffset.Now.ToString(@"dd.MM.yyyy");
			nextAttendance.Click -= NextAttendanceClickEventHandler;
			nextAttendance.Click += NextAttendanceClickEventHandler;

			var lastAttendance = view.FindViewById<TextView>(Resource.Id.dtiLastAttendanceDateTV);
			lastAttendance.Text = item.LastAttendanceDate.HasValue ? 
				item.LastAttendanceDate.Value.ToString(@"dd.MM.yyyy") : @"<нет визита>";

			if (!string.IsNullOrEmpty(item.MainWorkPlace))
			{
				var DB = Realms.Realm.GetInstance();
				var workPlace = DBHelper.Get<WorkPlace>(DB, item.MainWorkPlace);
				var hospital = DBHelper.GetHospital(DB, workPlace.Hospital);
				
				view.FindViewById<TextView>(Resource.Id.dtiHospitalAreaTV).Text = 
					string.IsNullOrEmpty(hospital.GetArea()) ? @"<нет округа>" : hospital.GetArea();
				view.FindViewById<TextView>(Resource.Id.dtiHospitalNameTV).Text = 
					string.IsNullOrEmpty(hospital.GetName()) ? @"<нет названия>" : hospital.GetName();
				view.FindViewById<TextView>(Resource.Id.dtiHospitalAddressTV).Text = 
					string.IsNullOrEmpty(hospital.GetAddress()) ? @"<нет адреса>" : hospital.GetAddress();
					
				view.FindViewById<TextView>(Resource.Id.dtiWorkPlaceCabinetTV).Text = 
					string.IsNullOrEmpty(workPlace.Cabinet) ? @"<нет кабинета>" : workPlace.Cabinet;
				view.FindViewById<TextView>(Resource.Id.dtiWorkPlaceTimetableTV).Text = 
					string.IsNullOrEmpty(workPlace.Timetable) ? @"<нет расписания>" : workPlace.Timetable;
			}
				
			if (DoctorsInRoute == null) return view;
			if (DoctorsInRoute.Length == 0) return view;

			//// wmOnlyRoute, wmRouteAndRecommendations, wmOnlyRecommendations
			//switch (Helper.WorkMode) {
			//	case WorkMode.wmOnlyRoute:
			//	case WorkMode.wmRouteAndRecommendations:
			//		if (PharmaciesInRoute.Contains(item.UUID)) {
			//			view.SetBackgroundResource(Resource.Color.Light_Green_100);
			//			if (item.LastAttendanceDate.HasValue) {
			//				if (item.LastAttendanceDate.Value.UtcDateTime.Date == DateTimeOffset.UtcNow.Date) {
			//					nextAttendance.Text = @"Пройдено!";
			//				} else {
			//					nextAttendance.Text = @"Начать визит";
			//				}
			//			} else {
			//				nextAttendance.Text = @"Начать визит";
			//			}
			//		} else {
			//			switch (item.GetState()) {
			//				case PharmacyState.psActive:
			//					view.SetBackgroundColor(Android.Graphics.Color.White);
			//					break;
			//				case PharmacyState.psReserve:
			//					view.SetBackgroundResource(Resource.Color.Yellow_100);
			//					break;
			//				case PharmacyState.psClose:
			//					view.SetBackgroundResource(Resource.Color.Red_100);
			//					break;
			//			}
			//		}
			//		break;
			//	case WorkMode.wmOnlyRecommendations:
			//		switch (item.GetState()) {
			//			case PharmacyState.psActive:
			//				view.SetBackgroundColor(Android.Graphics.Color.White);
			//				break;
			//			case PharmacyState.psReserve:
			//				view.SetBackgroundResource(Resource.Color.Yellow_100);
			//				break;
			//			case PharmacyState.psClose:
			//				view.SetBackgroundResource(Resource.Color.Red_100);
			//				break;
			//		}
			//		break;
			//}

			//Finally return the view
			return view;
		}

		void NextAttendanceClickEventHandler(object sender, EventArgs e)
		{
			if (sender is Button)
			{
                var doctorUUID = ((Button)sender).GetTag(Resource.String.DoctorUUID).ToString();
                var attendanceAcivity = new Intent(Context, typeof(AttendanceActivity));
				attendanceAcivity.PutExtra(Consts.C_DOCTOR_UUID, doctorUUID);
                Context.StartActivity(attendanceAcivity);
                //Toast.MakeText(Context, "Button clicked", ToastLength.Short).Show();
			}
		}
	}
}
