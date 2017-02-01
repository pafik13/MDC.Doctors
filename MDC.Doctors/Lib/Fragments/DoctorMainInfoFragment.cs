using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using V4App = Android.Support.V4.App;

using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Fragments
{
	public class DoctorMainInfoFragment : V4App.Fragment
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

			var mainView = inflater.Inflate(Resource.Layout.DoctorMainInfoFragment, container, false);

			var doctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID, string.Empty);
			if (string.IsNullOrEmpty(doctorUUID))
			{
				return mainView;
			}

			Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);

			//item.SetState((PharmacyState)State.SelectedItemPosition);
			mainView.FindViewById<EditText>(Resource.Id.dmifNameET).Text = Doctor.Name;
			mainView.FindViewById<AutoCompleteTextView>(Resource.Id.dmifSpecialtyACTV).Text = Doctor.Specialty;
			mainView.FindViewById<EditText>(Resource.Id.dmifSpecialismET).Text = Doctor.Specialism;
			mainView.FindViewById<AutoCompleteTextView>(Resource.Id.dmifPositionACTV).Text = Doctor.Position;
			mainView.FindViewById<EditText>(Resource.Id.dmifPhoneET).Text = Doctor.Phone;
			mainView.FindViewById<EditText>(Resource.Id.dmifEmailET).Text = Doctor.Email;
			mainView.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInActionsCB).Checked = Doctor.CanParticipateInActions;
			mainView.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInConferenceCB).Checked = Doctor.CanParticipateInConference;
			mainView.FindViewById<EditText>(Resource.Id.dmifCommentET).Text = Doctor.Comment;

			return mainView;
		}

		public override void OnResume()
		{
			base.OnResume();
		}

		public override void OnPause()
		{
			base.OnPause();
		}

		public Doctor Save(Transaction openedTransaction)
		{
			if (openedTransaction == null)
			{
				throw new ArgumentNullException(nameof(openedTransaction));
			}

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
				return null;
			}


			// TIP: save section
			Doctor item;
			if (Doctor == null)
			{
				item = DBHelper.Create<Doctor>(DB, openedTransaction);
				item.SetState(DoctorState.dsActive);
			}
			else
			{
				item = Doctor;
			}

			item.UpdatedAt = DateTimeOffset.Now;
			item.IsSynced = false;
			item.SetState(DoctorState.dsActive);
			//item.SetState((PharmacyState)State.SelectedItemPosition);
			item.Name = View.FindViewById<EditText>(Resource.Id.dmifNameET).Text;
			item.Specialty = View.FindViewById<AutoCompleteTextView>(Resource.Id.dmifSpecialtyACTV).Text;
			item.Specialism = View.FindViewById<EditText>(Resource.Id.dmifSpecialismET).Text;
			item.Position = View.FindViewById<AutoCompleteTextView>(Resource.Id.dmifPositionACTV).Text;
			item.Phone = View.FindViewById<EditText>(Resource.Id.dmifPhoneET).Text;
			item.Email = View.FindViewById<EditText>(Resource.Id.dmifEmailET).Text;
			item.CanParticipateInActions = View.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInActionsCB).Checked;
			item.CanParticipateInConference = View.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInConferenceCB).Checked;
			item.Comment = View.FindViewById<EditText>(Resource.Id.dmifCommentET).Text;

			if (!item.IsManaged) DBHelper.Save(DB, openedTransaction, item);

			return item;
		}
	}
}

