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


