using System;
using System.Collections.Generic;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;
using Android.Views.InputMethods;

using Realms;

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors
{
	[Activity(Label = "ProfileActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class ProfileActivity : Activity
	{
		Realm DB;

		LinearLayout Content;
		ListView Table;
		DateTimeOffset[] Dates;
		int[] Keys;

		Dictionary<string, Dictionary<int, int>> ReportData;

		ViewSwitcher SearchSwitcher;

		ImageView SearchImage;

		EditText SearchEditor;

		public const int WeeksCount = 14;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			DB = Realm.GetInstance();

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Profile);

			FindViewById<Button>(Resource.Id.paCloseB).Click += (sender, e) => {
				Finish();
			};

			FindViewById<Button>(Resource.Id.paExitAppB).Click += (sender, e) => {
				int count = 0;

				count += DBSpec.CountItemsToSyncAll(DB);

				if (count > 0) {
					new AlertDialog.Builder(this)
					               .SetTitle(Resource.String.warning_caption)
								   .SetMessage("Перед выходом необходимо синхронизировать все данные!!!")
								   .SetCancelable(true)
								   .SetPositiveButton("OK", (caller, arguments) => {
									   if (caller is Dialog) {
										   (caller as Dialog).Dismiss();
									   }
								   })
								   .Show();
				} else {

					//GetSharedPreferences(MainActivity.C_MAIN_PREFS, FileCreationMode.Private)
					//	.Edit()
					//	.PutString(SigninDialog.C_USERNAME, string.Empty)
					//	.Commit();
					//MainDatabase.Dispose();
					Finish();
				}
			};

			Content = FindViewById<LinearLayout>(Resource.Id.paAttendanceByWeekLL);
			Table = FindViewById<ListView>(Resource.Id.paAttendanceByWeekTable);

			Dates = new DateTimeOffset[WeeksCount];
			var header = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			(header.GetChildAt(0) as TextView).Text = @"Недели";
			for (int w = 0; w < WeeksCount; w++) {
				Dates[w] = DateTimeOffset.UtcNow.AddDays(-7 * (WeeksCount - 1 - w));
				Keys[w] = Helper.GetWeekKey(Dates[w]);
				var hView = header.GetChildAt(w + 1);
				if (hView is TextView) {
					(hView as TextView).Text = Helper.GetIso8601WeekOfYear(Dates[w].UtcDateTime.Date).ToString();
				}
			}
			Content.AddView(header, 1);

			var shared = GetSharedPreferences(Consts.C_MAIN_PREFERENCES, FileCreationMode.Private);

			//FindViewById<TextView>(Resource.Id.paUsernameTV).Text = shared.GetString(SigninDialog.C_USERNAME, string.Empty);

			//var agentUUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);
			//try {
			//	var agent = MainDatabase.GetItem<Agent>(agentUUID);
			//	FindViewById<TextView>(Resource.Id.paShortNameTV).Text = agent.shortName;
			//} catch (Exception ex) {
			//	System.Diagnostics.Debug.WriteLine(ex.Message);
			//}

			SearchSwitcher = FindViewById<ViewSwitcher>(Resource.Id.paSearchVS);
			SearchSwitcher.SetInAnimation(this, Android.Resource.Animation.SlideInLeft);
			SearchSwitcher.SetOutAnimation(this, Android.Resource.Animation.SlideOutRight);

			SearchImage = FindViewById<ImageView>(Resource.Id.paSearchIV);
			SearchImage.Click += (sender, e) => {
				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				SearchSwitcher.ShowNext();
			};

			SearchEditor = FindViewById<EditText>(Resource.Id.paSearchET);

			SearchEditor.AfterTextChanged += (sender, e) => {
				var text = e.Editable.ToString();

				(Table.Adapter as AttendanceByWeekAdapter).SetSearchText(text);
			};

		}

		protected override void OnResume()
		{
			base.OnResume();

			//Helper.CheckIfTimeChangedAndShowDialog(this);

			var chrono = new System.Diagnostics.Stopwatch();
			chrono.Start();
			ReportData = DBSpec.GetProfileReportData(DB, Keys);

			var summer = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.AttendanceByWeekTableHeader, Table, false);
			(summer.GetChildAt(0) as TextView).Text = @"Итого";
			for (int w = 0; w < WeeksCount; w++) {
				var hView = summer.GetChildAt(w + 1);
				if (hView is TextView) {
					int key = Keys[w];
					int sum = 0;
					foreach (var item in ReportData) {
						sum += item.Value[key];
					}

					(hView as TextView).Text = sum.ToString();
				}
			}
			Content.AddView(summer, 2);

			Table.Adapter = new AttendanceByWeekAdapter(this, ReportData, Keys);
			
			chrono.Stop();

			Console.WriteLine("OnResume: {0}", chrono.ElapsedMilliseconds);
		}
	}
}

