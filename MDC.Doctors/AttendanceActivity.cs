
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
	[Activity(Label = "AttendanceActivity", WindowSoftInputMode = SoftInput.StateHidden)]
	public class AttendanceActivity : Activity
	{
		Realm DB;
		Doctor Doctor;
		LinearLayout DrugBrandInfoTable;
		int CurrentDrugBrandInfoItem;
		LinearLayout InfoTable;
		Android.Util.SparseArray State;
		       
		protected override void OnCreate(Bundle savedInstanceState)
		{
			DB = Realm.GetInstance();

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Attendance);
			
			FindViewById<Button>(Resource.Id.aaCloseB).Click += (s, e) =>
			{
				Finish();
			};

			var doctorUUID = Intent.GetStringExtra(DoctorActivity.C_DOCTOR_UUID);

			Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);

			FindViewById<TextView>(Resource.Id.aaDoctorTV).Text = Doctor.Name;

			var brands = DBHelper.GetList<DrugBrand>(DB);
			brands.Add(new DrugBrand() { name = "Name1" });

			DrugBrandInfoTable = FindViewById<LinearLayout>(Resource.Id.aaDrugBrandInfoTable);
			DrugBrandInfoTable.WeightSum = 3; //brands.Count();
			for (int b = 0; b < brands.Count; b++)
			{
				var row = LayoutInflater.Inflate(Resource.Layout.DrugBrandInfoItem, DrugBrandInfoTable, false);
				row.FindViewById<TextView>(Resource.Id.dbiiDrugBrandS).Text = brands[b].name;
				row.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1) { BottomMargin = 2, TopMargin = 2, LeftMargin = 2, RightMargin = 2 };
				DrugBrandInfoTable.AddView(row);
			}

			FindViewById<Button>(Resource.Id.aaStartOrStopAttendanceB).Click += (s, e) =>
			{
				//DrugBrandInfoTable.GetChildAt(0).Visibility = ViewStates.Gone;				
				if (State == null)
				{
					State = new Android.Util.SparseArray();
					DrugBrandInfoTable.SaveHierarchyState(State);
				}
				else {
					DrugBrandInfoTable.RestoreHierarchyState(State);
				}

			};

			FindViewById<ImageView>(Resource.Id.aaDrugBrandInfoLRigthArrow).Click += (s, e) =>
			{
				//if ((CurrentDrugBrandInfoItem + 3) < brands.Count())
				//{
				//	DrugBrandInfoTable.RemoveViewAt(0);
				//	var row = LayoutInflater.Inflate(Resource.Layout.DrugBrandInfoItem, DrugBrandInfoTable, false);
				//	row.FindViewById<TextView>(Resource.Id.dbiiDrugBrandS).Text = brands[CurrentDrugBrandInfoItem + 3].name;
				//	row.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1) { BottomMargin = 2, TopMargin = 2, LeftMargin = 2, RightMargin = 2 };
				//	DrugBrandInfoTable.AddView(row);
				//	CurrentDrugBrandInfoItem++;
				//}
				for (int c = 0; c < DrugBrandInfoTable.ChildCount; c++)
				{
					if (DrugBrandInfoTable.GetChildAt(c).Visibility == ViewStates.Visible)
					{
						if ((c + 3) < DrugBrandInfoTable.ChildCount)
						{
							DrugBrandInfoTable.GetChildAt(c).Visibility = ViewStates.Gone;
							DrugBrandInfoTable.GetChildAt(c+3).Visibility = ViewStates.Visible;
						}
					}
				}
			};

			FindViewById<ImageView>(Resource.Id.aaDrugBrandInfoLeftArrow).Click += (s, e) =>
			{
				//if ((CurrentDrugBrandInfoItem - 1) >= 0)
				//{
				//	DrugBrandInfoTable.RemoveViewAt(2);
				//	var row = LayoutInflater.Inflate(Resource.Layout.DrugBrandInfoItem, DrugBrandInfoTable, false);
				//	row.FindViewById<TextView>(Resource.Id.dbiiDrugBrandS).Text = brands[CurrentDrugBrandInfoItem - 1].name;
				//	row.LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1) { BottomMargin = 2, TopMargin = 2, LeftMargin = 2, RightMargin = 2 };
				//	DrugBrandInfoTable.AddView(row, 0);
				//	CurrentDrugBrandInfoItem--;
				//}
				for (int c = DrugBrandInfoTable.ChildCount - 1; c >= 0; c--)
				{
					if (DrugBrandInfoTable.GetChildAt(c).Visibility == ViewStates.Visible)
					{
						if ((c - 3) >= 0)
						{
							DrugBrandInfoTable.GetChildAt(c).Visibility = ViewStates.Gone;
							DrugBrandInfoTable.GetChildAt(c - 3).Visibility = ViewStates.Visible;
						}
					}
				}
			};

			InfoTable = FindViewById<LinearLayout>(Resource.Id.aaInfoTable);

			var infoTableHeader = LayoutInflater.Inflate(Resource.Layout.InfoTableHeader, InfoTable, false);
			InfoTable.AddView(infoTableHeader);

			var infoTableItem = LayoutInflater.Inflate(Resource.Layout.InfoTableItem, InfoTable, false);
			var infoDataTable = infoTableItem.FindViewById<LinearLayout>(Resource.Id.itiInfoDataTable);
			foreach (var brand in brands)
			{
				var row = LayoutInflater.Inflate(Resource.Layout.InfoDataTableItem, infoDataTable, false);
				row.FindViewById<TextView>(Resource.Id.idtiBrandTV).Text = brand.name;
				infoDataTable.AddView(row);
			}
			InfoTable.AddView(infoTableItem);
		}

		protected override void OnResume()
		{
			base.OnResume();
			DBHelper.GetDB(ref DB);
		}
	}
}

