using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using V4App = Android.Support.V4.App;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Fragments
{
	public class DoctorMainInfoFragment : V4App.Fragment, ISave
	{
		Realm DB;
		Doctor Doctor;

		public static DoctorMainInfoFragment Create(string doctorUUID)
		{
			var fragment = new DoctorMainInfoFragment();
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

			return inflater.Inflate(Resource.Layout.DoctorMainInfoFragment, container, false);
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		public override void OnPause()
		{
			base.OnPause();
		}

		public void Save()
		{
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


			// TIP: save section
			using (var transaction = DB.BeginWrite())
			{

				Doctor item;
				if (Doctor == null)
				{
					item = DBHelper.Create<Doctor>(DB, transaction);
					item.SetState(DoctorState.dsActive);
				}
				else
				{
					item = Doctor;
				}

				item.UpdatedAt = DateTimeOffset.Now;
				item.IsSynced = false;
				//item.SetState((PharmacyState)State.SelectedItemPosition);
				item.Name = View.FindViewById<EditText>(Resource.Id.daNameET).Text;
				item.Specialty = View.FindViewById<AutoCompleteTextView>(Resource.Id.daSpecialtyACTV).Text;
				item.Specialism = View.FindViewById<EditText>(Resource.Id.daSpecialismET).Text;
				item.Position = View.FindViewById<AutoCompleteTextView>(Resource.Id.daPositionACTV).Text;
				item.Phone = View.FindViewById<EditText>(Resource.Id.daPhoneET).Text;
				item.Email = View.FindViewById<EditText>(Resource.Id.daEmailET).Text;
				item.CanParticipateInActions = View.FindViewById<CheckBox>(Resource.Id.daCanParticipateInActionsCB).Checked;
				item.CanParticipateInConference = View.FindViewById<CheckBox>(Resource.Id.daCanParticipateInConferenceCB).Checked;
				item.Comment = View.FindViewById<EditText>(Resource.Id.daCommentET).Text;

				if (!item.IsManaged) DBHelper.Save(DB, transaction, item);

				transaction.Commit();
			}
		}
	}
}

