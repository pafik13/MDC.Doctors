using System.Collections.Generic;
using SDiag = System.Diagnostics;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;

using Realms;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib.Entities;
using System;
using System.Linq;

namespace MDC.Doctors
{
	[Activity(Label = "MDC Doctors", Theme = "@style/MyTheme.Splash", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
        Realm DB;
        ListView DoctorTable;

		#if DEBUG
			const string DEBUG_CATEGORY = "MainActivity";
		#endif

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

            DoctorTable = FindViewById<ListView>(Resource.Id.maDoctorTable);

			var add = FindViewById<ImageView>(Resource.Id.maAdd);
			add.Click += (sender, e) =>
			{
				StartActivity(new Intent(this, typeof(DoctorActivity)));
			};

			var sync = FindViewById<ImageView>(Resource.Id.maSync);
			//sync.Click += (sender, e) =>
			//{
			//	StartActivity(new Intent(this, typeof(SyncActivity)));
			//};
			sync.LongClick += (sender, e) =>
			{
				StartActivity(new Intent(this, typeof(LoadDataActivity)));
			};
			
		}

        protected override void OnResume()
        {
            base.OnResume();
			var sw = new SDiag.Stopwatch();
			sw.Start();
			Realm.DeleteRealm(RealmConfiguration.DefaultConfiguration);
			DBHelper.GetDB(ref DB);

			var inputedHospitals = DBHelper.GetAll<HospitalInputed>(DB);

			if (inputedHospitals.Count() < 1)
			{
				for (int i = 0; i < 3; i++)
				{
					DB.Write(() =>
					{
						DB.Add(new HospitalInputed
						{
							UUID = Guid.NewGuid().ToString(),
							Name = string.Concat("Hospital #", i),
							Key = string.Concat("Key #", i),
							Address = string.Concat("Address #", i),
							Area = string.Concat("Area #", i)
						});

						DB.Add(new HospitalChecked
						{
							uuid = Guid.NewGuid().ToString(),
							name = string.Concat("hospital #", i),
							key = string.Concat("key #", i),
							address = string.Concat("address #", i),
							area = string.Concat("area #", i)
						});
					});
				}

				DB.Write(() =>
				{
					DB.Add(new HospitalInputed
					{
						UUID = Guid.NewGuid().ToString(),
						Name = @"ГБУЗ ""Детская городская поликлиника № 69 ДЗМ"" Филиал № 1",
						Key = @"ГБУЗ ""ДГП № 69 ДЗМ"" Филиал № 1",
						Address = "Москва, Севастопольский проспект, 40",
						Area = "ЮЗАО"
					});

					DB.Add(new HospitalChecked
					{
						uuid = Guid.NewGuid().ToString(),
						name = @"ГБУЗ ""Детская городская поликлиника № 10 ДЗМ"" Филиал № 4",
						key = @"ГБУЗ ""ДГП № 10 ДЗМ"" Филиал № 4",
						address = "Москва, Профсоюзная улица, 52",
						area = "ЮЗАО"
					});

					DB.Add(new HospitalInputed
					{
						UUID = Guid.NewGuid().ToString(),
						Name = @"ГБУЗ ""Городская поликлиника № 22 ДЗМ"" Филиал № 1",
						Key = @"ГБУЗ ""ГП № 22 ДЗМ"" Филиал № 1",
						Address = "Москва, улица Цюрупы, 30/63",
						Area = "ЮЗАО"
					});

					DB.Add(new HospitalChecked
					{
						uuid = Guid.NewGuid().ToString(),
						name = @"ГБУЗ ""Детская городская поликлиника № 69 ДЗМ"" Филиал № 3",
						key = @"ГБУЗ ""ДГП № 69 ДЗМ"" Филиал № 3",
						address = "Москва, улица Винокурова, 14",
						area = "ЮЗАО"
					});

					DB.Add(new HospitalInputed
					{
						UUID = Guid.NewGuid().ToString(),
						Name = @"ГБУЗ ""Детская городская поликлиника № 69 ДЗМ"" Филиал № 4",
						Key = @"ГБУЗ ""ДГП № 69 ДЗМ"" Филиал № 4",
						Address = "Москва, улица Винокурова, 4, корпус 3",
						Area = "ЮЗАО"
					});

					DB.Add(new HospitalChecked
					{
						uuid = Guid.NewGuid().ToString(),
						name = @"ГБУЗ ""Городская поликлиника № 22 ДЗМ"" Филиал № 3",
						key = @"ГБУЗ ""ГП № 22 ДЗМ"" Филиал № 3",
						address = "Большая Черемушкинская улица, 6а",
						area = "ЮЗАО"
					});
 
					DB.Add(new HospitalInputed
					{
						UUID = Guid.NewGuid().ToString(),
						Name = @"ГБУЗ ""Городская поликлиника № 22 ДЗМ"" Филиал № 1",
						Key = @"ГБУЗ ""ГП № 22 ДЗМ"" Филиал № 1",
						Address = "Москва, улица Цюрупы, 30/63",
						Area = "ЮЗАО"
					});
	
					DB.Add(new HospitalChecked
					{
						uuid = Guid.NewGuid().ToString(),
						name = @"ГБУЗ ""Детская городская поликлиника № 69 ДЗМ"" Филиал № 3",
						key = @"ГБУЗ ""ДГП № 69 ДЗМ"" Филиал № 3",
						address = "Москва, улица Винокурова, 14",
						area = "ЮЗАО"
					});
	
					DB.Add(new HospitalInputed
					{
						UUID = Guid.NewGuid().ToString(),
						Name = @"ГБУЗ ""Детская городская поликлиника № 69 ДЗМ"" Филиал № 4",
						Key = @"ГБУЗ ""ДГП № 69 ДЗМ"" Филиал № 4",
						Address = "Москва, улица Винокурова, 4, корпус 3",
						Area = "ЮЗАО"
					});
	
					DB.Add(new HospitalChecked
					{
						uuid = Guid.NewGuid().ToString(),
						name = @"ГБУЗ ""Городская поликлиника № 22 ДЗМ"" Филиал № 3",
						key = @"ГБУЗ ""ГП № 22 ДЗМ"" Филиал № 3",
						address = "Большая Черемушкинская улица, 6а",
						area = "ЮЗАО"
					});
				});
			}

			SDiag.Debug.WriteLine(sw.ElapsedMilliseconds, DEBUG_CATEGORY);
			sw.Reset();
			DoctorTable.Adapter = new DoctorAdapter(this, DBHelper.GetList<Doctor>(DB));
			SDiag.Debug.WriteLine(sw.ElapsedMilliseconds, DEBUG_CATEGORY);
			sw.Stop();
		}

        protected override void OnPause()
        {
            base.OnPause();
        }

		protected override void OnStop()
		{
			base.OnStop();
			DoctorTable.Adapter = null;
			DB = null;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (DB != null && !DB.IsClosed)
			{
				DB.Dispose();
			}
		}
	}
}


