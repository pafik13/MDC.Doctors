using System;
using System.Linq;
using SD = System.Diagnostics;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Locations;

using Realms;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Entities;

using V4App = Android.Support.V4.App;
using V4View = Android.Support.V4.View;
using Android.Views.InputMethods;

using MDC.Doctors.Lib.Interfaces;
using MDC.Doctors.Lib.Fragments;

namespace MDC.Doctors
{
	[Activity(Label = "AttendanceActivity", WindowSoftInputMode = SoftInput.StateHidden)]
	public class AttendanceActivity : V4App.FragmentActivity, V4View.ViewPager.IOnPageChangeListener, ILocationListener
	{
		public const int C_NUM_PAGES = 3;
		public const string C_TAG_FOR_DEBUG = "AttendanceActivity";
		public const double C_MIN_DURATION = -1.0D;

		Realm DB;
		Doctor Doctor;
		Attendance Attendance;
		LocationManager LocMgr;
		DateTimeOffset? AttendanceStart;
		
		bool WasDoctorChanged;
		bool WasWorkPlaceChanged;

		TextView FragmentTitle;

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
			switch (position) {
				case 0:
					FragmentTitle.Text = "СОБИРАЕМАЯ ИНФОРМАЦИЯ";

					var sw = new SD.Stopwatch();
					sw.Start();
					var doc = GetFragment(1);
					if (doc is DoctorMainInfoFragment) {
						using (var transaction = DB.BeginWrite())
						{
							WasDoctorChanged = (doc as DoctorMainInfoFragment).Save(transaction, out Doctor);
							transaction.Commit();
						}
					}
					var wp = GetFragment(2);
					if (wp is DoctorWorkPlacesFragment) {
						using (var transaction = DB.BeginWrite())
						{
							WasWorkPlaceChanged = (wp as DoctorWorkPlacesFragment).Save(transaction, Doctor);
						}
					}
					var info = GetFragment(0);
					if (info is InfoFragment) {
						(info as InfoFragment).RefreshInfo(ref WasDoctorChanged, ref WasWorkPlaceChanged);
					}
					
					sw.Stop();
					SD.Debug.WriteLine("OnPageSelected: pos-{0}-{1}", position, sw.ElapsedMilliseconds);
					break;
				case 1:
					FragmentTitle.Text = "ИНФОРМАЦИЯ О ВРАЧЕ";
					break;
				case 2:
					FragmentTitle.Text = "МЕСТА РАБОТЫ";
					break;
				default:
					FragmentTitle.Text = string.Concat("СТРАНИЦА ", (position + 1));
					break;
			}
		}

		V4View.ViewPager Pager;
		Button Close;
		Button StartOrStop;
		Button ResumeOrPause;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			DB = Realm.GetInstance();

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Attendance);

			FragmentTitle = FindViewById<TextView>(Resource.Id.aaTitleTV);

			Close = FindViewById<Button>(Resource.Id.aaCloseB);
			Close.Click += (s, e) =>
			{
				Finish();
			};

			var doctorUUID = Intent.GetStringExtra(Consts.C_DOCTOR_UUID);

			Doctor = DBHelper.Get<Doctor>(DB, doctorUUID);

			Attendance = DB.All<Attendance>()
			               .Where(att => att.Doctor == doctorUUID)
			               .OrderByDescending(att => att.When)
			               .FirstOrDefault();

			Pager = FindViewById<V4View.ViewPager>(Resource.Id.aaContainerVP);
			Pager.AddOnPageChangeListener(this);
			if (Attendance == null)
			{
				Pager.Adapter = new AttendancePagerAdapter(SupportFragmentManager, doctorUUID);
			}
			else
			{
				Pager.Adapter = new AttendancePagerAdapter(SupportFragmentManager, doctorUUID, Attendance.UUID);
			}

			ResumeOrPause = FindViewById<Button>(Resource.Id.aaResumeOrPauseAttendanceB);
			StartOrStop = FindViewById<Button>(Resource.Id.aaStartOrStopAttendanceB);

			ResumeOrPause.Click += (sender, e) =>
			{
				if (!IsLocationActive() || !IsInternetActive()) return;

				if (Attendance == null || Attendance.IsFinished) return;

				if (!AttendanceStart.HasValue)
				{
					AttendanceStart = DateTimeOffset.Now;

					// Location
					LocMgr = GetSystemService(LocationService) as LocationManager;
					var locationCriteria = new Criteria();
					locationCriteria.Accuracy = Accuracy.Coarse;
					locationCriteria.PowerRequirement = Power.Medium;
					string locationProvider = LocMgr.GetBestProvider(locationCriteria, true);
					SD.Debug.WriteLine("Starting location updates with " + locationProvider);
					LocMgr.RequestLocationUpdates(locationProvider, 5000, 1, this);
					// !Location

					for (int f = 0; f < C_NUM_PAGES; f++)
					{
						var fragment = GetFragment(f);
						if (fragment is IAttendanceControl)
						{
							(fragment as IAttendanceControl).OnAttendanceResume(Attendance);
						}
					}

					SetButtonsStatesWhenRunning();

					return;
				}

				if ((DateTimeOffset.Now - AttendanceStart.Value).TotalMilliseconds < 30) return;

				if (CurrentFocus != null)
				{
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				// Location
				LocMgr.RemoveUpdates(this);
				// !Location

				for (int f = 0; f < C_NUM_PAGES; f++)
				{
					var fragment = GetFragment(f);
					if (fragment is IAttendanceControl)
					{
						(fragment as IAttendanceControl).OnAttendancePause();
					}
				}

				DB.Write(() =>
				{
					Attendance.Duration += (DateTimeOffset.Now - AttendanceStart.Value).TotalMilliseconds;
					Attendance.UpdatedAt = DateTimeOffset.Now;
					Attendance.IsSynced = false;
				});

				AttendanceStart = null;

				SetButtonsStatesWhenPausing();
			};

			StartOrStop.Click += (sender, e) =>
			{
				if (!IsLocationActive() || !IsInternetActive()) return;

				if (Attendance == null || Attendance.IsFinished)
				{

					using (var transaction = DB.BeginWrite())
					{
						Attendance = DBHelper.Create<Attendance>(DB, transaction);
						Attendance.Doctor = Doctor.UUID;
						Attendance.When = DateTimeOffset.Now;
						Attendance.Duration = C_MIN_DURATION;

						Doctor.LastAttendance = Attendance.UUID;
						Doctor.LastAttendanceDate = Attendance.When;
						Doctor.NextAttendanceDate = (Helper.WeeksInRoute == 0) ?
							Attendance.When.AddDays(14) : Attendance.When.AddDays(Helper.WeeksInRoute * 7);
						
						if (!Attendance.IsManaged) DBHelper.Save(DB, transaction, Attendance);

						transaction.Commit();

						AttendanceStart = DateTimeOffset.Now;
					}

					if (!Attendance.IsManaged)
					{
						Toast.MakeText(this, "НОВЫЙ ВИЗИТ НЕ СОЗДАЛСЯ!", ToastLength.Long).Show();
					}

					if (Pager.Adapter is AttendancePagerAdapter)
					{
						((AttendancePagerAdapter)Pager.Adapter).AttendanceStart(Attendance.UUID);
					}

					// Location
					LocMgr = GetSystemService(LocationService) as LocationManager;
					var locationCriteria = new Criteria();
					locationCriteria.Accuracy = Accuracy.Fine;
					locationCriteria.PowerRequirement = Power.Medium;
					string locationProvider = LocMgr.GetBestProvider(locationCriteria, true);
					SD.Debug.WriteLine("Starting location updates with " + locationProvider);
					LocMgr.RequestLocationUpdates(locationProvider, 5000, 1, this);
					// !Location

					for (int f = 0; f < C_NUM_PAGES; f++)
					{
						var fragment = GetFragment(f);
						if (fragment is IAttendanceControl)
						{
							(fragment as IAttendanceControl).OnAttendanceStart(Attendance);
						}
					}

					SetButtonsStatesWhenRunning();

					return;
				}

				if (AttendanceStart.HasValue && (DateTimeOffset.Now - AttendanceStart.Value).TotalMilliseconds < 5) return;

				if (CurrentFocus != null)
				{
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				for (int f = 0; f < C_NUM_PAGES; f++)
				{
					var fragment = GetFragment(f);
					if (fragment is IAttendanceControl)
					{
						(fragment as IAttendanceControl).OnAttendanceStop();
					}
				}

				DB.Write(() =>
				{
					if (AttendanceStart.HasValue) {
						Attendance.Duration += (DateTimeOffset.Now - AttendanceStart.Value).TotalMilliseconds;
					}

					Attendance.UpdatedAt = DateTimeOffset.Now;
					Attendance.IsFinished = true;
					Attendance.IsSynced = false;
				});

				Finish();
			};

			// Set view visibles
			if (Attendance == null || Attendance.IsFinished)
			{
				SetButtonsStatesWhenNotStart();
			}
			else
			{
				SetButtonsStatesWhenPausing();
			}

			var btnMaterial = FindViewById<Button>(Resource.Id.aaMaterialB);
			btnMaterial.Click += (sender, e) =>
			{
				var materials = DBHelper.GetList<Material>(DB);

				new AlertDialog.Builder(this)
				               .SetTitle(Resource.String.material_pick_caption)
							   .SetCancelable(true)
							   .SetItems(
					               materials.Select(item => item.name).ToArray(),
								   (caller, arguments) =>
								   {
									   if (string.IsNullOrEmpty(materials[arguments.Which].fullPath))
									   {
										   Toast.MakeText(this, Resource.String.material_file_not_found, ToastLength.Short).Show();
										   return;
									   }
									   var intent = new Intent(Intent.ActionView);
									   var uri = Android.Net.Uri.FromFile(new Java.IO.File(materials[arguments.Which].fullPath));
									   intent.SetDataAndType(uri, "application/pdf");
									   intent.SetFlags(ActivityFlags.NoHistory);
									   StartActivity(intent);
								   })
							   .Show();
			};
		}

		void SetButtonsStatesWhenNotStart()
		{
			Close.Visibility = ViewStates.Visible;

			ResumeOrPause.Visibility = ViewStates.Gone;

			StartOrStop.SetBackgroundResource(Resource.Color.Light_Green_500);
			StartOrStop.Text = "НАЧАТЬ ВИЗИТ";
			StartOrStop.Visibility = ViewStates.Visible;
		}

		void SetButtonsStatesWhenRunning()
		{
			Close.Visibility = ViewStates.Gone;

			ResumeOrPause.SetBackgroundResource(Resource.Color.Yellow_600);
			ResumeOrPause.Text = "ОСТАНОВИТЬ ВИЗИТ";
			ResumeOrPause.Visibility = ViewStates.Visible;

			StartOrStop.SetBackgroundResource(Resource.Color.Deep_Orange_500);
			StartOrStop.Text = "ЗАКОНЧИТЬ ВИЗИТ";
			StartOrStop.Visibility = ViewStates.Visible;
		}

		void SetButtonsStatesWhenPausing()
		{
			Close.Visibility = ViewStates.Visible;

			ResumeOrPause.SetBackgroundResource(Resource.Color.Light_Green_500);
			ResumeOrPause.Text = "ПРОДОЛЖИТЬ ВИЗИТ";
			ResumeOrPause.Visibility = ViewStates.Visible;

			StartOrStop.SetBackgroundResource(Resource.Color.Deep_Orange_500);
			StartOrStop.Text = "ЗАКОНЧИТЬ ВИЗИТ";
			StartOrStop.Visibility = ViewStates.Visible;		
		}

		bool IsLocationActive()
		{
			var locMgr = GetSystemService(LocationService) as LocationManager;

			if (locMgr.IsProviderEnabled(LocationManager.NetworkProvider)
			  || locMgr.IsProviderEnabled(LocationManager.GpsProvider)
			   )
			{
				return true;
			}

			new AlertDialog.Builder(this)
						   .SetTitle(Resource.String.warning_caption)
						   .SetMessage(Resource.String.no_location_provider)
						   .SetCancelable(false)
						   .SetPositiveButton(Resource.String.on_button, delegate
						   {
							   var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
							   StartActivity(intent);
						   })
						   .Show();

			return false;
		}

		bool IsInternetActive()
		{
			var cm = GetSystemService(ConnectivityService) as Android.Net.ConnectivityManager;
			if (cm.ActiveNetworkInfo != null)
			{
				if (cm.ActiveNetworkInfo.IsConnectedOrConnecting)
				{
					return true;
				}
				new AlertDialog.Builder(this)
							   .SetTitle(Resource.String.error_caption)
							   .SetMessage(Resource.String.no_internet_connection)
							   .SetCancelable(false)
							   .SetNegativeButton(Resource.String.cancel_button, (sender, args) =>
							   {
								   if (sender is Dialog)
								   {
									   (sender as Dialog).Dismiss();
								   }
							   })
							   .Show();
				return false;
			}
			new AlertDialog.Builder(this)
						   .SetTitle(Resource.String.warning_caption)
						   .SetMessage(Resource.String.no_internet_provider)
						   .SetCancelable(false)
						   .SetPositiveButton(Resource.String.on_button, (sender, args) =>
						   {
							   var intent = new Intent(Android.Provider.Settings.ActionWirelessSettings);
							   StartActivity(intent);
						   })
						   .Show();
			return false;
		}


		protected override void OnResume()
		{
			base.OnResume();
			DBHelper.GetDB(ref DB);
		}

		#region location
		public void OnLocationChanged(Location location)
		{
			DB.Write(() =>
			{
				DB.Add(new GPSData
				{
					UUID = Guid.NewGuid().ToString(),
					CreatedBy = string.IsNullOrEmpty(Helper.AgentUUID) ? @"AgentUUID is Empty" : Helper.AgentUUID,
					CreatedAt = DateTimeOffset.Now,
					UpdatedAt = DateTimeOffset.Now,

					Attendance = Attendance.UUID,
					Accuracy = location.Accuracy,
					Altitude = location.Altitude,
					Bearing = location.Bearing,
					ElapsedRealtimeNanos = location.ElapsedRealtimeNanos,
					IsFromMockProvider = location.IsFromMockProvider,
					Latitude = location.Latitude,
					Longitude = location.Longitude,
					Provider = location.Provider,
					Speed = location.Speed
				});
			});
		}
					        
		public void OnProviderDisabled(string provider)
		{
			SD.Debug.WriteLine(provider + " disabled by user");
		}
		public void OnProviderEnabled(string provider)
		{
			SD.Debug.WriteLine(provider + " enabled by user");
		}
		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			SD.Debug.WriteLine(provider + " availability has changed to " + status);
		}
		#endregion

		public override void OnBackPressed()
		{
			if (AttendanceStart.HasValue) return;

			base.OnBackPressed();
		}

		/**
		 * @param containerViewId the ViewPager this adapter is being supplied to
		 * @param id pass in getItemId(position) as this is whats used internally in this class
		 * @return the tag used for this pages fragment
		 */
		public string MakeFragmentName(int containerViewId, long id)
		{
			return string.Concat("android:switcher:", containerViewId, ":", id);
		}
		/**
		 * @return may return null if the fragment has not been instantiated yet for that position - this depends on if the fragment has been viewed
		 * yet OR is a sibling covered by {@link android.support.v4.view.ViewPager#setOffscreenPageLimit(int)}. Can use this to call methods on
		 * the current positions fragment.
		 */
		public V4App.Fragment GetFragment(int position)
		{
			string tag = string.Concat("android:switcher:", Pager.Id, ":", position);
			var fragment = SupportFragmentManager.FindFragmentByTag(tag);
			return fragment;
		}

		/**
		 * A pager adapter that represents <NUM_PAGES> fragments.
		 */
		class AttendancePagerAdapter : V4App.FragmentPagerAdapter
		{
			readonly string DoctorUUID;
			readonly SD.Stopwatch Chrono;
			string AttendanceLastOrNewUUID;
			bool IsAttendanceRunning;

			public AttendancePagerAdapter(V4App.FragmentManager fm, string doctorUUID, string attLastOrNewUUID = null) : base(fm)
			{
				DoctorUUID = doctorUUID;
				AttendanceLastOrNewUUID = attLastOrNewUUID;
				IsAttendanceRunning = false;
				Chrono = new SD.Stopwatch();
			}

			public override int Count
			{
				get
				{
					return C_NUM_PAGES;
				}
			}

			public void AttendanceStart(string attendanceNewUUID = null)
			{
				if (!string.IsNullOrEmpty(attendanceNewUUID))
				{
					AttendanceLastOrNewUUID = attendanceNewUUID;
				}
				IsAttendanceRunning = true;
			}

			public override V4App.Fragment GetItem(int position)
			{
				V4App.Fragment result = null;
				string fragmentName = string.Empty;
				Chrono.Restart();
				switch (position)
				{
					case 0:
						fragmentName = "InfoFragment";
						result = InfoFragment.Create(DoctorUUID, AttendanceLastOrNewUUID);
						break;
					case 1:
						fragmentName = "DoctorMainInfoFragment";
						result = DoctorMainInfoFragment.Create(DoctorUUID);
						break;
					case 2:
						fragmentName = "DoctorWorkPlacesFragment";
						result = DoctorWorkPlacesFragment.Create(DoctorUUID, true);
						break;
				}
				SD.Debug.WriteLine(string.Concat(C_TAG_FOR_DEBUG, "-", fragmentName, ":", Chrono.ElapsedMilliseconds));
				Chrono.Stop();
				return result;
			}
		}
	}
}

