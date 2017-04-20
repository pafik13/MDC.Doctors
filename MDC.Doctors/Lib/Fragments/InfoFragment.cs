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
		Dictionary<int, Category> Categories;

		Button PotentialBrands;
		LinearLayout PotentialTable;
		Dictionary<string, CacheItem> PotentialDataCache;

		Button InfoBrands;
		LinearLayout InfoTable;
		Dictionary<string, CacheItem> InfoDataCache;
		
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
			// DB = Realm.GetInstance();
			DBHelper.GetDB(ref DB);
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

		#region CacheItem
		struct CacheItem
		{
			public bool IsActive { get; set; }
			public View View { get; set; }
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

			Categories = new Dictionary<int, Category>();
			foreach (var category in DB.All<Category>())
			{
				Categories[category.order] = category;	
			}
			
			PotentialTable = mainView.FindViewById<LinearLayout>(Resource.Id.aaPotentialTable);

			foreach (var pot in DBHelper.GetAll<PotentialData>(DB))
			{
				if (pot.Doctor == Doctor.UUID)
				{
					AddPotentialItem(pot);
				}
			}
			
			PotentialBrands = mainView.FindViewById<Button>(Resource.Id.aaPotentialBrandsB);
			PotentialBrands.Click += PotentialBrands_Click;
			PotentialBrands.Enabled = false;
			PotentialDataCache = new Dictionary<string, CacheItem>();

			
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
			
			InfoBrands = mainView.FindViewById<Button>(Resource.Id.aaInfoBrandsB);
			InfoBrands.Click += InfoBrands_Click;
			InfoBrands.Enabled = false;
			InfoDataCache = new Dictionary<string, CacheItem>();

			Locker = mainView.FindViewById<TextView>(Resource.Id.locker);
			Arrow = mainView.FindViewById<ImageView>(Resource.Id.arrow);


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
			public TextView PrescribeOther { get; set; }
			public TextView Proportion { get; set; }
			public TextView Category { get; set; }
		}
		#endregion

		void PotentialInfoChange(object sender, EventArgs e)
		{
			var item = (sender as View).Parent.Parent as TableLayout;
			item.SetTag(Resource.String.CategoryUUID, string.Empty);
			item.SetTag(Resource.String.IsChanged, true);

			var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
			viewHolder.Category.Text = "Категория";
			
			if (string.IsNullOrEmpty(viewHolder.PrescribeOur.Text) || string.IsNullOrEmpty(viewHolder.Potential.Text)) return;

			var potential = int.Parse(viewHolder.Potential.Text);

			var prescribeOur = int.Parse(viewHolder.PrescribeOur.Text);

			if (prescribeOur > potential) {
				Toast.MakeText(Activity, "ОШИБКА: Выписка наших не может быть больше потенциала!", ToastLength.Short).Show();
				return;
			}

			var prescribeOther = potential - prescribeOur;

			var proportion = prescribeOur / potential;

			viewHolder.PrescribeOther.Text = prescribeOther.ToString();
			viewHolder.Proportion.Text = proportion.ToString();

			var drugBrandUUID = (string)item.GetTag(Resource.String.DrugBrandUUID);

			var rules = DBHelper.GetAll<CategoryRule>(DB).Where(cr => cr.brand == drugBrandUUID);

			Category category = null;

			foreach (var rule in rules)
			{
				if ((potential > rule.potential) && (proportion < rule.proportion)) category = Categories[1];

				if ((potential > rule.potential) && (proportion > rule.proportion)) category = Categories[2];

				if ((potential < rule.potential) && (proportion < rule.proportion)) category = Categories[4];

				if ((potential < rule.potential) && (proportion > rule.proportion)) category = Categories[3];
			}

			if (category == null)
			{
				viewHolder.Category.Text = "<Не найдена подходящая категория>";
			}
			else 
			{
				viewHolder.Category.Text = category.name;
				item.SetTag(Resource.String.CategoryUUID, category.uuid);
				item.SetTag(Resource.String.CategoryOrder, category.order);
			}

		}

		void AddPotentialItem(DrugBrand brand, bool isEditable = false, PotentialData potential = null)
		{
			var item = Activity.LayoutInflater.Inflate(Resource.Layout.PotentialTableItem, PotentialTable, false);
			item.FindViewById<TextView>(Resource.Id.ptiDrugBrandTV).Text = brand.name;
			item.SetTag(Resource.String.DrugBrandUUID, brand.uuid);
			item.SetTag(Resource.String.IsChanged, false);

			var viewHolder = new PotentialViewHolder
			{
				Potential = item.FindViewById<EditText>(Resource.Id.ptiPotentialET),
				PrescribeOur = item.FindViewById<EditText>(Resource.Id.ptiPrescribeOurET),
				PrescribeOther = item.FindViewById<TextView>(Resource.Id.ptiPrescribeOtherTV),
				Proportion = item.FindViewById<TextView>(Resource.Id.ptiProportionTV),
				Category = item.FindViewById<TextView>(Resource.Id.ptiCategoryTV)
			};

			if (potential != null)
			{
				item.SetTag(Resource.String.PotentialDataUUID, potential.UUID);

				viewHolder.Potential.Text = potential.Potential.ToString();
				viewHolder.PrescribeOur.Text = potential.PrescriptionOfOur.ToString();
				viewHolder.PrescribeOther.Text = potential.PrescriptionOfOther.ToString();
				viewHolder.Proportion.Text = potential.Proportion.ToString();

				if (string.IsNullOrEmpty(potential.Category)) {
					viewHolder.Category.Text = string.Empty;
				} else {
					viewHolder.Category.Text = DBHelper.Get<Category>(DB, potential.Category).name;
				}
				
			}
			
			if (isEditable) {
				viewHolder.Potential.TextChanged += PotentialInfoChange;
				viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
				PotentialDataCache.Add(brand.uuid, new CacheItem { IsActive = true, View = row});
			}
			
			item.SetTag(Resource.String.ViewHolder, viewHolder);

			PotentialTable.AddView(item);
		}

		void AddPotentialItem(PotentialData potential)
		{
			var brand = DBHelper.Get<DrugBrand>(DB, potential.Brand);

			AddPotentialItem(brand, false, potential);
		}

		void PotentialBrands_Click(object sender, EventArgs e)
		{
			var cacheBrands = new Dictionary<string, bool>(Brands.Count);
			for (int b = 0; b < Brands.Count; b++)
			{
				cacheBrands.Add(Brands[b].uuid, false);
			}

			for (int c = 0; c < PotentialTable.ChildCount; c++) {
				var row = PotentialTable.GetChildAt(c) as TableLayout;
				var brandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
				if (string.IsNullOrEmpty(brandUUID)) continue;
				cacheBrands[brandUUID] = true;
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
								   if (PotentialDataCache.ContainsKey(brand.uuid))
								   {
									   var cacheItem = PotentialDataCache[brand.uuid];
									   if (cacheItem.IsActive){
										   if (cacheItem.View.Parent == null) {
											   PotentialTable.AddView(cacheItem.View);
										   }
										   continue;
									   }
									   
									   if (cacheItem.View.Parent == null) continue;
									   
									   PotentialTable.RemoveView(PotentialTable.View);
									   
									   //var infoDataUUID = (string) (cacheViews[brand.uuid].Parent as TableLayout).GetTag(Resource.String.InfoDataUUID);
									   //if (string.IsNullOrEmpty(infoDataUUID)) continue;
									   //InfoDataToDelete.Add(infoDataUUID, true);
									   continue;
								   }
								   if (cacheBrands[brand.uuid])
								   {
									  AddPotentialItem(brand, true);
								   }
								}
								
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton("Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}
		

		void AddInfoItem(Attendance attendance, bool isEditable = false)
		{
			var brands = DBHelper.GetList<DrugBrand>(DB);

			var infoTableItem = Activity.LayoutInflater.Inflate(Resource.Layout.InfoTableItem, InfoTable, false);
			infoTableItem.SetTag(Resource.String.AttendanceUUID, attendance.UUID);
			infoTableItem.SetTag(Resource.String.IsChanged, isEditable);
			var infoDataTable = infoTableItem.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);

			var infoDatas = DBHelper.GetList<InfoData>(DB).Where(iData => iData.Attendance == attendance.UUID);
			foreach (var iData in infoDatas)
			{
				var brand = DBHelper.Get<DrugBrand>(DB, iData.Brand);
				
				var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
				row.SetTag(Resource.String.InfoDataUUID, iData.UUID);
				row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand == null ? "<УДАЛЕНО>" : brand.name;
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
					
					InfoDataCache.Add(brand.uuid, new CacheItem { IsActive = true, View = row});
				}
				infoDataTable.AddView(row);
			}
			
			if (infoDataTable.ChildCount > 1) {
				infoTableItem.FindViewById<TextView>(Resource.Id.itiHolderTV).Visibility = ViewStates.Gone;
			} else {
				infoTableItem.FindViewById<TextView>(Resource.Id.itiHolderTV).Text = "<НЕТ ДАННЫХ>";
			}
			
			InfoTable.AddView(infoTableItem, 1);
		}

		void InfoBrands_Click(object sender, EventArgs e)
		{
			var cacheBrands = new Dictionary<string, bool>(Brands.Count);
			for (int b = 0; b < Brands.Count; b++)
			{
				cacheBrands.Add(Brands[b].uuid, false);
			}

			var infoDataTable = InfoTable.GetChildAt(1).FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
			for (int c = 0; c < infoDataTable.ChildCount; c++) {
				var row = infoDataTable.GetChildAt(c) as TableLayout;
				var brandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
				if (string.IsNullOrEmpty(brandUUID)) continue;
				cacheBrands[brandUUID] = true;
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
								   if (InfoDataCache.ContainsKey(brand.uuid))
								   {
									   if (InfoDataCache[brand.uuid].IsActive){
										   if (InfoDataCache[brand.uuid].View.Parent == null) {
											   infoDataTable.AddView(InfoDataCache[brand.uuid].View);
										   }
										   continue;
									   }
									   
									   if (InfoDataCache[brand.uuid].View.Parent == null) continue;
									   
									   infoDataTable.RemoveView(InfoDataCache[brand.uuid].View);
									   
									   //var infoDataUUID = (string) (cacheViews[brand.uuid].Parent as TableLayout).GetTag(Resource.String.InfoDataUUID);
									   //if (string.IsNullOrEmpty(infoDataUUID)) continue;
									   //InfoDataToDelete.Add(infoDataUUID, true);
									   continue;
								   }
								   if (cacheBrands[brand.uuid])
								   {
									   //AddInfoItem(Attendance, true, true);
				
										var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
										row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand == null ? "<УДАЛЕНО>" : brand.name;
										row.SetTag(Resource.String.DrugBrandUUID, brand.uuid);

										row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click += WorkTypes_Click;
										row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Enabled = true;
										row.FindViewById<TextView>(Resource.Id.idtiResumeET).Enabled = true;
										row.FindViewById<TextView>(Resource.Id.idtiGoalET).Enabled = true;
										
										infoDataTable.AddView(row);
										
										InfoDataCache.Add(brand.uuid, new CacheItem { IsActive = true, View = row});
								   }
								}
								
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton("Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}
		
		void WorkTypes_Click(object sender, EventArgs e)
		{
			var cacheWorkTypes = new Dictionary<string, bool>(WorkTypes.Count);
			for (int wt = 0; wt < WorkTypes.Count; wt++)
			{
				cacheWorkTypes.Add(WorkTypes[wt].uuid, false);
			}
			
			var tv = sender as View;
			var workTypeUUIDs = tv.GetTag(Resource.String.WorkTypeUUIDs);
			if (!string.IsNullOrEmpty(workTypeUUIDs)) {
				foreach (var uuid in workTypeUUIDs.Split(';')) {
					cacheWorkTypes[uuid] = true;
				}
			}

			
			new Android.App.AlertDialog.Builder(Activity)
						   .SetTitle("Выберите виды работ:")
						   .SetCancelable(true)
					   .SetMultiChoiceItems(
				           WorkTypes.Select(item => item.name).ToArray(),
				           cacheWorkTypes.Values.ToArray(),
						   (caller, arguments) => {
								cacheWorkTypes[WorkTypes[arguments.Which].uuid] = arguments.IsChecked;
						   }
					   )
						.SetPositiveButton(
						   "Сохранить",
						   (caller, arguments) => {
							    var workTypeUUIDs = string.Empty;
								var workTypeNames = string.Empty;
								foreach (var workType in WorkTypes)
								{
									if (cacheWorkTypes[workType]) {
										workTypeUUIDs = string.Concat(workTypeUUIDs, workType.uuid, ";");
										workTypeNames = string.Concat(workTypeNames, workType.name, ";");
									}
								}
								
								tv.Text = workTypeNames;
								tv.SetTag(Resource.String.WorkTypeUUIDs, workTypeUUIDs);
								
							    (caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton("Отмена", (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}

		public void OnAttendanceStart(Attendance newAttendance)
		{
			// Добавить в начало строки и снять экран блокировки
			Locker.Visibility = ViewStates.Gone;
			Arrow.Visibility = ViewStates.Gone;
			Attendance = newAttendance;
			AddInfoItem(newAttendance, true);
			
			InfoBrands.Enabled = true;
			PotentialBrands.Enabled = true;
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
						var drugBrandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
						row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Click += WorkTypes_Click;
						row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiResumeET).Enabled = true;
						row.FindViewById<TextView>(Resource.Id.idtiGoalET).Enabled = true;
						if (InfoDataCache.ContainsKey(drugBrandUUID)) continue;
						
						InfoDataCache.Add(drugBrandUUID, new CacheItem { IsActive = true, View = row});
					}
				}
			}
			
			InfoBrands.Enabled = true;
			PotentialBrands.Enabled = true;
		}

		public void OnAttendancePause()
		{
			InfoBrands.Enabled = false;
			PotentialBrands.Enabled = false;
			
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
			InfoBrands.Enabled = false;
			PotentialBrands.Enabled = false;
			
			// Сохранить данные
			using (var transaction = DB.BeginWrite())
			{
				foreach (var cacheItem in InfoDataCache.Values) {
					var row = cacheItem.View as LinearLayout;
					var infoDataUUID = (string)row.GetTag(Resource.String.InfoDataUUID);
					if (cacheItem.IsActive) {
						var isChanged = (bool)row.Parent.GetTag(Resource.String.IsChanged);
						if (isChanged)
						{
							var attendaceUUID = (string)row.Parent.GetTag(Resource.String.AttendanceUUID);
							var drugBrandUUID = (string)row.GetTag(Resource.String.DrugBrandUUID);
							InfoData infoData;
							if (string.IsNullOrEmpty(infoDataUUID))
							{
								infoData = DBHelper.Create<InfoData>(DB, transaction);
							}
							else
							{
								infoData = DBHelper.Get<InfoData>(DB, infoDataUUID);
								infoData.UpdatedAt = DateTimeOffset.Now;
							}
							infoData.Brand = drugBrandUUID;
							infoData.Attendance = attendaceUUID;
							infoData.WorkTypes = row.FindViewById<TextView>(Resource.Id.idtiCallbackET).GetTag(Resource.String.WorkTypeUUIDs);
							infoData.Callback = row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Text;
							infoData.Resume = row.FindViewById<TextView>(Resource.Id.idtiResumeET).Text;
							infoData.Goal = row.FindViewById<TextView>(Resource.Id.idtiGoalET).Text;

							if (!infoData.IsManaged) DBHelper.Save(DB, transaction, infoData);
						}						
					} else {
						if (string.IsNullOrEmpty(infoDataUUID)) continue;
						
						DBHelper.Delete(BD, infoDataUUID);
					}

				}
				
				
				
				foreach (var cacheItem in InfoDataCache.Values) {
					var item = cacheItem.View as TableLayout;
					var potentialDataUUID = (string)item.GetTag(Resource.String.PotentialDataUUID);
					if (cacheItem.IsActive) {
						var isChanged = (bool)item.GetTag(Resource.String.IsChanged);
						if (isChanged)
						{
							var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
							var categoryUUID = (string)item.GetTag(Resource.String.CategoryUUID);

							var drugBrandUUID = (string)item.GetTag(Resource.String.DrugBrandUUID);

							PotentialData potentialData;
							if (string.IsNullOrEmpty(potentialDataUUID))
							{
								potentialData = DBHelper.Create<PotentialData>(DB, transaction);
								potentialData.Attendance = Attendance.UUID;
							}
							else
							{
								potentialData = DBHelper.Get<PotentialData>(DB, potentialDataUUID);
								potentialData.UpdatedAt = DateTimeOffset.Now;
								potentialData.AttendanceWithChanges = Attendance.UUID;	
							}
							potentialData.Brand = drugBrandUUID;
							potentialData.Potential = int.Parse(viewHolder.Potential.Text);
							potentialData.PrescriptionOfOur = int.Parse(viewHolder.PrescribeOur.Text);
							potentialData.PrescriptionOfOther = int.Parse(viewHolder.PrescribeOther.Text);
							potentialData.Proportion = float.Parse(viewHolder.Proportion.Text);
							potentialData.Category = categoryUUID;

							if (!potentialData.IsManaged) DBHelper.Save(DB, transaction, potentialData);

							if (string.IsNullOrEmpty(categoryUUID)) continue;

							var category = DBHelper.Get<Category>(DB, categoryUUID);

							var categoryOrderInfo = new CategoryOrderInfo
							{
								BrandName = DBHelper.Get<DrugBrand>(DB, drugBrandUUID).name,
								CategoryName = category.name,
								CategoryOrder = category.order
							};

							categoryOrderInfos.Add(categoryOrderInfo);
						}						
					} else {
						if (string.IsNullOrEmpty(potentialDataUUID)) continue;
						
						DBHelper.Delete(BD, potentialDataUUID);
					}

				}

				var categoryOrderInfos = new List<CategoryOrderInfo>();
				for (int c = 1; c < PotentialTable.ChildCount; c++)
				{
					var item = InfoTable.GetChildAt(c);
					var isChanged = (bool)item.GetTag(Resource.String.IsChanged);
					if (isChanged)
					{
						
					}
				}
				
				transaction.Commit();

				if (categoryOrderInfos.Count == 0) return;
				
				categoryOrderInfos.Sort((x, y) => x.CategoryOrder.CompareTo(y.CategoryOrder));
				
				DB.Write(() =>{
					switch (categoryOrderInfos.Count)
					{
						case 0:
							break;
						case 1:
							Doctor.CategoryText = string.Format("{0}:({1})", categoryOrderInfos[0].BrandName, categoryOrderInfos[0].CategoryName);
							Doctor.CategoryOrderSum = categoryOrderInfos[0].CategoryOrder;
							Doctor.UpdatedAt = DateTimeOffset.Now;
							break;
						default:
							Doctor.CategoryText = string.Format("{0}:({1}), {2}:({3})"
															   , categoryOrderInfos[0].BrandName, categoryOrderInfos[0].CategoryName
															   , categoryOrderInfos[1].BrandName, categoryOrderInfos[1].CategoryName
															   );
							Doctor.CategoryOrderSum = categoryOrderInfos.Sum(coi => coi.CategoryOrder);
							Doctor.UpdatedAt = DateTimeOffset.Now;
							break;
					}
				});


			}
		}

	}
}