using System;
using System.Linq;
using SD = System.Diagnostics;

using Android.OS;
using Android.Views;
using Android.Widget;

using V4App = Android.Support.V4.App;

using Realms;

using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Fragments
{
	public class InfoFragment : V4App.Fragment, IAttendanceControl
	{

		SD.Stopwatch Chrono;
		Realm DB;
		Doctor Doctor;

		LinearLayout PotentialTable;
		LinearLayout InfoTable;

		TextView Locker;
		ImageView Arrow;

		Attendance Attendance;

		public static InfoFragment Create(string doctorUUID, string attendanceLastOrNewUUID)
		{
			var fragment = new InfoFragment();
			var arguments = new Bundle();
			arguments.PutString(Consts.C_DOCTOR_UUID, doctorUUID);
			arguments.PutString(Consts.C_ATTENDANCE_UUID, attendanceLastOrNewUUID);
			fragment.Arguments = arguments;
			return fragment;
		}

		// TODO: add savedInstanceStat processe
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Chrono = new SD.Stopwatch();
			Chrono.Start();
			DB = Realm.GetInstance();
		}

		#region InfoDataViewHolder
		class InfoDataViewHolder : Java.Lang.Object
		{
			public TextView Brand { get; set; }
			public Button WorkTypes { get; set; }
			public EditText Callback { get; set; }
			public EditText Resume { get; set; }
			public EditText Goal { get; set; }
			public EditText Comment { get; set; }
		}
		#endregion

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var mainView = inflater.Inflate(Resource.Layout.InfoFragment, container, false);

			var doctorUUID = Arguments.GetString(Consts.C_DOCTOR_UUID);
			Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);
			
			var speciality = DBHelper.Get<Specialty>(DB, Doctor.Specialty);
			mainView.FindViewById<TextView>(Resource.Id.ifDoctorTV).Text = 
				speciality == null ? Doctor.Name : string.Concat(Doctor.Name, ", ", speciality.name);
			
			var mainWorkPlace = DBHelper.Get<WorkPlace>(DB, Doctor.MainWorkPlace);
			var hospital = DBHelper.GetHospital(DB, mainWorkPlace.Hospital);
			mainView.FindViewById<TextView>(Resource.Id.ifHospitalTV).Text = string.Concat(hospital.GetName(), ", ", hospital.GetAddress(), ", ", hospital.GetPhone());
			
			
			var attendanceLastOrNewUUID = Arguments.GetString(Consts.C_ATTENDANCE_UUID);
			if (!string.IsNullOrEmpty(attendanceLastOrNewUUID))
			{
				Attendance = DBHelper.Get<Attendance>(DB, attendanceLastOrNewUUID);

				var infoDatas = DBHelper.GetList<InfoData>(DB).Where(iData => iData.Attendance == Attendance.UUID);

				mainView.FindViewById<EditText>(Resource.Id.ifGoalsET).Text = string.Join(", ", infoDatas.Select(iData => iData.Goal));
			}

			var brands = DBHelper.GetList<DrugBrand>(DB);

			PotentialTable = mainView.FindViewById<LinearLayout>(Resource.Id.aaPotentialTable);
			PotentialTable.WeightSum = 3; //brands.Count();
			for (int b = 0; b < brands.Count; b++)
			{
				var row = inflater.Inflate(Resource.Layout.PotentialTableItem, PotentialTable, false);
				row.FindViewById<TextView>(Resource.Id.ptiDrugBrandTV).Text = brands[b].name;
				// row.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1) { 
					// BottomMargin = 2, TopMargin = 2, LeftMargin = 2, RightMargin = 2 
				// };
				PotentialTable.AddView(row);
			}

			mainView.FindViewById<ImageView>(Resource.Id.aaPotentialRigthArrow).Click += (s, e) =>
			{
				for (int c = 0; c < PotentialTable.ChildCount; c++)
				{
					if (PotentialTable.GetChildAt(c).Visibility == ViewStates.Visible)
					{
						if ((c + 3) < PotentialTable.ChildCount)
						{
							PotentialTable.GetChildAt(c).Visibility = ViewStates.Gone;
							PotentialTable.GetChildAt(c + 3).Visibility = ViewStates.Visible;
						}
					}
				}
			};

			mainView.FindViewById<ImageView>(Resource.Id.aaPotentialLeftArrow).Click += (s, e) =>
			{
				for (int c = PotentialTable.ChildCount - 1; c >= 0; c--)
				{
					if (PotentialTable.GetChildAt(c).Visibility == ViewStates.Visible)
					{
						if ((c - 3) >= 0)
						{
							PotentialTable.GetChildAt(c).Visibility = ViewStates.Gone;
							PotentialTable.GetChildAt(c - 3).Visibility = ViewStates.Visible;
						}
					}
				}
			};

			InfoTable = mainView.FindViewById<LinearLayout>(Resource.Id.aaInfoTable);
			var infoTableHeader = inflater.Inflate(Resource.Layout.InfoTableHeader, InfoTable, false);
			InfoTable.AddView(infoTableHeader);

			foreach (var att in DBHelper.GetAll<Attendance>(DB))
			{
				if (att.Doctor == Doctor.UUID)
				{
					AddInfoItem(att);
				}
			}

			Locker = mainView.FindViewById<TextView>(Resource.Id.locker);
			Arrow = mainView.FindViewById<ImageView>(Resource.Id.arrow);

			return mainView;
		}

		void AddInfoItem(Attendance attendance, bool atTop = false, bool isEditable = false)
		{
			var brands = DBHelper.GetList<DrugBrand>(DB);

			var infoTableItem = Activity.LayoutInflater.Inflate(Resource.Layout.InfoTableItem, InfoTable, false);
			infoTableItem.SetTag(Resource.String.AttendanceUUID, attendance.UUID);
			infoTableItem.SetTag(Resource.String.IsChanged, isEditable);
			var infoDataTable = infoTableItem.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);

			var infoDatas = DBHelper.GetList<InfoData>(DB).Where(iData => iData.Attendance == attendance.UUID);
			if (infoDatas.Count() < 1)
			{
				foreach (var brand in brands)
				{
					var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
					row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand.name;
					row.SetTag(Resource.String.DrugBrandUUID, brand.uuid);
					if (isEditable)
					{
						row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click += WorkTypes_Click;
						row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiResumeET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiGoalET).Enabled = true;
					}
					infoDataTable.AddView(row);
				}
			}
			else
			{
				foreach (var iData in infoDatas)
				{
					var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
					row.SetTag(Resource.String.InfoDataUUID, iData.UUID);
					row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brands.First(b => b.uuid == iData.Brand).name;
					row.SetTag(Resource.String.DrugBrandUUID, iData.Brand);

					var workTypes = row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV);
					var callback = row.FindViewById<TextView>(Resource.Id.idtiCallbackET);
					var resume = row.FindViewById<TextView>(Resource.Id.idtiResumeET);
					var goal = row.FindViewById<TextView>(Resource.Id.idtiGoalET);

					callback.Text = iData.Callback;
					resume.Text = iData.Resume;
					goal.Text = iData.Goal;
					if (isEditable)
					{
						workTypes.Click += WorkTypes_Click;
						callback.Enabled = true;
						resume.Enabled = true;
						goal.Enabled = true;
					}
					infoDataTable.AddView(row);
				}
			}

			if (atTop)
			{
				InfoTable.AddView(infoTableItem, 1);
			}
			else
			{
				InfoTable.AddView(infoTableItem);
			}
		}

		void WorkTypes_Click(object sender, EventArgs e)
		{
			Toast.MakeText(Activity, "<Click on WorkTypes>", ToastLength.Short).Show();
		}

		public void OnAttendanceStart(Attendance newAttendance)
		{
			// Добавить в начало строки и снять экран блокировки
			Locker.Visibility = ViewStates.Gone;
			Arrow.Visibility = ViewStates.Gone;
			Attendance = newAttendance;
			AddInfoItem(newAttendance, true, true);
		}
		
		public void OnAttendanceResume(Attendance oldAttendance)
		{
			// Разблокировать строки для ввода данных
			Locker.Visibility = ViewStates.Gone;
			Arrow.Visibility = ViewStates.Gone;
			Attendance = oldAttendance;

			for (int c = 1; c < InfoTable.ChildCount; c++)
			{
				var item = InfoTable.GetChildAt(c);
				var attendanceUUID = (string)item.GetTag(Resource.String.AttendanceUUID);
				if (oldAttendance.UUID == attendanceUUID)
				{
					var table = item.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
					for (int cc = 0; cc < table.ChildCount; cc++)
					{
						var row = (LinearLayout)table.GetChildAt(cc);
						row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click += WorkTypes_Click;
						row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiResumeET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiGoalET).Enabled = true;
					}
				}
			}
		}

		public void OnAttendancePause()
		{
			// Заблокировать строки от ввода данных
			Locker.Text = "ПРОДОЛЖИТЕ ВИЗИТ";
			Locker.Visibility = ViewStates.Visible;
			Arrow.Visibility = ViewStates.Visible;

			for (int c = 1; c < InfoTable.ChildCount; c++)
			{
				var item = InfoTable.GetChildAt(c);
				var table = item.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
				for (int cc = 0; cc < table.ChildCount; cc++)
				{
					var row = (LinearLayout)table.GetChildAt(cc);
					row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click -= WorkTypes_Click;
					row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Enabled = false;
					row.FindViewById<TextView>(Resource.Id.idtiResumeET).Enabled = false;
					row.FindViewById<TextView>(Resource.Id.idtiGoalET).Enabled = false;
				}
			}
		}
		
		public void OnAttendanceStop()
		{
			// Сохранить данные
			using (var transaction = DB.BeginWrite())
			{
				for (int c = 1; c < InfoTable.ChildCount; c++)
				{
					var item = InfoTable.GetChildAt(c);
					var isChanged = (bool)item.GetTag(Resource.String.IsChanged);
					if (isChanged)
					{
						var attendaceUUID = (string)item.GetTag(Resource.String.AttendanceUUID);
						var table = item.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
						for (int cc = 0; cc < table.ChildCount; cc++)
						{
							var row = (LinearLayout)table.GetChildAt(cc);
							var drugBrandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
							var infoDataUUID = (string)row.GetTag(Resource.String.InfoDataUUID);
							InfoData infoData;
							if (string.IsNullOrEmpty(infoDataUUID))
							{
								infoData = DBHelper.Create<InfoData>(DB, transaction);
							}
							else
							{
								infoData = DBHelper.Get<InfoData>(DB, infoDataUUID);
							}
							infoData.Brand = drugBrandUUID;
							infoData.Attendance = attendaceUUID;
							//infoData.row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click -= WorkTypes_Click;
							infoData.Callback = row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Text;
							infoData.Resume = row.FindViewById<TextView>(Resource.Id.idtiResumeET).Text;
							infoData.Goal = row.FindViewById<TextView>(Resource.Id.idtiGoalET).Text;

							if (!infoData.IsManaged) DBHelper.Save(DB, transaction, infoData);
						}
					}
				}

				transaction.Commit();
			}
		}

	}
}