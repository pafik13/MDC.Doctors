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
using System.Collections.Generic;

namespace MDC.Doctors.Lib.Fragments
{
	public class InfoFragment : V4App.Fragment, IAttendanceControl
	{

		SD.Stopwatch Chrono;
		Realm DB;
		Doctor Doctor;

		List<DrugBrand> Brands;

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

			Brands = DBHelper.GetList<DrugBrand>(DB);

			var attendanceLastOrNewUUID = Arguments.GetString(Consts.C_ATTENDANCE_UUID);
			if (!string.IsNullOrEmpty(attendanceLastOrNewUUID))
			{
				Attendance = DBHelper.Get<Attendance>(DB, attendanceLastOrNewUUID);

				var infoDatas = DBHelper.GetList<InfoData>(DB).Where(iData => iData.Attendance == Attendance.UUID);

				mainView.FindViewById<EditText>(Resource.Id.ifGoalsET).Text = string.Join(", ", infoDatas.Select(iData => iData.Goal));
			}

			PotentialTable = mainView.FindViewById<LinearLayout>(Resource.Id.aaPotentialTable);
			PotentialTable.WeightSum = 3; //brands.Count();

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

			mainView.FindViewById<Button>(Resource.Id.aaPotentialBrandsB).Click += PotentialBrands_Click;

			Chrono.Stop();
			SD.Debug.WriteLine("InfoFragment: {0}", Chrono.ElapsedMilliseconds);

			return mainView;
		}

		#region PotentialViewHolder
		class PotentialViewHolder : Java.Lang.Object
		{
			public TextView Brand { get; set; }
			public EditText Potential { get; set; }
			public EditText PrescribeOur { get; set; }
			public EditText PrescribeOther { get; set; }
			public TextView Proportion { get; set; }
			public TextView Category { get; set; }
		}
		#endregion

		void PotentialInfoChange(object sender, EventArgs e)
		{
			var item = (sender as View).Parent.Parent as TableLayout;
			item.SetTag(Resource.String.CategoryUUID, string.Empty);

			var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;

			if (string.IsNullOrEmpty(viewHolder.PrescribeOur.Text) || string.IsNullOrEmpty(viewHolder.PrescribeOther.Text)) return;

			var prescribeOur = float.Parse(viewHolder.PrescribeOur.Text);
			var prescribeOther = float.Parse(viewHolder.PrescribeOther.Text);
			var proportion = prescribeOur / (prescribeOur + prescribeOther);
			viewHolder.Proportion.Text = proportion.ToString();

			if (string.IsNullOrEmpty(viewHolder.Potential.Text)) return;

			var potential = int.Parse(viewHolder.Potential.Text);

			var drugBrandUUID = (string)item.GetTag(Resource.String.DrugBrandUUID);

			var rules = DBHelper.GetAll<CategoryRule>(DB).Where(cr => cr.brand == drugBrandUUID);

			string categoryUUID = string.Empty;

			foreach (var rule in rules)
			{
				if ((rule.proportionStart <= proportion) && (proportion <= rule.proportionEnd)) {
					if ((rule.potentialStart <= potential) && (potential <= rule.potentialEnd))
					{
						categoryUUID = rule.category;
					}
				}
			}

			if (string.IsNullOrEmpty(categoryUUID))
			{
				viewHolder.Category.Text = "<Не найдена подходящая категория>";
			}
			else 
			{
				var category = DBHelper.Get<Category>(DB, categoryUUID);

				viewHolder.Category.Text = category.name;
				item.SetTag(Resource.String.CategoryUUID, category.uuid);
				item.SetTag(Resource.String.CategoryOrder, category.order);
			}

		}

		void AddPotentialItem(DrugBrand brand)
		{
			var item = Activity.LayoutInflater.Inflate(Resource.Layout.PotentialTableItem, PotentialTable, false);
			item.FindViewById<TextView>(Resource.Id.ptiDrugBrandTV).Text = brand.name;
			item.SetTag(Resource.String.DrugBrandUUID, brand.uuid);

			var viewHolder = new PotentialViewHolder
			{
				Potential = item.FindViewById<EditText>(Resource.Id.ptiPotentialET),
				PrescribeOur = item.FindViewById<EditText>(Resource.Id.ptiPrescribeOurET),
				PrescribeOther = item.FindViewById<EditText>(Resource.Id.ptiPrescribeOtherET),
				Proportion = item.FindViewById<TextView>(Resource.Id.ptiProportionTV),
				Category = item.FindViewById<TextView>(Resource.Id.ptiCategoryTV)
			};
			viewHolder.Potential.TextChanged += PotentialInfoChange;
			viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
			viewHolder.PrescribeOther.TextChanged += PotentialInfoChange;
			item.SetTag(Resource.String.ViewHolder, viewHolder);

			PotentialTable.AddView(item);
		}

		void AddPotentialItem(PotentialData potential)
		{
			var item = Activity.LayoutInflater.Inflate(Resource.Layout.PotentialTableItem, PotentialTable, false);
			item.FindViewById<TextView>(Resource.Id.ptiDrugBrandTV).Text = potential.UUID;
			item.SetTag(Resource.String.DrugBrandUUID, potential.Brand);

			var viewHolder = new PotentialViewHolder
			{
				Potential = item.FindViewById<EditText>(Resource.Id.ptiPotentialET),
				PrescribeOur = item.FindViewById<EditText>(Resource.Id.ptiPrescribeOurET),
				PrescribeOther = item.FindViewById<EditText>(Resource.Id.ptiPrescribeOtherET),
				Proportion = item.FindViewById<TextView>(Resource.Id.ptiProportionTV),
				Category = item.FindViewById<TextView>(Resource.Id.ptiCategoryTV)
			};
			viewHolder.Potential.TextChanged += PotentialInfoChange;
			viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
			viewHolder.PrescribeOther.TextChanged += PotentialInfoChange;
			item.SetTag(Resource.String.ViewHolder, viewHolder);

			PotentialTable.AddView(item);
		}

		void PotentialBrands_Click(object sender, EventArgs e)
		{
			//var button = (Button)sender;
			//var parent = button.Parent as LinearLayout;
			//var potentialTable = parent.FindViewById<LinearLayout>(Resource.Id.aaPotentialTable);

			var cacheBrands = new Dictionary<string, bool>(Brands.Count);
			for (int b = 0; b < Brands.Count; b++)
			{
				cacheBrands.Add(Brands[b].uuid, false);
			}

			var cacheViews = new Dictionary<string, View>(Brands.Count);
			for (int c = 0; c < PotentialTable.ChildCount; c++) {
				var row = PotentialTable.GetChildAt(c) as TableLayout;
				var brandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
				if (string.IsNullOrEmpty(brandUUID)) continue;
				cacheBrands[brandUUID] = true;
				cacheViews.Add(brandUUID, row);
			}
			
			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle("Выберите бренды:")
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
				           Brands.Select(item => item.name).ToArray(),
				           cacheBrands.Values.ToArray(),
						   (caller, arguments) => {
								cacheBrands[Brands[arguments.Which].uuid] = arguments.IsChecked;
						   }
					   )
						.SetPositiveButton(
						   "Сохранить",
						   (caller, arguments) => {
								foreach (var brand in Brands)
								{
								   if (cacheViews.ContainsKey(brand.uuid))
								   {
									   if (cacheBrands[brand.uuid]) continue;
									   PotentialTable.RemoveView(cacheViews[brand.uuid]);
									   continue;
									}
								   if (cacheBrands[brand.uuid])
								   {
									   AddPotentialItem(brand);
								   }
								}
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton("Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
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

		public struct CategoryOrderInfo
		{
			public string BrandName;
			public string CategoryName;
			public int CategoryOrder;
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

				var categoryOrderInfos = new List<CategoryOrderInfo>();
				for (int c = 1; c < PotentialTable.ChildCount; c++)
				{
					var item = InfoTable.GetChildAt(c);
					var isChanged = (bool)item.GetTag(Resource.String.IsChanged);
					if (isChanged)
					{
						var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
						var categoryUUID = (string)item.GetTag(Resource.String.CategoryUUID);

						if (string.IsNullOrEmpty(categoryUUID)) break;

						var category = DBHelper.Get<Category>(DB, categoryUUID);

						var potentialDataUUID = (string)item.GetTag(Resource.String.PotentialDataUUID);
						var drugBrandUUID = (string)item.GetTag(Resource.String.DrugBrandUUID);

						PotentialData potentialData;
						if (string.IsNullOrEmpty(potentialDataUUID))
						{
							potentialData = DBHelper.Create<PotentialData>(DB, transaction);
						}
						else
						{
							potentialData = DBHelper.Get<PotentialData>(DB, potentialDataUUID);
						}
						potentialData.Brand = drugBrandUUID;
						potentialData.Attendance = Attendance.UUID;
						potentialData.Potential = int.Parse(viewHolder.Potential.Text);
						potentialData.PrescriptionOfOur = float.Parse(viewHolder.PrescribeOur.Text);
						potentialData.PrescriptionOfOther = float.Parse(viewHolder.PrescribeOther.Text);
						potentialData.Proportion = float.Parse(viewHolder.Proportion.Text);
						potentialData.Category = categoryUUID;

						if (!potentialData.IsManaged) DBHelper.Save(DB, transaction, potentialData);

						var categoryOrderInfo = new CategoryOrderInfo
						{
							BrandName = DBHelper.Get<DrugBrand>(DB, drugBrandUUID).name,
							CategoryName = category.name,
							CategoryOrder = category.order
						};

						categoryOrderInfos.Add(categoryOrderInfo);
					}
				}

				categoryOrderInfos.Sort((x, y) => x.CategoryOrder.CompareTo(y.CategoryOrder));

				switch (categoryOrderInfos.Count)
				{
					case 0:
						break;
					case 1:
						Doctor.CategoryText = string.Format("{0}:({1})", categoryOrderInfos[0].BrandName, categoryOrderInfos[0].CategoryName);
						Doctor.CategoryOrderSum = categoryOrderInfos[0].CategoryOrder;
						break;
					default:
						Doctor.CategoryText = string.Format("{0}:({1}), {2}:({3})"
						                                   , categoryOrderInfos[0].BrandName, categoryOrderInfos[0].CategoryName
						                                   , categoryOrderInfos[1].BrandName, categoryOrderInfos[1].CategoryName
						                                   );
						Doctor.CategoryOrderSum = categoryOrderInfos.Sum(coi => coi.CategoryOrder);
						break;
				}

				transaction.Commit();
			}
		}

	}
}