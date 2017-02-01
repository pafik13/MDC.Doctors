using System;

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
		//string DoctorUUID;
		LinearLayout WPTable;
		
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
			//DoctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID, string.Empty);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var mainView = inflater.Inflate(Resource.Layout.DoctorWorkPlacesFragment, container, false);
			
			WPTable = mainView.FindViewById<LinearLayout>(Resource.Id.dwpfMainLL);
			AddWorkPlace();

			mainView.FindViewById<Button>(Resource.Id.dwpfAddB).Click += (s, e) =>
			{
				AddWorkPlace();
			};

			return mainView;
		}
		
		void AddWorkPlace(WorkPlace workPlace = null)
		{
			var workPlaceItem = Activity.LayoutInflater.Inflate(Resource.Layout.WorkPlaceItem, WPTable, false);
			var hospital = workPlaceItem.FindViewById<AutoCompleteTextView>(Resource.Id.wpiHospitalACTV);
			var cabinet = workPlaceItem.FindViewById<EditText>(Resource.Id.wpiCabinetET);
			var timetable = workPlaceItem.FindViewById<EditText>(Resource.Id.wpiTimetableET);
			var isMain = workPlaceItem.FindViewById<Switch>(Resource.Id.wpiIsMainS);

			if (workPlace != null) {
				workPlaceItem.SetTag(Resource.String.WorkPlaceUUID, workPlace.UUID);
				workPlaceItem.SetTag(Resource.String.IsChanged, false);
				hospital.SetTag(Resource.String.HospitalUUID, workPlace.Hospital);
				cabinet.Text = workPlace.Cabinet;
				timetable.Text = workPlace.Timetable;
				isMain.Checked = workPlace.IsMain;
			}

			isMain.CheckedChange += (s, e) =>
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

			hospital.Adapter = new HospitalSuggestionAdapter(Activity);
			hospital.ItemClick += (sender, e) => {
				var actv = ((AutoCompleteTextView)sender);
				var hospitalHolder = (actv.Adapter as HospitalSuggestionAdapter)[e.Position];
				actv.SetTag(Resource.String.HospitalUUID, hospitalHolder.UUID);
				
				var parent =  (GridLayout)actv.Parent;	
				parent.SetTag(Resource.String.IsChanged, true);
			};
			cabinet.AfterTextChanged += EditTextAfterTextChanged;
			timetable.AfterTextChanged += EditTextAfterTextChanged;

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

		public void Save(Transaction openedTransaction, Doctor doctor)
		{
			//if (string.IsNullOrEmpty(DoctorUUID) && string.IsNullOrEmpty(doctorUUID)) return;
			if (openedTransaction == null)
			{
				throw new ArgumentNullException(nameof(openedTransaction));
			}

			if (doctor == null)
			{
				throw new ArgumentNullException(nameof(doctor));
			}

			if (WPTable.ChildCount > 0)
			{
				var list = DBHelper.GetList<WorkPlace>(DB);
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
							wp = DBHelper.Create<WorkPlace>(DB, openedTransaction);
						}
						else
						{
							bool isChanged = (bool)item.GetTag(Resource.String.IsChanged);
							if (isChanged) {
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
			return;
		}
	}
}

