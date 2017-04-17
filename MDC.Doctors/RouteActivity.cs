using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views.InputMethods;

using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Adapters;
using MDC.Doctors.Lib.Fragments;
using MDC.Doctors.Lib;
using Realms;
using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors
{
	[Activity(Label = "RouteActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class RouteActivity : FragmentActivity, ViewPager.IOnPageChangeListener
	{
		const int C_SEARCH_THRESHOLD = 2;

		DateTimeOffset SelectedDate;

		//ListView RouteDoctorTable;
		ExpandableListView RouteDoctorExpTable;
		List<string> listDataHeader;
		Dictionary<string, List<string>> listDataChild;
		int previousGroup = -1;


		LinearLayout RouteTable;

		ViewSwitcher SearchSwitcher;

		ImageView SearchImage;

		EditText SearchEditor;

		TextView Info;

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
			switch (position) {
				default:
					Info.Text = string.Format(
						@"Период планирования: {0} недели ({1} дней). Номер недели: {2}",
						Helper.WeeksInRoute, Helper.WeeksInRoute * 5, position + 1
					);
					break; ;
			}
		}


		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			DB = Realm.GetInstance();

			// Create your application here
			SetContentView(Resource.Layout.Route);

			FindViewById<Button>(Resource.Id.raCloseB).Click += (s, e) => {
				Finish();
			};

			//RouteDoctorTable = FindViewById<ListView>(Resource.Id.raDoctorTable);
			//RouteDoctorTable.Adapter = new DoctorsForRouteAdapter(this);

			//RouteDoctorTable.ItemClick += RouteDoctorTable_ItemClick;
			RouteDoctorExpTable = FindViewById<ExpandableListView>(Resource.Id.raDoctorExpTable);

			// Prepare list data
			var dataCollectSW = new Stopwatch();
			dataCollectSW.Start();
			FnGetListData();
			dataCollectSW.Stop();
			System.Diagnostics.Debug.WriteLine("Route, Data collect:{0}", dataCollectSW.ElapsedMilliseconds);

			//Bind list
			//RouteDoctorExpTable.SetAdapter(new ExpandableListAdapter(this, listDataHeader, listDataChild));

			FnClickEvents();



			SearchSwitcher = FindViewById<ViewSwitcher>(Resource.Id.raSearchVS);
			SearchSwitcher.SetInAnimation(this, Android.Resource.Animation.SlideInLeft);
			SearchSwitcher.SetOutAnimation(this, Android.Resource.Animation.SlideOutRight);

			SearchImage = FindViewById<ImageView>(Resource.Id.raSearchIV);
			SearchImage.Click += (sender, e) => {
				if (CurrentFocus != null) {
					var imm = (InputMethodManager)GetSystemService(InputMethodService);
					imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, HideSoftInputFlags.None);
				}

				SearchSwitcher.ShowNext();
			};

			SearchEditor = FindViewById<EditText>(Resource.Id.raSearchET);

			//SearchEditor.AfterTextChanged += (sender, e) => {
			//	var text = e.Editable.ToString();
			//	if (text.Length < C_SEARCH_THRESHOLD) return;

			//	var adapter = RouteDoctorTable.Adapter as DoctorsForRouteAdapter;
			//	adapter.SetSearchText(text);
			//};

			RouteTable = FindViewById<LinearLayout>(Resource.Id.raRouteTable);

			FindViewById<Button>(Resource.Id.raSelectDateB).Click += (sender, e) => {
				var dateDialog = MyDatePickerDialog.NewInstance(delegate (DateTime date) {
					System.Diagnostics.Debug.WriteLine("DatePicker:{0}", date.ToLongDateString());
					System.Diagnostics.Debug.WriteLine("DatePicker:{0}", new DateTimeOffset(date));
					SelectedDate = new DateTimeOffset(date, new TimeSpan(0, 0, 0)); ;
					RefreshTables();
				});
				dateDialog.Show(FragmentManager, MyDatePickerDialog.TAG);
			};

			Info = FindViewById<TextView>(Resource.Id.raInfoTV);
			Info.Text = string.Format(@"Период планирования: {0} недели ({1} дней)", Helper.WeeksInRoute, Helper.WeeksInRoute * 5);

			var switcher = FindViewById<ViewSwitcher>(Resource.Id.raSwitchViewVS);
			FindViewById<ImageView>(Resource.Id.raSwitchIV).Click += (sender, e) => {
				System.Diagnostics.Debug.WriteLine(@"switcher:{0}; Resource{1}", switcher.CurrentView.Id, Resource.Id.raContainerVP);
				if (switcher.CurrentView.Id != Resource.Id.raContainerVP) {
					Info.Text = string.Format(
						@"Период планирования: {0} недели ({1} дней). Номер недели: {2}",
						Helper.WeeksInRoute, Helper.WeeksInRoute * 5, 1
					);
					var pager = FindViewById<ViewPager>(Resource.Id.raContainerVP);
					pager.AddOnPageChangeListener(this);
					pager.Adapter = new RoutePagerAdapter(SupportFragmentManager);
				} else {
					Info.Text = string.Format(@"Период планирования: {0} недели ({1} дней)", Helper.WeeksInRoute, Helper.WeeksInRoute * 5);
				}
				switcher.ShowNext();
			};
		}

		void FnClickEvents()
		{
			//Listening to child item selection
			RouteDoctorExpTable.ChildClick += delegate (object sender, ExpandableListView.ChildClickEventArgs e)
			{
				Toast.MakeText(this, "child clicked", ToastLength.Short).Show();
			};

			//Listening to group expand
			//modified so that on selection of one group other opened group has been closed
			RouteDoctorExpTable.GroupExpand += delegate (object sender, ExpandableListView.GroupExpandEventArgs e)
			{

				if (e.GroupPosition != previousGroup)
					RouteDoctorExpTable.CollapseGroup(previousGroup);
				previousGroup = e.GroupPosition;
			};

			//Listening to group collapse
			RouteDoctorExpTable.GroupCollapse += delegate (object sender, ExpandableListView.GroupCollapseEventArgs e)
			{
				Toast.MakeText(this, "group collapsed", ToastLength.Short).Show();
			};

		}

		void FnGetListData()
		{
			var headers = new string[] {
				"Computer science",
				"Electrocs & comm.",
				"Mechanical"
			};

			listDataHeader = new List<string>();
			listDataChild = new Dictionary<string, List<string>>();
			var emptyUUID = Guid.Empty.ToString();

			var doctorsDict = DBSpec.GetDoctorsForRoute(DB, DateTimeOffset.Now);
			var hospitals = doctorsDict.ContainsKey(emptyUUID) ? new IHospital[doctorsDict.Keys.Count - 1] : new IHospital[doctorsDict.Keys.Count];
			var h = 0;
			foreach (var hospital in doctorsDict.Keys)
			{
				//listDataHeader.Add(hospital)
				listDataChild.Add(hospital, doctorsDict[hospital].Select(doc => doc.Doctor.Name).ToList());
				if (emptyUUID != hospital)
				{
					hospitals[h] = doctorsDict[hospital][0].Hospital;
					h++;
				}
			}

			listDataHeader = hospitals.OrderBy(
				(IHospital arg) => arg, new MainActivity.HospitalComparer(System.ComponentModel.ListSortDirection.Ascending)
			).Select((IHospital arg) => arg.GetUUID()).ToList();

			listDataHeader.Add(emptyUUID);

			RouteDoctorExpTable.SetAdapter(new ExpandableListAdapter(this, listDataHeader, listDataChild, doctorsDict));

			// Adding child data
			//listDataHeader.Add("Computer science");
			//listDataHeader.Add("Electrocs & comm.");
			//listDataHeader.Add("Mechanical");

			//for (int i = 0; i < 50; i++)
			//{
			//	listDataHeader.Add(string.Format("{0} - #{1}", headers[0], i));
			//	listDataHeader.Add(string.Format("{0} - #{1}", headers[1], i));
			//	listDataHeader.Add(string.Format("{0} - #{1}", headers[2], i));

			//	// Adding child data
			//	var lstCS = new List<string>();
			//	lstCS.Add("Data structure");
			//	lstCS.Add("C# Programming");
			//	lstCS.Add("Java programming");
			//	lstCS.Add("ADA");
			//	lstCS.Add("Operation reserach");
			//	lstCS.Add("OOPS with C");
			//	lstCS.Add("C++ Programming");

			//	var lstEC = new List<string>();
			//	lstEC.Add("Field Theory");
			//	lstEC.Add("Logic Design");
			//	lstEC.Add("Analog electronics");
			//	lstEC.Add("Network analysis");
			//	lstEC.Add("Micro controller");
			//	lstEC.Add("Signals and system");

			//	var lstMech = new List<string>();
			//	lstMech.Add("Instrumentation technology");
			//	lstMech.Add("Dynamics of machinnes");
			//	lstMech.Add("Energy engineering");
			//	lstMech.Add("Design of machine");
			//	lstMech.Add("Turbo machine");
			//	lstMech.Add("Energy conversion");

			//	// Header, Child data
			//	listDataChild.Add(string.Format("{0} - #{1}", headers[0], i), lstCS);
			//	listDataChild.Add(string.Format("{0} - #{1}", headers[1], i), lstEC);
			//	listDataChild.Add(string.Format("{0} - #{1}", headers[2], i), lstMech);
			//}
		}

		void RouteDoctorTable_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var lv = (ListView)sender;
			var adapter = (DoctorsForRouteAdapter)lv.Adapter;
			var item = adapter[e.Position];

			if (string.IsNullOrEmpty(item.MainWorkPlace))
			{
				Toast.MakeText(this, "У данного врача не выбрано/не введено основное рабочее место.", ToastLength.Short).Show();
				return;
			}

			var row = LayoutInflater.Inflate(Resource.Layout.RouteTableItem, RouteTable, false);
			row.SetTag(Resource.String.Position, e.Position);

			using (var transaction = DB.BeginWrite())
			{
				var newRouteItem = DBHelper.Create<RouteItem>(DB, transaction);
				newRouteItem.WorkPlace = item.MainWorkPlace;
				newRouteItem.Order = RouteTable.ChildCount;
				newRouteItem.Date = SelectedDate;
				if (!newRouteItem.IsManaged) DBHelper.Save(DB, transaction, newRouteItem);
				transaction.Commit();
				row.SetTag(Resource.String.RouteItemUUID, newRouteItem.UUID);
				adapter.AddCurrentRouteItem(newRouteItem);
			}

			row.SetTag(Resource.String.DoctorUUID, item.UUID);
			row.SetTag(Resource.String.WorkPlaceUUID, item.MainWorkPlace);

			row.FindViewById<TextView>(Resource.Id.rtiDoctorNameTV).Text = item.Name;
			row.FindViewById<TextView>(Resource.Id.rtiHospitalNameTV).Text = item.HospitalName;
			row.FindViewById<TextView>(Resource.Id.rtiHospitalAddressTV).Text = item.HospitalAddress;
			row.SetTag(Resource.String.RouteItemOrder, RouteTable.ChildCount);
			row.FindViewById<TextView>(Resource.Id.rtiOrderTV).Text = (RouteTable.ChildCount + 1).ToString();

			row.FindViewById<ImageView>(Resource.Id.rtiDeleteIV).Click += RowDelete_Click;
			row.LongClick += Row_LongClick;
			row.Drag += Row_Drag;

			RouteTable.AddView(row);
		}

		void RowDelete_Click(object sender, EventArgs e)
		{
			//var adapter = (DoctorsForRouteAdapter)RouteDoctorTable.Adapter;

			var rowForDelete = (LinearLayout)((ImageView)sender).Parent;

			var routeItemUUID = (string)rowForDelete.GetTag(Resource.String.RouteItemUUID);

			int pos = (int)rowForDelete.GetTag(Resource.String.Position);
			int index = (int)rowForDelete.GetTag(Resource.String.RouteItemOrder);

			RouteTable.RemoveView(rowForDelete);

			using (var transaction = DB.BeginWrite()) {
				var routeItem = DBHelper.Get<RouteItem>(DB, routeItemUUID);
				//adapter.RemoveCurrentRouteItem(routeItem);
				DBHelper.Delete(DB, transaction, routeItem);

				for (int c = index; c < RouteTable.ChildCount; c++) {
					var rowForUpdate = (LinearLayout)RouteTable.GetChildAt(c);
					routeItemUUID = (string)rowForUpdate.GetTag(Resource.String.RouteItemUUID);
					var updRouteItem = DBHelper.Get<RouteItem>(DB, routeItemUUID);
					updRouteItem.Order = c;
					updRouteItem.IsSynced = false;
					updRouteItem.UpdatedAt = DateTimeOffset.Now;
					rowForUpdate.SetTag(Resource.String.RouteItemOrder, c);
					rowForUpdate.FindViewById<TextView>(Resource.Id.rtiOrderTV).Text = (c + 1).ToString();
					if (!updRouteItem.IsManaged) DBHelper.Save(DB, transaction, updRouteItem);
				}
				transaction.Commit();
			}
		}

		void Row_LongClick(object sender, View.LongClickEventArgs e)
		{
			if (sender is LinearLayout) {
				var view = sender as LinearLayout;

				var index = (int)view.GetTag(Resource.String.RouteItemOrder);

				var data = ClipData.NewPlainText(@"RouteItemOrder", index.ToString());

				var shadow = new View.DragShadowBuilder(view);

				view.StartDrag(data, shadow, null, 0);
			}
		}

		void Row_Drag(object sender, View.DragEventArgs e)
		{
			if (sender is LinearLayout) {
				var view = sender as LinearLayout;
				switch (e.Event.Action) {
					case DragAction.Started:
						e.Handled = true;
						break;
					case DragAction.Entered:
						view.Visibility = ViewStates.Invisible;
						break;
					case DragAction.Exited:
						view.Visibility = ViewStates.Visible;
						break;
					case DragAction.Ended:
						view.Visibility = ViewStates.Visible;
						e.Handled = true;
						break;
					case DragAction.Drop:
						int clipedIndex = int.Parse(e.Event.ClipData.GetItemAt(0).Text);
						var index = (int)view.GetTag(Resource.String.RouteItemOrder);
						if (clipedIndex != index) {
							var dragedView = RouteTable.GetChildAt(clipedIndex);
							RouteTable.RemoveView(dragedView);
							RouteTable.AddView(dragedView, index);
							RouteTable.RemoveView(view);
							RouteTable.AddView(view, clipedIndex);

							using (var transaction = DB.BeginWrite()) {
								for (int c = 0; c < RouteTable.ChildCount; c++) {
									var rowForUpdate = (LinearLayout)RouteTable.GetChildAt(c);
									var routeItemUUID = (string)rowForUpdate.GetTag(Resource.String.RouteItemUUID);
									var updRouteItem = DBHelper.Get<RouteItem>(DB, routeItemUUID);
									updRouteItem.Order = c;
									updRouteItem.IsSynced = false;
									updRouteItem.UpdatedAt = DateTimeOffset.Now;
									rowForUpdate.SetTag(Resource.String.RouteItemOrder, c);
									rowForUpdate.FindViewById<TextView>(Resource.Id.rtiOrderTV).Text = (c + 1).ToString();
									if (!updRouteItem.IsManaged) DBHelper.Save(DB, transaction, updRouteItem);
								}
								transaction.Commit();
							}
							dragedView.SetTag(Resource.String.RouteItemOrder, index);
							view.SetTag(Resource.String.RouteItemOrder, clipedIndex);
						}
						view.Visibility = ViewStates.Visible;
						e.Handled = true;
						break;
				}
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			//Helper.CheckIfTimeChangedAndShowDialog(this);

			var now = DateTime.Now;
			SelectedDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0, 0, 0));
			RefreshTables();
		}

		void RefreshTables()
		{
			FindViewById<Button>(Resource.Id.raSelectDateB).Text = SelectedDate.Date.ToLongDateString();
			
			//var adapter = (RouteDoctorTable.Adapter as DoctorsForRouteAdapter);
			//if (SelectedDate.Date <= DateTimeOffset.Now.Date) {		
			//	RouteDoctorTable.Visibility = ViewStates.Gone;
			//} else {
			//	RouteDoctorTable.Visibility = ViewStates.Visible;
			//	adapter.SetSelectedDate(SelectedDate);
			//	adapter.SetSearchText(SearchEditor.Text);
			//}

			RouteTable.RemoveAllViews();
			//adapter.ClearCurrentRoute();
			foreach (var routeItem in DBSpec.GetRouteItems(DB, SelectedDate).OrderBy(ri => ri.Order)) {
				var row = LayoutInflater.Inflate(Resource.Layout.RouteTableItem, RouteTable, false);
				//var doctor = adapter.Get(routeItem.WorkPlace);
				//if (string.IsNullOrEmpty(doctor.UUID)) {
				//	row.FindViewById<TextView>(Resource.Id.rtiDoctorNameTV).Text = "<аптека не найдена>";
				//} else {
				//	row.FindViewById<TextView>(Resource.Id.rtiDoctorNameTV).Text = doctor.Name;
				//	row.FindViewById<TextView>(Resource.Id.rtiHospitalNameTV).Text = doctor.HospitalName;
				//	row.FindViewById<TextView>(Resource.Id.rtiHospitalAddressTV).Text = doctor.HospitalAddress;
				//}

				//adapter.AddCurrentRouteItem(routeItem);
				
				row.SetTag(Resource.String.RouteItemUUID, routeItem.UUID);
				row.SetTag(Resource.String.WorkPlaceUUID, routeItem.WorkPlace);

				row.SetTag(Resource.String.RouteItemOrder, routeItem.Order);
				row.FindViewById<TextView>(Resource.Id.rtiOrderTV).Text = (routeItem.Order + 1).ToString();

				var delImage = row.FindViewById<ImageView>(Resource.Id.rtiDeleteIV);
				var now = DateTime.Now;
				if (SelectedDate > new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new TimeSpan(0, 0, 0))) {
					delImage.Click += RowDelete_Click;
					delImage.Visibility = ViewStates.Visible;
				} else {
					delImage.Visibility = ViewStates.Gone;
				}

				row.LongClick += Row_LongClick;
				row.Drag += Row_Drag;

				RouteTable.AddView(row);
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
		}


		/**
		 * A pager adapter that represents <NUM_PAGES> fragments, in sequence.
		 */
		class RoutePagerAdapter : FragmentPagerAdapter
		{
			public RoutePagerAdapter(Android.Support.V4.App.FragmentManager fm) : base(fm)
			{
			}

			public override int Count {
				get {
					return Helper.WeeksInRoute;
				}
			}

			public override Android.Support.V4.App.Fragment GetItem(int position)
			{
				return RouteFragment.create(position);
			}
		}
	}
}
