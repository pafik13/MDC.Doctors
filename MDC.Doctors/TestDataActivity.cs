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
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

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

		#if DEBUG
		const string S3BucketName = "sbl-crm";
		#else
		const string S3BucketName = "sbl-crm-frankfurt";
		#endif

		readonly RegionEndpoint S3Endpoint = RegionEndpoint.USEast1;

		string DBPath = string.Empty;
		string AgentUUID = string.Empty;
		IAmazonS3 S3Client;

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

			#if DEBUG
			var loggingConfig = AWSConfigs.LoggingConfig;
			loggingConfig.LogMetrics = true;
			loggingConfig.LogResponses = ResponseLoggingOption.Always;
			loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
			loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
			#endif

			AWSConfigsS3.UseSignatureVersion4 = true;

			S3Client = new AmazonS3Client(Secret.AWSAccessKeyId, Secret.AWSSecretKey, S3Endpoint);
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
			Helper.Username = "test1";

			var materials = DBHelper.GetAll<Material>(DB);

			foreach (var material in materials)
			{
				//if (string.IsNullOrEmpty(material.fullPath) || !File.Exists(material.fullPath))
				{
					var dest = new Java.IO.File(Helper.MaterialDir, material.s3Key).ToString();
					var objectRequest = new GetObjectRequest
					{
						BucketName = material.s3Bucket,
						Key = material.s3Key
					};

					using (var response = S3Client.GetObjectAsync(objectRequest).Result)
					{
						//if (!File.Exists(dest))
						{
							//response.WriteResponseStreamToFileAsync(dest, true);
							using (Stream s = response.ResponseStream)
							{
								using (FileStream fs = new FileStream(dest, FileMode.Create, FileAccess.Write))
								{
									byte[] data = new byte[32768];
									int bytesRead = 0;
									do
									{
										bytesRead = s.Read(data, 0, data.Length);
										fs.Write(data, 0, bytesRead);
									}
									while (bytesRead > 0);
									fs.Flush();


								}
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
			var newCats = new Category[4];
			if (cats.Count() == 0)
			{
				for (int i = 1; i <= 4; i++)
				{
					newCats[i-1] = new Category
					{
						uuid = Guid.NewGuid().ToString(),
						name = i + "-ая кат.",
						order = i
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
				var newRules = new CategoryRule[brands.Count()];

				for (int b = 0; b < brands.Count; b++)
				{
					newRules[b] = new CategoryRule
					{
						uuid = Guid.NewGuid().ToString(),
						desc = string.Format("Правило для препарата {0}", brands[b].name),
						brand = brands[b].uuid,
						potential = 10 - b,
						proportion = 0.5f
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

