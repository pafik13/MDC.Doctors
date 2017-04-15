using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using Java.Lang;

using Realms;

using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Adapters
{
	public struct IHospitalHolder
	{
		public string UUID;
		public string Name;
		public string Area;
		public string Address;
	}

	public class HospitalSuggestionAdapter: BaseAdapter<IHospitalHolder>, IFilterable
	{
		readonly Activity Context;
		readonly public IHospitalHolder[] IHospitals;
		public List<IHospitalHolder> ForDisplay;
		Filter HFilter;

		public HospitalSuggestionAdapter(Activity context)
		{
			Context = context;
			var DB = Realm.GetInstance();
			var inputedHospitals = DBHelper.GetAll<HospitalInputed>(DB);
			var checkedHospitals = DBHelper.GetAll<HospitalChecked>(DB);
			IHospitals = new IHospitalHolder[inputedHospitals.Count() + checkedHospitals.Count()];
			
			int i = 0;
			
			foreach(var hospital in inputedHospitals)
			{
				IHospitals[i] = new IHospitalHolder
				{
					UUID = string.Copy(hospital.GetUUID()),
					Name = string.Copy(hospital.GetName()),
					Area = string.Copy(hospital.GetArea()),
					Address = string.Copy(hospital.GetAddress())
				};
				
				i++;
			}
			
			foreach(var hospital in checkedHospitals)
			{
				IHospitals[i] = new IHospitalHolder
				{
					UUID = string.Copy(hospital.GetUUID()),
					Name = string.Copy(hospital.GetName()),
					Area = string.Copy(hospital.GetArea()),
					Address = string.Copy(hospital.GetAddress())
				};
				
				i++;				
			}
			
			ForDisplay = null;
		}

		public override IHospitalHolder this[int position] {
			get {
				return ForDisplay == null ? IHospitals[position] : ForDisplay[position];
			}
		}

		public override int Count {
			get {
				return ForDisplay == null ? IHospitals.Length : ForDisplay.Count;
			}
		}

		public Filter Filter {
			get {
				if (HFilter == null) {
					HFilter = new HospitalFilter(this);
				}
				return HFilter;
			}
		}

		public class HospitalFilter : Filter
		{
			readonly HospitalSuggestionAdapter Adapter;
			readonly IHospitalHolder[] IHospitals;
			
			public HospitalFilter(HospitalSuggestionAdapter adapter)
			{
				Adapter = adapter;
				IHospitals = adapter.IHospitals;
			}

			public override ICharSequence ConvertResultToStringFormatted(Object resultValue)
			{
				var piInstance = resultValue.GetType().GetProperty("Instance");
				var instance  = piInstance == null ? null : piInstance.GetValue(resultValue, null);

				if (instance == null) return base.ConvertResultToStringFormatted(resultValue);

				var piName = instance.GetType().GetField("Name");
				var name = piName == null ? null : piName.GetValue(instance);

				if (name == null) return base.ConvertResultToStringFormatted(resultValue);

				return new String(name.ToString());
			}

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var results = new FilterResults();
				if (constraint == null) return results;

				var list = new List<IHospitalHolder>();
				var search = constraint.ToString();
				for (int i = 0; i < IHospitals.Length; i++)
				{
					var item = IHospitals[i];
					if (item.Name.Contains(search, System.StringComparison.CurrentCultureIgnoreCase))
					{
						list.Add(item);
					}
					else if (item.Address.Contains(search, System.StringComparison.CurrentCultureIgnoreCase))
					{
						list.Add(item);
					}
					else if (item.Area.Contains(search, System.StringComparison.CurrentCultureIgnoreCase))
					{
						list.Add(item);
					}
				}

				Object[] matchObjects;
				matchObjects = new Object[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					matchObjects[i] = new String(list[i].Name);
				}

				results.Values = matchObjects;
				results.Count = list.Count;

				Adapter.SetHospitalsForDisplay(list);

				return results;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				if (results.Count > 0) {
					Adapter.NotifyDataSetChanged();
                } else {
                    Adapter.NotifyDataSetInvalidated();
                }
				return;		
			}
		}

		public void SetHospitalsForDisplay(List<IHospitalHolder> hospitals)
		{
			Context.RunOnUiThread(() =>
			{
				ForDisplay = hospitals.Count == 0 ? null : hospitals;
			});
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			// Get our object for positio
			var item = ForDisplay == null ? IHospitals[position] : ForDisplay[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleDropDownItem1Line, parent, false)
			           ) as TextView;

			view.Text = item.Name;

			return view;
		}
	}
}
