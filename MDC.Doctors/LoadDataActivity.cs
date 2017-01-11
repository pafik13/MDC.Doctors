using System;
using SD = System.Diagnostics;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Widget;

using Realms;

using RestSharp;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors
{
	[Activity(Label = "LoadDataActivity")]
	public class LoadDataActivity : Activity
	{
		Realm DB;

		Button GetData;
		Button CheckAll;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			DB = Realm.GetInstance();

			// Create your application here
			SetContentView(Resource.Layout.LoadData);

			GetData = FindViewById<Button>(Resource.Id.saGetDataB);

			GetData.Click += GetData_Click;

			CheckAll = FindViewById<Button>(Resource.Id.saCheckAll);

			CheckAll.Click += CheckAll_Click;
		}

		void CheckAll_Click(object sender, EventArgs e)
		{
			var mainLL = FindViewById<LinearLayout>(Resource.Id.saMainLL);
			for (int c = 0; c < mainLL.ChildCount; c++) {
				var view = mainLL.GetChildAt(c);
				if (view is CheckBox) {
					((CheckBox)view).Checked = true;
				}
			}
		}

		void GetData_Click(object sender, EventArgs e)
		{
			var client = new RestClient(@"http://front-sblcrm.rhcloud.com/");
			//var client = new RestClient(@"http://sbl-crm-project-pafik13.c9users.io:8080/");

			//if (FindViewById<CheckBox>(Resource.Id.saLoadPositionsCB).Checked) LoadPositions(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadNetsCB).Checked) LoadNets(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadSubwaysCB).Checked) LoadSubways(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadRegionsCB).Checked) LoadRegions(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadPlacesCB).Checked) LoadPlaces(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadCategoriesCB).Checked) LoadCategories(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadDrugSKUsCB).Checked) Load<DrugSKU>(client);
			if (FindViewById<CheckBox>(Resource.Id.saLoadDrugBrandsCB).Checked) Load<DrugBrand>(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadPromotionsCB).Checked) LoadPromotions(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadMessageTypesCB).Checked) LoadMessageTypes(client);
			//if (FindViewById<CheckBox>(Resource.Id.saLoadPhotoTypesCB).Checked) LoadPhotoTypes(client);
			//if (FindViewById<CheckBox>(Resource.Id.saContractsCB).Checked) LoadContracts(client);
			if (FindViewById<CheckBox>(Resource.Id.saWorkTypesCB).Checked) Load<WorkType>(client);
			//if (FindViewById<CheckBox>(Resource.Id.saMaterialsCB).Checked) LoadMaterials(client);
			//if (FindViewById<CheckBox>(Resource.Id.saListedHospitalsCB).Checked) LoadListedHospitals(client);

		}

		void Load<T>(RestClient client) where T: RealmObject, IEntityFromServer
		{
			var entity = typeof(T).Name;
			var request = new RestRequest(entity + "?limit=300&populate=false", Method.GET);
			var response = client.Execute<List<T>>(request);
			if (response.StatusCode == System.Net.HttpStatusCode.OK) {
				SD.Debug.WriteLine(string.Format("Получено {0} {1}", entity, response.Data.Count));
				SD.Debug.WriteLine(string.Format("ResponseURL {0} {1}", entity, response.ResponseUri.AbsolutePath));
				DBHelper.Save(DB, response.Data);
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			DBHelper.GetDB(ref DB);
		}
	}
}

