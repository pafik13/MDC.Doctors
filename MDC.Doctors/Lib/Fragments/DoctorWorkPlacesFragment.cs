using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using V4App = Android.Support.V4.App;

using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;
using MDC.Doctors.Lib;
using System.Collections.Generic;

namespace MDC.Doctors
{
	public class DoctorWorkPlacesFragment : V4App.Fragment, ISave
	{
		Realm DB;
		Doctor Doctor;
		
		public static DoctorWorkPlacesFragment Create(string doctorUUID)
		{
			var fragment = new DoctorWorkPlacesFragment();
			var arguments = new Bundle();
			arguments.PutString(Consts.C_DOCTOR_UUID, doctorUUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			DB = Realm.GetInstance();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var mainView = inflater.Inflate(Resource.Layout.DoctorWorkPlacesFragment, container, false);

			var WPTable = mainView.FindViewById<LinearLayout>(Resource.Id.dwpfMainLL);
			var item = inflater.Inflate(Resource.Layout.WorkPlaceItem, WPTable, true);
			var hospital = item.FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
			var hospitals = DBHelper.GetList<HospitalInputed>(DB).ToArray();
			hospital.Adapter = new HospitalSuggestionAdapter(Activity, hospitals);
			return mainView;
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		public override void OnPause()
		{
			base.OnPause();
			// TIP: check section
			var errors = string.Empty;
			// TODO: state fired and comment not empty
			//if()

			if (!string.IsNullOrEmpty(errors))
			{
				new AlertDialog.Builder(Activity)
							   .SetTitle(Resource.String.error_caption)
							   .SetMessage("Обнаружены следующие проблемы:" + System.Environment.NewLine + errors)
							   .SetCancelable(false)
							   .SetPositiveButton(Resource.String.ok_button, (dialog, args) =>
							   {
								   if (dialog is Dialog)
								   {
									   ((Dialog)dialog).Dismiss();
								   }
							   })
							   .Show();
				return;
			}
		}

		public void Save()
		{
			//throw new NotImplementedException();
			return;
		}
	}
}

