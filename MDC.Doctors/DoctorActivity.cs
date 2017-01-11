using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Realms;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors
{
	[Activity(Label = "DoctorActivity", WindowSoftInputMode = SoftInput.StateHidden)]
	public class DoctorActivity : Activity
	{
		public const string C_DOCTOR_UUID = "C_DOCTOR_UUID";

		Realm DB;
		Doctor Doctor;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			DB = Realm.GetInstance();

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Doctor);

			FindViewById<Button>(Resource.Id.daCloseB).Click += (s, e) =>
			{
				Finish();
			};

			FindViewById<Button>(Resource.Id.daSaveB).Click += (s, e) =>
			{
				// TIP: check section
				var errors = string.Empty;
				// TODO: state fired and comment not empty
				//if()

				if (!string.IsNullOrEmpty(errors))
				{
					new AlertDialog.Builder(this)
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
					} else
                    {
						item = Doctor;
					}

					item.UpdatedAt = DateTimeOffset.Now;
					item.IsSynced = false;
					//item.SetState((PharmacyState)State.SelectedItemPosition);
                    item.Name = FindViewById<EditText>(Resource.Id.daNameET).Text;
                    item.Specialty = FindViewById<AutoCompleteTextView>(Resource.Id.daSpecialtyACTV).Text;
                    item.Specialism = FindViewById<EditText>(Resource.Id.daSpecialismET).Text;
                    item.Position = FindViewById<AutoCompleteTextView>(Resource.Id.daPositionACTV).Text;
                    item.Phone = FindViewById<EditText>(Resource.Id.daPhoneET).Text;
                    item.Email = FindViewById<EditText>(Resource.Id.daEmailET).Text;
                    item.CanParticipateInActions = FindViewById<CheckBox>(Resource.Id.daCanParticipateInActionsCB).Checked;
                    item.CanParticipateInConference = FindViewById<CheckBox>(Resource.Id.daCanParticipateInConferenceCB).Checked;
                    item.Comment = FindViewById<EditText>(Resource.Id.daCommentET).Text;

                    if (!item.IsManaged) DBHelper.Save(DB, transaction, item);

					transaction.Commit();
				}

				Finish();
			};
		}
	}
}

