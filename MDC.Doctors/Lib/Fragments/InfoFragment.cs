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
		#if DEBUG
		SD.Stopwatch Chrono;
		#endif
		
		Realm DB;
		Doctor Doctor;

		List<DrugBrand> Brands;
		List<WorkType> WorkTypes;
		Dictionary<int, Category> Categories;

		Button PotentialBrands;
		LinearLayout PotentialTable;
		Dictionary<string, View> PotentialDataCache;
		
		Button InfoBrands;
		LinearLayout InfoTable;
		LinearLayout InfoDataTable;
		Dictionary<string, View> InfoDataCache;

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

		// TODO: add savedInstanceState processing
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Chrono = new SD.Stopwatch();
			Chrono.Start();
			// DB = Realm.GetInstance();
			DBHelper.GetDB(ref DB);
		}
		
		public void RefreshInfo(ref bool isWorkPlaceChanged, ref bool isDocotorChanged)
		{
			if (isWorkPlaceChanged) {
				
			}
			
			if (isDocotorChanged) {
				Doctor = DBHelper.Get<Doctor>(DB, Doctor.UUID);
				RefreshDoctorInfo();
			}
			
			isWorkPlaceChanged = false;
			isDocotorChanged = false;
		}
		
		void RefreshDoctorInfo(View mainView = null)
		{			
			var view = mainView == null ? View : mainView;
			
			var specialityText = string.Empty;
			var speciality = DBHelper.Get<Specialty>(DB, Doctor.Specialty);
			if (speciality != null) {
				specialityText = speciality.name;
			}
			
			view.FindViewById<TextView>(Resource.Id.ifDoctorTV).Text =
				string.Concat(Doctor.Name, ", ", specialityText, ", ", Doctor.Specialism);
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
			RefreshDoctorInfo(mainView);
			
			var mainWorkPlace = DBHelper.Get<WorkPlace>(DB, Doctor.MainWorkPlace);
			var hospital = DBHelper.GetHospital(DB, mainWorkPlace.Hospital);
			mainView.FindViewById<TextView>(Resource.Id.ifHospitalTV).Text = string.Concat(hospital.GetName(), ", ", hospital.GetAddress(), ", ", hospital.GetPhone());

			Brands = DBHelper.GetList<DrugBrand>(DB);
			WorkTypes = DBHelper.GetList<WorkType>(DB);

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
			PotentialDataCache = new Dictionary<string, View>();
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

			
			InfoTable = mainView.FindViewById<LinearLayout>(Resource.Id.aaInfoTable);
			var infoTableHeader = inflater.Inflate(Resource.Layout.InfoTableHeader, InfoTable, false);
			InfoTable.AddView(infoTableHeader);
			InfoDataCache = new Dictionary<string, View>();
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
				viewHolder.Potential.Enabled = true;
				viewHolder.Potential.TextChanged += PotentialInfoChange;
				
				viewHolder.PrescribeOur.Enabled = true;
				viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
			}
			
			item.SetTag(Resource.String.ViewHolder, viewHolder);
			
			PotentialDataCache.Add(brand.uuid, item);
			PotentialTable.AddView(item);

			var layoutParams = item.LayoutParameters as LinearLayout.LayoutParams;
			var potentialItemMargin = (int)Resources.GetDimension(Resource.Dimension.potential_item_margin);

			layoutParams.SetMargins(potentialItemMargin, potentialItemMargin, potentialItemMargin, potentialItemMargin);
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
				var brand = Brands[b];
				if (PotentialDataCache.ContainsKey(brand.uuid)) {
					cacheBrands.Add(brand.uuid, PotentialDataCache[brand.uuid].Parent != null);
				} else {
					cacheBrands.Add(brand.uuid, false);
				}
			}
			
			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle(Resource.String.brand_pick_caption)
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
				           Brands.Select(item => item.name).ToArray(),
				           cacheBrands.Values.ToArray(),
						   (caller, arguments) => {
								cacheBrands[Brands[arguments.Which].uuid] = arguments.IsChecked;
						   }
					   )
						.SetPositiveButton(
						   Resource.String.save_button,
						   (caller, arguments) => {
							    foreach (var brand in Brands)
								{
								   if (cacheBrands[brand.uuid])
								   {
									   if (PotentialDataCache.ContainsKey(brand.uuid))
									   {
										   var view = PotentialDataCache[brand.uuid];
										   if (view.Parent == null) {
											   PotentialTable.AddView(view);
											}
									   } else {
										   AddPotentialItem(brand, true);
									   }
								   } else {
									   if (PotentialDataCache.ContainsKey(brand.uuid))
									   {
										   var view = PotentialDataCache[brand.uuid];
										   if (view.Parent != null) {
											   PotentialTable.RemoveView(view);
										   }
									   }
								   }
								}
								
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(Resource.String.cancel_button, (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}
		
		#region InfoViewHolder
		class InfoViewHolder : Java.Lang.Object
		{
			public TextView WorkTypes { get; set; }
			public EditText Callback { get; set; }
			public EditText Resume { get; set; }
			public EditText Goal { get; set; }
		}
		#endregion

		void AddInfoItem(Attendance attendance)
		{
			var infoTableItem = Activity.LayoutInflater.Inflate(Resource.Layout.InfoTableItem, InfoTable, false);
			infoTableItem.SetTag(Resource.String.AttendanceUUID, attendance.UUID);
			var infoDataTable = infoTableItem.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
				
			if (!attendance.IsFinished) {
				InfoDataTable = infoDataTable;
			}
			
			var infoDatas = DBHelper.GetList<InfoData>(DB).Where(iData => iData.Attendance == attendance.UUID);
			foreach (var iData in infoDatas)
			{
				var brand = DBHelper.Get<DrugBrand>(DB, iData.Brand);
				
				var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
				row.SetTag(Resource.String.InfoDataUUID, iData.UUID);
				row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand == null ? "<УДАЛЕНО>" : brand.name;
				row.SetTag(Resource.String.DrugBrandUUID, iData.Brand);
				
				if (attendance.IsFinished) {
					if (!string.IsNullOrEmpty(iData.WorkTypes)) {
						var workTypeUUIDs = iData.WorkTypes.Split(';');
						var workTypeNames = DBHelper.GetAll<WorkType>(DB).Where(wt => workTypeUUIDs.Contains(wt.uuid)).Select(wt => wt.name).ToArray();
						row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV).Text = string.Join(", ", workTypeNames);
					}
					
					row.FindViewById<EditText>(Resource.Id.idtiCallbackET).Text = iData.Callback;
					row.FindViewById<EditText>(Resource.Id.idtiResumeET).Text = iData.Resume;
					row.FindViewById<EditText>(Resource.Id.idtiGoalET).Text = iData.Goal;
				} else {
					var viewHolder = new InfoViewHolder
					{
						WorkTypes = row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV),
						Callback = row.FindViewById<EditText>(Resource.Id.idtiCallbackET),
						Resume = row.FindViewById<EditText>(Resource.Id.idtiResumeET),
						Goal = row.FindViewById<EditText>(Resource.Id.idtiGoalET)
					};

					if (!string.IsNullOrEmpty(iData.WorkTypes)) {
						var workTypeUUIDs = iData.WorkTypes.Split(';');
						var workTypeNames = DBHelper.GetAll<WorkType>(DB).Where(wt => workTypeUUIDs.Contains(wt.uuid)).Select(wt => wt.name).ToArray();
						viewHolder.WorkTypes.Text = string.Join(", ", workTypeNames);
					}
					
					viewHolder.Callback.Text = iData.Callback;
					viewHolder.Resume.Text = iData.Resume;
					viewHolder.Goal.Text = iData.Goal;
					
					InfoDataCache.Add(brand.uuid, row);
				}
				
				infoDataTable.AddView(row);
			}
			
			if (infoDataTable.ChildCount > 0) {
				infoTableItem.FindViewById<TextView>(Resource.Id.itiHolderTV).Visibility = ViewStates.Gone;
				infoDataTable.Visibility = ViewStates.Visible;
			} else {
				infoTableItem.FindViewById<TextView>(Resource.Id.itiHolderTV).Text = "<НЕТ ДАННЫХ>";
			}
			
			InfoTable.AddView(infoTableItem, 1);
		}
		
		void AddInfoItem(DrugBrand brand)
		{
			if (InfoDataTable == null) throw new Exception("InfoDataTable is NULL");
			
			var row = Activity.LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, InfoDataTable, false);
			row.SetTag(Resource.String.DrugBrandUUID, brand.uuid);

			row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand == null ? "<УДАЛЕНО>" : brand.name;

			var viewHolder = new InfoViewHolder
			{
				WorkTypes = row.FindViewById<TextView>(Resource.Id.idtiWorkTypesTV),
				Callback = row.FindViewById<EditText>(Resource.Id.idtiCallbackET),
				Resume = row.FindViewById<EditText>(Resource.Id.idtiResumeET),
				Goal = row.FindViewById<EditText>(Resource.Id.idtiGoalET)
			};
			
			viewHolder.WorkTypes.Enabled = true;
			viewHolder.WorkTypes.Click += WorkTypes_Click;
			
			viewHolder.Callback.Enabled = true;
			viewHolder.Resume.Enabled = true;
			viewHolder.Goal.Enabled = true;
			
			row.SetTag(Resource.String.ViewHolder, viewHolder);
			
			InfoDataCache.Add(brand.uuid, row);
			InfoDataTable.AddView(row, 0);

			(InfoDataTable.Parent as LinearLayout).FindViewById<TextView>(Resource.Id.itiHolderTV).Visibility = ViewStates.Gone;
			InfoDataTable.Visibility = ViewStates.Visible;
		}
		
		void InfoBrands_Click(object sender, EventArgs e)
		{
			var cacheBrands = new Dictionary<string, bool>(Brands.Count);
			for (int b = 0; b < Brands.Count; b++)
			{
				var brand = Brands[b];
				if (InfoDataCache.ContainsKey(brand.uuid)) {
					cacheBrands.Add(brand.uuid, InfoDataCache[brand.uuid].Parent != null);
				} else {
					cacheBrands.Add(brand.uuid, false);
				}			
			}
			
			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle(Resource.String.brand_pick_caption)
					   .SetCancelable(false)
					   .SetMultiChoiceItems(
				           Brands.Select(item => item.name).ToArray(),
				           cacheBrands.Values.ToArray(),
						   (caller, arguments) => {
								cacheBrands[Brands[arguments.Which].uuid] = arguments.IsChecked;
						   }
					   )
						.SetPositiveButton(
						   Resource.String.save_button,
						   (caller, arguments) => {
							   	foreach (var brand in Brands)
								{
								   if (cacheBrands[brand.uuid])
								   {
									   if (InfoDataCache.ContainsKey(brand.uuid))
									   {
										   var view = InfoDataCache[brand.uuid];
										   if (view.Parent == null) {
											   InfoDataTable.AddView(view);
											}
									   } else {
										   AddInfoItem(brand);
									   }
								   } else {
									   if (InfoDataCache.ContainsKey(brand.uuid))
									   {
										   var view = InfoDataCache[brand.uuid];
										   if (view.Parent != null) {
											   InfoDataTable.RemoveView(view);
										   }
									   }
								   }
								}
								
								(caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(Resource.String.cancel_button, (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}
		
		void WorkTypes_Click(object sender, EventArgs e)
		{
			var cacheWorkTypes = new Dictionary<string, bool>(WorkTypes.Count);
			for (int wt = 0; wt < WorkTypes.Count; wt++)
			{
				cacheWorkTypes.Add(WorkTypes[wt].uuid, false);
			}
			
			var tv = sender as TextView;
			var workTypeUUIDs = (string)tv.GetTag(Resource.String.WorkTypeUUIDs);
			if (!string.IsNullOrEmpty(workTypeUUIDs)) {
				foreach (var uuid in workTypeUUIDs.Split(';')) {
					cacheWorkTypes[uuid] = true;
				}
			}

			
			new Android.App.AlertDialog.Builder(Activity)
					   .SetTitle(Resource.String.brand_pick_caption)
			           .SetCancelable(true)
					   .SetMultiChoiceItems(
				           WorkTypes.Select(item => item.name).ToArray(),
				           cacheWorkTypes.Values.ToArray(),
						   (caller, arguments) => {
								cacheWorkTypes[WorkTypes[arguments.Which].uuid] = arguments.IsChecked;
						   }
				          )
			           .SetPositiveButton(
						   Resource.String.save_button,
						   (caller, arguments) => {
							    workTypeUUIDs = string.Empty;
								var workTypeNames = string.Empty;
								foreach (var workType in WorkTypes)
								{
								if (cacheWorkTypes[workType.uuid]) {
										workTypeUUIDs = string.Concat(workTypeUUIDs, workType.uuid, ";");
										workTypeNames = string.Concat(workTypeNames, workType.name, ", ");
									}
								}
								
								tv.Text = workTypeNames;
								tv.SetTag(Resource.String.WorkTypeUUIDs, workTypeUUIDs);
								
							    (caller as Android.App.Dialog).Dispose();
						   }
						)
						.SetNegativeButton(Resource.String.cancel_button, (caller, arguments) => { (caller as Android.App.Dialog).Dispose(); })
						.Show();
		}

		public void OnAttendanceStart(Attendance newAttendance)
		{
			// Добавить в начало строки и снять экран блокировки
			Locker.Visibility = ViewStates.Gone;
			Arrow.Visibility = ViewStates.Gone;
			Attendance = newAttendance;
			AddInfoItem(newAttendance);
			
			foreach(var item in PotentialDataCache.Values){
				var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
				
				viewHolder.Potential.Enabled = true;
				viewHolder.Potential.TextChanged += PotentialInfoChange;
				
				viewHolder.PrescribeOur.Enabled = true;
				viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
			}
			
			InfoBrands.Enabled = true;
			PotentialBrands.Enabled = true;
		}
		
		public void OnAttendanceResume(Attendance oldAttendance)
		{
			// Разблокировать строки для ввода данных
			Locker.Visibility = ViewStates.Gone;
			Arrow.Visibility = ViewStates.Gone;
			Attendance = oldAttendance;
			
			foreach(var item in PotentialDataCache.Values){
				var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
				
				viewHolder.Potential.Enabled = true;
				viewHolder.Potential.TextChanged += PotentialInfoChange;
				
				viewHolder.PrescribeOur.Enabled = true;
				viewHolder.PrescribeOur.TextChanged += PotentialInfoChange;
			}
			
			foreach(var item in InfoDataCache.Values){
				var viewHolder = item.GetTag(Resource.String.ViewHolder) as InfoViewHolder;
				
				viewHolder.WorkTypes.Enabled = true;
				viewHolder.WorkTypes.Click += WorkTypes_Click;
				
				viewHolder.Callback.Enabled = true;
				viewHolder.Resume.Enabled = true;
				viewHolder.Goal.Enabled = true;
			}
			
			
			if (InfoDataTable.ChildCount > 1) {
				(InfoDataTable.Parent as LinearLayout).FindViewById<TextView>(Resource.Id.itiHolderTV).Visibility = ViewStates.Gone;
			} else {
				(InfoDataTable.Parent as LinearLayout).FindViewById<TextView>(Resource.Id.itiHolderTV).Text = "ВЫБЕРИТЕ БРЕНДЫ";
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

			foreach(var item in PotentialDataCache.Values){
				var viewHolder = item.GetTag(Resource.String.ViewHolder) as PotentialViewHolder;
				
				viewHolder.Potential.Enabled = false;
				viewHolder.Potential.TextChanged -= PotentialInfoChange;
				
				viewHolder.PrescribeOur.Enabled = false;
				viewHolder.PrescribeOur.TextChanged -= PotentialInfoChange;
			}
			
			foreach(var item in InfoDataCache.Values){
				var viewHolder = item.GetTag(Resource.String.ViewHolder) as InfoViewHolder;
				
				viewHolder.WorkTypes.Enabled = false;
				viewHolder.WorkTypes.Click -= WorkTypes_Click;
				
				viewHolder.Callback.Enabled = false;
				viewHolder.Resume.Enabled = false;
				viewHolder.Goal.Enabled = false;
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
				foreach (var row in InfoDataCache.Values) {
					//var row = cacheItem.View as LinearLayout;
					var infoDataUUID = (string)row.GetTag(Resource.String.InfoDataUUID);
					if (row.Parent != null) {
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
						infoData.Attendance = Attendance.UUID;
						infoData.WorkTypes = (string)row.FindViewById<TextView>(Resource.Id.idtiCallbackET).GetTag(Resource.String.WorkTypeUUIDs);
						infoData.Callback = row.FindViewById<TextView>(Resource.Id.idtiCallbackET).Text;
						infoData.Resume = row.FindViewById<TextView>(Resource.Id.idtiResumeET).Text;
						infoData.Goal = row.FindViewById<TextView>(Resource.Id.idtiGoalET).Text;

						if (!infoData.IsManaged) DBHelper.Save(DB, transaction, infoData);
					} else {
						if (string.IsNullOrEmpty(infoDataUUID)) continue;
						
						DBHelper.Delete(DB, transaction, DBHelper.Get<InfoData>(DB, infoDataUUID));
					}

				}
				
				
				var categoryOrderInfos = new List<CategoryOrderInfo>();
				foreach (var item in PotentialDataCache.Values) {
					// var item = cacheItem.View as TableLayout;
					var potentialDataUUID = (string)item.GetTag(Resource.String.PotentialDataUUID);
					if (item.Parent != null) {
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
								potentialData.Doctor = Doctor.UUID;
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
						
						DBHelper.Delete(DB, transaction, DBHelper.Get<PotentialData>(DB, potentialDataUUID));
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