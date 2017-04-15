using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using SDiag = System.Diagnostics;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Accounts;
using Android.Content.PM;

using RestSharp;

using Realms;

//using Newtonsoft.Json;

using MDC.Doctors.Lib.Other;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors
{
	[Activity(Label = "SyncActivity", ScreenOrientation = ScreenOrientation.Landscape)]
	public class SyncActivity : Activity
	{
		Realm DB;

		public string ACCESS_TOKEN { get; private set; }
		public string HOST_URL { get; private set; }
		public string USERNAME { get; private set; }
		public string AGENT_UUID { get; private set; }

		public int Count { get; private set; }

		protected override void OnCreate(Bundle savedInstanceState)
		{
			DB = Realm.GetInstance();

			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);

			base.OnCreate(savedInstanceState);

			// Create your application here
			SetContentView(Resource.Layout.Sync);

			FindViewById<Button>(Resource.Id.saCloseB).Click += (s, e) => {
				Finish();
			};
			
			FindViewById<Button>(Resource.Id.saSyncB).Click += Sync_Click;

			//FindViewById<Button>(Resource.Id.saUploadPhotoB).Click += UploadPhoto_Click;

			//FindViewById<Button>(Resource.Id.saUploadRealmB).Click += UploadRealm_Click;

			var shared = GetSharedPreferences(Consts.C_MAIN_PREFERENCES, FileCreationMode.Private);

			//ACCESS_TOKEN = shared.GetString(SigninDialog.C_ACCESS_TOKEN, string.Empty);
			//USERNAME = shared.GetString(SigninDialog.C_USERNAME, string.Empty);
			//HOST_URL = shared.GetString(SigninDialog.C_HOST_URL, string.Empty);
			//HOST_URL = @"http://sbl-crm-project-pafik13.c9users.io:8080/";
			HOST_URL = "http://front-sbldev.rhcloud.com/";
			//AGENT_UUID = shared.GetString(SigninDialog.C_AGENT_UUID, string.Empty);

			RefreshView();
		}

		void UploadRealm_Click(object sender, EventArgs e)
		{
			var client = new RestClient(HOST_URL);

			var request = new RestRequest("RealmFile/upload", Method.POST);

			request.AddQueryParameter("access_token", ACCESS_TOKEN);
			request.AddQueryParameter("androidId", Helper.AndroidId);
			//request.AddFile("realm", File.ReadAllBytes(MainDatabase.DBPath), Path.GetFileName(MainDatabase.DBPath), string.Empty);

			var response = client.Execute(request);

			switch (response.StatusCode) {
				case HttpStatusCode.OK:
				case HttpStatusCode.Created:
					SDiag.Debug.WriteLine("Удалось загрузить копию базы!");
					Toast.MakeText(this, "Удалось загрузить копию базы!", ToastLength.Short).Show();
					break;
				default:
					SDiag.Debug.WriteLine("Не удалось загрузить копию базы!");
					Toast.MakeText(this, "Не удалось загрузить копию базы!", ToastLength.Short).Show();
					break;
			}
		}

		//void UploadPhoto_Click(object sender, EventArgs e)
		//{
		//	var progress = ProgressDialog.Show(this, string.Empty, @"Выгрузка фото");

		//	new Task(() => {

		//		var client = new RestClient(HOST_URL);
		//		IRestRequest request;
		//		IRestResponse response;

		//		using (var trans = MainDatabase.BeginTransaction()) {

		//			foreach (var photo in MainDatabase.GetItemsToSync<PhotoData>()) {
		//				try {
		//					//Toast.MakeText(this, string.Format(@"Загрузка фото с uuid {0} по посещению с uuid:{1}", photo.UUID, photo.Attendance), ToastLength.Short).Show();
		//					SDiag.Debug.WriteLine(string.Format(@"Загрузка фото с uuid {0} по посещению с uuid:{1}", photo.UUID, photo.Attendance));

		//					request = new RestRequest(@"PhotoData/upload", Method.POST);
		//					request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
		//					//request.AddQueryParameter(@"Stamp", photo.Stamp.ToString());
		//					request.AddQueryParameter(@"Attendance", photo.Attendance);
		//					request.AddQueryParameter(@"PhotoType", photo.PhotoType);
		//					request.AddQueryParameter(@"Brand", photo.Brand);
		//					request.AddQueryParameter(@"Latitude", photo.Latitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
		//					request.AddQueryParameter(@"Longitude", photo.Longitude.ToString(CultureInfo.CreateSpecificCulture(@"en-GB")));
		//					request.AddFile(@"photo", File.ReadAllBytes(photo.PhotoPath), Path.GetFileName(photo.PhotoPath), string.Empty);

		//					response = client.Execute(request);

		//					switch (response.StatusCode) {
		//						case HttpStatusCode.OK:
		//						case HttpStatusCode.Created:
		//							// TODO: переделать на вызов с проверкой открытой транзакции
		//							photo.IsSynced = true;
		//							if (!photo.IsManaged) MainDatabase.SavePhoto(photo);
		//							//Toast.MakeText(this, "Фото ЗАГРУЖЕНО!", ToastLength.Short).Show();
		//							SDiag.Debug.WriteLine("Фото ЗАГРУЖЕНО!");
		//							continue;
		//						default:
		//							//Toast.MakeText(this, "Не удалось загрузить фото по посещению!", ToastLength.Short).Show();
		//							SDiag.Debug.WriteLine("Не удалось загрузить фото по посещению!");
		//							continue;
		//					}
		//				} catch (Exception ex) {
		//					//Toast.MakeText(this, @"Error : " + ex.Message, ToastLength.Short).Show();
		//					SDiag.Debug.WriteLine("Error : " + ex.Message);
		//					continue;
		//				}
		//			}
		//			trans.Commit();
		//		}

		//		MainDatabase.Dispose();
		//		RunOnUiThread(() => {
		//			MainDatabase.Username = USERNAME;
		//			// Thread.Sleep(1000);
		//			progress.Dismiss();
		//			RefreshView();
		//		});
		//	}).Start();
		//}

		void RefreshView(){

			Count = 0;

			Count += DBSpec.CountItemsToSyncAll(DB);

			//Count += DBSpec.CountItemsToSync<Attendance>(DB);
			//Count += DBSpec.CountItemsToSync<Doctor>(DB);
			//Count += DBSpec.CountItemsToSync<EntityDelete>(DB);
			//Count += DBSpec.CountItemsToSync<GPSData>(DB);
			//Count += DBSpec.CountItemsToSync<HospitalInputed>(DB);
			//Count += DBSpec.CountItemsToSync<InfoData>(DB);
			//Count += DBSpec.CountItemsToSync<PotentialData>(DB);
			//Count += DBSpec.CountItemsToSync<RouteItem>(DB);
			//Count += DBSpec.CountItemsToSync<WorkPlace>(DB);

			var toSyncCount = FindViewById<TextView>(Resource.Id.saSyncEntitiesCount);
			toSyncCount.Text = string.Format("Необходимо синхронизировать {0} объектов", Count);

			var toUpdateCount = FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount);
			toUpdateCount.Text = string.Format("Необходимо обновить {0} объектов", 0);

			var photoCount = FindViewById<TextView>(Resource.Id.saSyncPhotosCount);
			//photoCount.Text = string.Format("Необходимо выгрузить {0} фото", MainDatabase.CountItemsToSync<PhotoData>());
		}

		//public bool IsTokenExpired(string token)
		//{
		//	var payloadBytes = Convert.FromBase64String(token.Split('.')[1] + "=");
		//	var payloadStr = Encoding.UTF8.GetString(payloadBytes, 0, payloadBytes.Length);

		//	// Here, I only extract the "exp" payload property. You can extract other properties if you want.
		//	var payload = JsonConvert.DeserializeAnonymousType(payloadStr, new { Exp = 0UL }); // 0UL makes implicit typing create the field as unsigned long. 

		//	var currentTimestamp = (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

		//	return currentTimestamp > payload.Exp;
		//}

		//public Account CreateSyncAccount(Context context)
		//{
		//	var newAccount = new Account(SyncConst.ACCOUNT, SyncConst.ACCOUNT_TYPE);

		//	var accountManager = (AccountManager)context.GetSystemService(AccountService);

		//	var tag = "CRMLite:SyncActivity:CreateSyncAccount";
		//	if (accountManager.AddAccountExplicitly(newAccount, null, null)) {
		//		Android.Util.Log.Info(tag, "AddAccountExplicitly");
		//	} else {
		//		Android.Util.Log.Info(tag, "NOT AddAccountExplicitly");
		//	}

		//	return newAccount;
		//}

		async void Sync_Click(object sender, EventArgs e){

			if (Count > 0) {
				var progress = ProgressDialog.Show(this, string.Empty, "Синхронизация");

				await SyncEntities<Attendance>();
				await SyncEntities<Doctor>();
				await SyncEntities<EntityDelete>();
				await SyncEntities<GPSData>();
				await SyncEntities<HospitalInputed>();
				await SyncEntities<InfoData>();
				await SyncEntities<PotentialData>();
				await SyncEntities<RouteItem>();
				await SyncEntities<WorkPlace>();

				RunOnUiThread(() =>
				{
					progress.Dismiss();
					RefreshView();
				});
			}
		}

		protected override void OnResume()
		{
			base.OnResume();

			//Helper.CheckIfTimeChangedAndShowDialog(this);
		}

		async Task SyncEntities<T>() where T : RealmObject, ISync
		{
			var client = new RestClient(HOST_URL); 
			string entityPath = typeof(T).Name;
			foreach (var item in DB.All<T>()) {
				if (item.IsSynced) continue;

				var request = new RestRequest(entityPath, Method.POST);
				request.AddQueryParameter(@"access_token", ACCESS_TOKEN);
				request.JsonSerializer = new NewtonsoftJsonSerializer();
				request.AddJsonBody(item);
				var response = await client.ExecuteTaskAsync(request);
				switch (response.StatusCode) {
					case HttpStatusCode.OK:
					case HttpStatusCode.Created:
						DB.Write(() => item.IsSynced = true);
						break;
				}
				SDiag.Debug.WriteLine(response.StatusDescription);
			}
		}

		protected override void OnPause()
		{
			base.OnPause();

			//if (CancelToken.CanBeCanceled && CancelSource != null) {
			//	CancelSource.Cancel();
			//}		
		}
	}
}

