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

		//Spinner State;


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

		#region DoctorMainInfoHolder
		public class DoctorMainInfoHolder : Java.Lang.Object
		{
			public Spinner DoctorState;
			public EditText Name;
			public AutoCompleteTextView Speciality;
			public EditText Specialism;
			public AutoCompleteTextView Position;
			public EditText Phone;
			public EditText Email;
			public CheckBox CanParticipateInActions;
			public CheckBox CanParticipateInConference;
			public EditText Comment;
			public TextView LastAttendanceDate;
			public TextView NextAttendanceDate;
		}
		#endregion

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.DoctorMainInfoFragment, container, false);

			var viewHolder = new DoctorMainInfoHolder
			{
				DoctorState = view.FindViewById<Spinner>(Resource.Id.dmifStateS),
				Name = view.FindViewById<EditText>(Resource.Id.dmifNameET),
				Speciality = view.FindViewById<AutoCompleteTextView>(Resource.Id.dmifSpecialtyACTV),
				Specialism = view.FindViewById<EditText>(Resource.Id.dmifSpecialismET),
				Position = view.FindViewById<AutoCompleteTextView>(Resource.Id.dmifPositionACTV),
				Phone = view.FindViewById<EditText>(Resource.Id.dmifPhoneET),
				Email = view.FindViewById<EditText>(Resource.Id.dmifEmailET),
				CanParticipateInActions = view.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInActionsCB),
				CanParticipateInConference = view.FindViewById<CheckBox>(Resource.Id.dmifCanParticipateInConferenceCB),
				Comment = view.FindViewById<EditText>(Resource.Id.dmifCommentET),
				LastAttendanceDate = view.FindViewById<TextView>(Resource.Id.dmifLastAttendanceDateTV),
				NextAttendanceDate = view.FindViewById<TextView>(Resource.Id.dmifNextAttendanceDateTV)
			};

			view.SetTag(Resource.String.ViewHolder, viewHolder);

			var stateAdapter = new ArrayAdapter(Activity, Android.Resource.Layout.SimpleSpinnerItem, Doctor.GetStates());
			stateAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerItem);
			viewHolder.DoctorState.Adapter = stateAdapter;

			var doctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID);

			if (!string.IsNullOrEmpty(doctorUUID))
			{
				Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);

				// var shared = Activity.GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private);

				// var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
				// try {
				// Agent = MainDatabase.GetItem<Agent>(agentUUID);
				// } catch (Exception ex) {
				// Console.WriteLine(ex.Message);
				// Agent = null;
				// }

				#region State
				viewHolder.DoctorState.SetSelection((int)Doctor.GetState());
				#endregion

				viewHolder.Name.Text = Doctor.Name;

				#region Specialty
				viewHolder.Speciality.Text = Doctor.Specialty;
				#endregion

				viewHolder.Specialism.Text = Doctor.Specialism;

				#region Position
				viewHolder.Position.Text = Doctor.Position;
				#endregion

				viewHolder.Phone.Text = Doctor.Phone;
				viewHolder.Email.Text = Doctor.Email;
				viewHolder.CanParticipateInActions.Checked = Doctor.CanParticipateInActions;
				viewHolder.CanParticipateInConference.Checked = Doctor.CanParticipateInConference;
				viewHolder.Comment.Text = Doctor.Comment;
			}

			#region ADD_ACTIONS
			viewHolder.Name.AfterTextChanged += Name_AfterTextChanged;
			viewHolder.Phone.TextChanged += TextChanged;
			viewHolder.Email.TextChanged += TextChanged;
			viewHolder.Specialism.TextChanged += TextChanged;
			viewHolder.Comment.TextChanged += TextChanged;

			viewHolder.CanParticipateInActions.CheckedChange += CheckedChange;
			viewHolder.CanParticipateInConference.CheckedChange += CheckedChange;
			#endregion

			return view;
		}

		void Name_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			if (sender is EditText)
			{
				View.SetTag(Resource.String.IsChanged, true);
			}
		}

		void TextChanged(object sender, Android.Text.TextChangedEventArgs e)
		{
			if (sender is EditText)
			{
				View.SetTag(Resource.String.IsChanged, true);
			}
		}

		void CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (sender is CheckBox)
			{
				View.SetTag(Resource.String.IsChanged, true);
			}
		}

		public override void OnResume()
		{
			base.OnResume();
			DBHelper.GetDB(ref DB);
		}

		public override void OnPause()
		{
			base.OnPause();
		}

		public bool Save(Transaction openedTransaction, out Doctor doctor)
		{
			if (openedTransaction == null)
			{
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			doctor = Doctor;

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
				return false;
			}


			// TIP: save sectio
			var isChanged = (bool)View.GetTag(Resource.String.IsChanged);
			if (isChanged)
			{
				var viewHolder = View.GetTag(Resource.String.ViewHolder) as DoctorMainInfoHolder;

				if (doctor == null)
				{
					doctor = DBHelper.Create<Doctor>(DB, openedTransaction);
					doctor.SetState(DoctorState.dsActive);
				}
				else
				{
					doctor = Doctor;
					doctor.SetState((DoctorState)viewHolder.DoctorState.SelectedItemPosition);
				}

				doctor.UpdatedAt = DateTimeOffset.Now;
				doctor.IsSynced = false;
				doctor.Name = viewHolder.Name.Text;
				doctor.Specialty = viewHolder.Speciality.Text;
				doctor.Specialism = viewHolder.Specialism.Text;
				doctor.Position = viewHolder.Position.Text;
				doctor.Phone = viewHolder.Phone.Text;
				doctor.Email = viewHolder.Email.Text;
				doctor.CanParticipateInActions = viewHolder.CanParticipateInActions.Checked;
				doctor.CanParticipateInConference = viewHolder.CanParticipateInConference.Checked;
				doctor.Comment = viewHolder.Comment.Text;

				if (!doctor.IsManaged) DBHelper.Save(DB, openedTransaction, doctor);
			}

			return isChanged;
		}
	}
}

