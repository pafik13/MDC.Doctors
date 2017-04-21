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

using MDC.Doctors.Lib;
using MDC.Doctors.Lib.Other;
using MDC.Doctors.Lib.Entities;
using MDC.Doctors.Lib.Interfaces;

using Amazon;
using Amazon.S3;

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

		#if DEBUG
		const string S3BucketName = "sbl-crm";
		#else
		const string S3BucketName = "sbl-crm-frankfurt";
		#endif

		readonly RegionEndpoint S3Endpoint = RegionEndpoint.USEast1;

		string DBPath = string.Empty;
		string AgentUUID = string.Empty;
		IAmazonS3 S3Client;

		public int CountItems { get; private set; }
		public int CountMaterials { get; private set; }

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

			#if DEBUG
			var loggingConfig = AWSConfigs.LoggingConfig;
			loggingConfig.LogMetrics = true;
			loggingConfig.LogResponses = ResponseLoggingOption.Always;
			loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
			loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
			#endif

			AWSConfigsS3.UseSignatureVersion4 = true;

			S3Client = new AmazonS3Client(Secret.AWSAccessKeyId, Secret.AWSSecretKey, S3Endpoint);

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


		void RefreshView(){

			CountItems = 0;

			CountItems += DBSpec.CountItemsToSyncAll(DB);

			var toSyncCount = FindViewById<TextView>(Resource.Id.saSyncEntitiesCount);
			toSyncCount.Text = string.Format("Необходимо синхронизировать {0} объектов", CountItems);
			
			CountMaterials = 0;
			foreach(var material in DBHelper.GetAll<Material>(DB)) {
				if (string.IsNullOrEmpty(material.fullPath)) {
					CountMaterials++;
					continue;
				}
				
				if (File.Exists(material.fullPath)) continue;
				
				CountMaterials++;
			}
			
			var toUpdateCount = FindViewById<TextView>(Resource.Id.saUpdateEntitiesCount);
			toUpdateCount.Text = string.Format("Необходимо обновить {0} объектов", 0);

			var photoCount = FindViewById<TextView>(Resource.Id.saSyncPhotosCount);
			//photoCount.Text = string.Format("Необходимо выгрузить {0} фото", MainDatabase.CountItemsToSync<PhotoData>());
		}

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
			
			ProgressDialog progress = null;
			if (CountItems > 0) {
				progress = ProgressDialog.Show(this, string.Empty, "Синхронизация");

				await SyncEntities<Attendance>();
				await SyncEntities<Doctor>();
				await SyncEntities<EntityDelete>();
				await SyncEntities<GPSData>();
				await SyncEntities<HospitalInputed>();
				await SyncEntities<InfoData>();
				await SyncEntities<PotentialData>();
				await SyncEntities<RouteItem>();
				await SyncEntities<WorkPlace>();

			}
			
			if (CountMaterials > 0) {
				if (progress == null) {
					progress = ProgressDialog.Show(this, string.Empty, "Обновление материалов");
				} else {
					progress.SetMessage("Обновление материалов");
				}
				
				Helper.Username = "test1";

				foreach (var material in DBHelper.GetAll<Material>(DB))
				{
					var dest = new Java.IO.File(Helper.MaterialDir, material.s3Key).ToString();
					if (!File.Exists(dest))
					{
						using (var response = await S3Client.GetObjectAsync(material.s3Bucket, material.s3Key))
						{
							using (Stream s = response.ResponseStream)
							{
								using (FileStream fs = new FileStream(dest, FileMode.Create, FileAccess.Write))
								{
									s.CopyTo(fs);
									// byte[] data = new byte[32768];
									// int bytesRead = 0;
									// do
									// {
										// bytesRead = s.Read(data, 0, data.Length);
										// fs.Write(data, 0, bytesRead);
									// }
									// while (bytesRead > 0);
									// fs.Flush();
								}
							}
							DB.Write(() =>
							{
								material.fullPath = dest;
							});
						}
					}
				}
			}
			
			if (progress == null) return;
			
			RunOnUiThread(() =>
			{
				progress.Dismiss();
				RefreshView();
			});
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

