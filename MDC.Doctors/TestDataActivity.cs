using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using SDiag = System.Diagnostics;

using Android.App;
using Android.OS;
using Android.Widget;

using System.IO;
using Android.Content;
using Android.Accounts;
using Realms;
using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors
{
	[Activity(Label = "TestDataActivity")]
	public class TestDataActivity : Activity
	{
		const int PICKFILE_REQUEST_CODE = 3;

		Realm DB;

		Button GenerateData;
		Button CustomAction;
		Button Clear;
		Button ChangeWorkMode;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.TestData);

			GenerateData = FindViewById<Button>(Resource.Id.tdaGenerateDataB);

			GenerateData.Click += GenerateData_Click;

			CustomAction = FindViewById<Button>(Resource.Id.tdaCustomActionB);

			CustomAction.Click += CustomAction_Click;

			Clear = FindViewById<Button>(Resource.Id.tdaClearB);
			Clear.Click += Clear_Click;

			ChangeWorkMode = FindViewById<Button>(Resource.Id.tdaChangeWorkModeB);
			ChangeWorkMode.Click += (sender, e) => {
				switch (Helper.WorkMode) {
					case WorkMode.wmOnlyRoute:
						Helper.WorkMode = WorkMode.wmRouteAndRecommendations;
						break;
					case WorkMode.wmRouteAndRecommendations:
						Helper.WorkMode = WorkMode.wmOnlyRecommendations;
						break;
					case WorkMode.wmOnlyRecommendations:
						Helper.WorkMode = WorkMode.wmOnlyRoute;
						break;
				}
			};

			DBHelper.GetDB(ref DB);

			RefreshView();
		}

		void Clear_Click(object sender, EventArgs e)
		{
			DB.Write(() =>
			{
			   DB.RemoveAll();
			});
		}


		void CustomAction_Click(object sender, EventArgs e)
		{
			var intent = new Intent(Intent.ActionGetContent);
			intent.SetType("*/*");
			intent.AddCategory(Intent.CategoryOpenable);
			StartActivityForResult(intent, PICKFILE_REQUEST_CODE);


			//if (File.Exists(MainDatabase.DBPath)) {
			//	SDiag.Debug.WriteLine(MainDatabase.DBPath + " is Exists!");
			//	//var fi = new FileInfo(MainDatabase.DBPath);
			//	//var directory = fi.Directory.FullName;
			//	var newPath = Path.Combine(new FileInfo(MainDatabase.DBPath).Directory.FullName, fileName);
			//	if (!File.Exists(newPath)) File.Copy(dbFileLocation, newPath, true);

			//	if (File.Exists(newPath)) {
			//		SDiag.Debug.WriteLine(newPath + " is Exists!");
			//		MainDatabase.Dispose();
			//		Helper.C_DB_FILE_NAME = fileName;
			//		MainDatabase.Username = Helper.Username;
			//	}
			//}
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == PICKFILE_REQUEST_CODE) {
				// SDiag.Debug.WriteLine(resultCode);
				if (resultCode == Result.Ok) {
					//StartActivity(new Intent(Intent.ActionView, data.Data));
					string fileName = data.Data.LastPathSegment.Split(new char[] { ':' })[1]; //"bd0712a3-a5e7-4704-b565-889f673a393b.realm";
					//fileName = Path.GetFileName(fileName);
					//string dbFileLocation = Path.Combine(Helper.AppDir, fileName);
					string dbFileLoc = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, fileName);

					if (File.Exists(dbFileLoc)) {
						SDiag.Debug.WriteLine(dbFileLoc + " is Exists!");
					}
				}
			}
		}


		void GenerateData_Click(object sender, EventArgs e)
		{
			var cats = DBHelper.GetAll<Category>(DB);
			var newCats = new Category[5];
			if (cats.Count() == 0)
			{
				for (int i = 0; i < 5; i++)
				{
					newCats[i] = new Category
					{
						uuid = Guid.NewGuid().ToString(),
						name = (i + 1) + "-ая кат.",
						order = (i + 1)
					};
				};

				DB.Write(() =>
				{
					foreach (var cat in newCats)
					{
						DB.Add(cat);
					}
				});
			}
			else
			{
				newCats = cats.OrderBy(c => c.order).ToArray();
			}

			var brands = DBHelper.GetList<DrugBrand>(DB);
			var rules = DBHelper.GetAll<CategoryRule>(DB);
			if (rules.Count() == 0)
			{
				var newRules = new CategoryRule[brands.Count() * 5];

				for (int b = 0; b < brands.Count; b++)
				{
					for (int i = 0; i < 5; i++)
					{
						newRules[b * 5 + i] = new CategoryRule
						{
							uuid = Guid.NewGuid().ToString(),
							name = string.Format("Правило для категории {0} и препарата {1}", i, brands[b].name),
							brand = brands[b].uuid,
							potentialStart = (i * 3 + 1),
							potentialEnd = (i * 3 + 1) + 2,
							proportionStart = 0.0f,
							proportionEnd = 1.0f,
							category = newCats[i].uuid
						};
					};	
				};

				DB.Write(() =>
				{
					foreach (var rule in newRules)
					{
						DB.Add(rule);
					}
				});
			}

			RefreshView();
		}

		void RefreshView()
		{
		}

	}
}

