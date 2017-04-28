using System;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

using Realms;

using V4App = Android.Support.V4.App;

using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Fragments
{
	public class DoctorWorkPlacesFragment : V4App.Fragment
	{
		Realm DB;
		Doctor Doctor;
		bool IsLockMainWorkPlace;
		LinearLayout WPTable;
		
		public static DoctorWorkPlacesFragment Create(string doctorUUID, bool isLockMainWorkPlace = false)
		{
			var fragment = new DoctorWorkPlacesFragment();
			var arguments = new Bundle();
			arguments.PutString(Consts.C_DOCTOR_UUID, doctorUUID);
			arguments.PutBoolean(Consts.C_IS_LOCK_MAIN_WORK_PLACE, isLockMainWorkPlace);
			fragment.Arguments = arguments;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			DB = Realm.GetInstance();
			//DoctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID, string.Empty);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var mainView = inflater.Inflate(Resource.Layout.DoctorWorkPlacesFragment, container, false);
			
			WPTable = mainView.FindViewById<LinearLayout>(Resource.Id.dwpfMainLL);

			mainView.FindViewById<Button>(Resource.Id.dwpfAddB).Click += (s, e) =>
			{
				AddWorkPlace();
			};
			
			IsLockMainWorkPlace = Arguments.GetBoolean(Consts.C_IS_LOCK_MAIN_WORK_PLACE, false);
			var doctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID);
			if (string.IsNullOrEmpty(doctorUUID))
			{
				AddWorkPlace();
				return mainView;
			}
			else 
			{
				Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);
				var workPlaces = DBHelper.GetAll<WorkPlace>(DB).Where(wp => wp.Doctor == doctorUUID);
				foreach (var place in workPlaces)
				{
					AddWorkPlace(place);
				}
			}
			
			return mainView;
		}

		#region WorkPlaceHolder
		public class WorkPlaceHolder : Java.Lang.Object
		{
			public AutoCompleteTextView Hospital;
			public EditText Cabinet;
			public EditText Timetable;
			public Switch IsMain;
		}
		#endregion

		void AddWorkPlace(WorkPlace workPlace = null)
		{
			var workPlaceItem = Activity.LayoutInflater.Inflate(Resource.Layout.WorkPlaceItem, WPTable, false);

			var holder = new WorkPlaceHolder
			{
				Hospital = workPlaceItem.FindViewById<AutoCompleteTextView>(Resource.Id.wpiHospitalACTV),
				Cabinet = workPlaceItem.FindViewById<EditText>(Resource.Id.wpiCabinetET),
				Timetable = workPlaceItem.FindViewById<EditText>(Resource.Id.wpiTimetableET),
				IsMain = workPlaceItem.FindViewById<Switch>(Resource.Id.wpiIsMainS)
			};

			workPlaceItem.SetTag(Resource.String.ViewHolder, holder);
			workPlaceItem.SetTag(Resource.String.IsChanged, false);

			if (workPlace != null) {
				workPlaceItem.SetTag(Resource.String.WorkPlaceUUID, workPlace.UUID);
				workPlaceItem.SetTag(Resource.String.HospitalUUID, workPlace.Hospital);
				holder.Hospital.Text = DBHelper.GetHospital(DB, workPlace.Hospital).GetName();
				holder.Cabinet.Text = workPlace.Cabinet;
				holder.Timetable.Text = workPlace.Timetable;
				holder.IsMain.Checked = workPlace.IsMain;
			}
			
			if (IsLockMainWorkPlace) {
				holder.Hospital.Enabled = false;
				holder.IsMain.Enabled = false;
			}
			
			holder.Hospital.Adapter = new HospitalSuggestionAdapter(Activity);
			holder.Hospital.ItemClick += (sender, e) => {
				var actv = ((AutoCompleteTextView)sender);
				var hospitalHolder = (actv.Adapter as HospitalSuggestionAdapter)[e.Position];
				actv.SetTag(Resource.String.HospitalUUID, hospitalHolder.UUID);
				
				var parent =  (GridLayout)actv.Parent;	
				parent.SetTag(Resource.String.IsChanged, true);
			};
			
			holder.Cabinet.AfterTextChanged += EditTextAfterTextChanged;
			holder.Timetable.AfterTextChanged += EditTextAfterTextChanged;
			
			holder.IsMain.CheckedChange += (s, e) =>
			{
				if (e.IsChecked)
				{
					// TODO: simplify
					var switcher = (Switch)s;
					var parent = switcher.Parent as GridLayout;
					parent.SetTag(Resource.String.IsChanged, true);
					var parent_parent = parent.Parent as LinearLayout;
					int index = parent_parent.IndexOfChild(parent);
					for (int c = 0; c < parent_parent.ChildCount; c++)
					{
						var item = parent_parent.GetChildAt(c);
						if (parent_parent.IndexOfChild(item) == index) continue;
						var otherSwitcher = item.FindViewById<Switch>(Resource.Id.wpiIsMainS);
						if (otherSwitcher.Checked)
						{
							otherSwitcher.Checked = false;
						}
					}
				}
			};
			
			WPTable.AddView(workPlaceItem);
		}

		void EditTextAfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
		{
			// TODO: simplify
			var editText = (EditText)sender;
			var parent =  (GridLayout)editText.Parent;	
			parent.SetTag(Resource.String.IsChanged, true);			
		}

		public override void OnResume()
		{
			base.OnResume();
			DBHelper.GetDB(ref DB);
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

		public bool Save(Transaction openedTransaction, Doctor doctor)
		{
			//if (string.IsNullOrEmpty(DoctorUUID) && string.IsNullOrEmpty(doctorUUID)) return;
			if (openedTransaction == null)
			{
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			if (doctor == null)
			{
				if (Doctor == null)
				{
					throw new Exception("`Doctor` field is NULL and `doctor` arguemnt is NULL");
				}
			}
			else
			{
				if ((Doctor != null) && Doctor.UUID != doctor.UUID)
				{
					throw new Exception("`Doctor.UUID` not equal `doctor.UUID`");
				}
			}

			bool wasChanges = false;
			if (WPTable.ChildCount > 0)
			{
				for(int c = 0; c < WPTable.ChildCount; c++)
				{
					var item = WPTable.GetChildAt(c);
					var workPlaceUUID = (string)item.GetTag(Resource.String.WorkPlaceUUID);
					var hospital = item.FindViewById<AutoCompleteTextView>(Resource.Id.wpiHospitalACTV);
					var hospitalUUID = (string)hospital.GetTag(Resource.String.HospitalUUID);
					if (!string.IsNullOrEmpty(hospitalUUID))
					{
						WorkPlace wp = null;
						if (string.IsNullOrEmpty(workPlaceUUID))
						{
							wasChanges = true;
							wp = DBHelper.Create<WorkPlace>(DB, openedTransaction);
						}
						else
						{
							var isChanged = (bool)item.GetTag(Resource.String.IsChanged);
							if (isChanged) {
								wasChanges = true;
								wp = DBHelper.Get<WorkPlace>(DB, workPlaceUUID);
							}
						}
						
						if (wp == null) continue;

						wp.Doctor = doctor.UUID;
						wp.Hospital = hospitalUUID;
						wp.IsMain = item.FindViewById<Switch>(Resource.Id.wpiIsMainS).Checked;
						wp.Cabinet = item.FindViewById<EditText>(Resource.Id.wpiCabinetET).Text;
						wp.Timetable = item.FindViewById<EditText>(Resource.Id.wpiTimetableET).Text;
						
						if (wp.IsMain) {
							doctor.MainWorkPlace = wp.UUID;
						}

						if (!wp.IsManaged) DBHelper.Save(DB, openedTransaction, wp);
					}
				}
			}

			return wasChanges;
		}
	}
}

