using SD = System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using V4App = Android.Support.V4.App;
using Android.Support.V4.View;

using Realms;

using MDC.Doctors.Lib.Fragments;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors
{
	[Activity(Label = "DoctorActivity", WindowSoftInputMode = SoftInput.StateHidden)]
	public class DoctorActivity : V4App.FragmentActivity, ViewPager.IOnPageChangeListener
	{
		public const int C_NUM_PAGES = 2;

		Realm DB;


		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			return;
		}

		public void OnPageScrollStateChanged(int state)
		{
			return;
		}

		public void OnPageSelected(int position)
		{
			switch (position)
			{
				case 0:
					SelectMainInfo();
					break;
				case 1:
					SelectWorkPlaces();
					break;
			}
		}

		ViewPager Pager;

		View TabMainInfoView;

		View TabWorkPlacesView;

		void SelectMainInfo()
		{
			TabMainInfoView.SetBackgroundColor(Android.Graphics.Color.Black);
			TabWorkPlacesView.SetBackgroundColor(Android.Graphics.Color.Transparent);			
		}

		void SelectWorkPlaces()
		{
			TabMainInfoView.SetBackgroundColor(Android.Graphics.Color.Transparent);
			TabWorkPlacesView.SetBackgroundColor(Android.Graphics.Color.Black);			
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			// Create your application here
			SetContentView(Resource.Layout.Doctor);

			FindViewById<Button>(Resource.Id.daCloseB).Click += (s, e) =>
			{
				Finish();
			};


			var doctorUUID = Intent.GetStringExtra(Consts.C_DOCTOR_UUID);

			Pager = FindViewById<ViewPager>(Resource.Id.daContainerVP);
			Pager.AddOnPageChangeListener(this);
			Pager.Adapter = new DoctorPagerAdapter(SupportFragmentManager, doctorUUID, this);

			DB = Realm.GetInstance();

			FindViewById<Button>(Resource.Id.daSaveB).Click += (s, e) =>
			{
				SaveData();

				Finish();
			};

			TabMainInfoView = FindViewById<View>(Resource.Id.daTabMainInfoV);
			(TabMainInfoView.Parent as RelativeLayout).Click += (sender, e) => { Pager.CurrentItem = 0;/*SelectMainInfo();*/ };
			TabWorkPlacesView = FindViewById<View>(Resource.Id.daTabWorkPlacesV);
			(TabWorkPlacesView.Parent as RelativeLayout).Click += (sender, e) => { Pager.CurrentItem = 1;/*SelectWorkPlaces();*/ };
		}

		void SaveData()
		{
			Doctor doctor = null;
			using (var transaction = DB.BeginWrite())
			{
				for (int f = 0; f < C_NUM_PAGES; f++)
				{
					var fragment = GetFragment(f);
					if (fragment is DoctorMainInfoFragment)
					{
						doctor = (fragment as DoctorMainInfoFragment).Save(transaction);
						break;
					}
				}

				if (doctor == null) return;

				for (int f = 0; f < C_NUM_PAGES; f++)
				{
					var fragment = GetFragment(f);
					if (fragment is DoctorWorkPlacesFragment)
					{
						(fragment as DoctorWorkPlacesFragment).Save(transaction, doctor);
						break;
					}
				}

				transaction.Commit();
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			//SaveData();
		}

		V4App.Fragment GetFragment(int position)
		{
			string tag = string.Concat("android:switcher:", Pager.Id, ":", position);
			var fragment = SupportFragmentManager.FindFragmentByTag(tag);
			return fragment;
		}

		/**
		 * A pager adapter that represents <NUM_PAGES> fragments.
		 */
		class DoctorPagerAdapter : V4App.FragmentPagerAdapter
		{
			readonly SD.Stopwatch Chrono;
			readonly Context Conext;
			readonly string DoctorUUID;

			public DoctorPagerAdapter(V4App.FragmentManager fm, string doctorUUID, Context context) : base(fm)
			{
				Chrono = new SD.Stopwatch();
				Conext = context;
				DoctorUUID = doctorUUID;
			}

			public override int Count
			{
				get
				{
					return C_NUM_PAGES;
				}
			}

			public override V4App.Fragment GetItem(int position)
			{
				V4App.Fragment result = null;
				string fragmentName = string.Empty;
				Chrono.Restart();
				switch (position)
				{
					case 0:
						fragmentName = "DoctorMainInfoFragment";
						result = DoctorMainInfoFragment.Create(DoctorUUID);
						break;
					case 1:
						fragmentName = "DoctorWorkPlacesFragment";
						result = DoctorWorkPlacesFragment.Create(DoctorUUID);
						break;
				}
				SD.Debug.WriteLine(string.Concat(typeof(DoctorPagerAdapter).FullName, fragmentName, ":", Chrono.ElapsedMilliseconds));
				Chrono.Stop();
				return result;
			}
		}
	}
}

