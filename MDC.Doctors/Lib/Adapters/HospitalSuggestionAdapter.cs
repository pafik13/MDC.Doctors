using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;
using MDC.Doctors.Lib.Interfaces;
using Java.Lang;
using Realms;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Adapters
{
	public class HospitalSuggestionAdapter: BaseAdapter<IHospital>, IFilterable
	{
		public struct IHospitalHolder
		{
			public string UUID;
			public string Name;
			public string Area;
			public string Address;
		}

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
			IHospitals = new IHospitals[inputedHospitals.Count() + checkedHospitals.Count()];
			
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

		public override IHospital this[int position] {
			get {
				return ForDisplay == null ? IHospitals[position] : ForDisplay[position];
			}
		}

		public override int Count {
			get {
				return ForDisplay == null ? IHospitals.Count() : ForDisplay.Count();
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

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var results = new FilterResults();
				if (constraint == null) return result;

				var list = new List<IHospitalHolder>();
				var search = constraint.ToString();
				for (int i = 0; i < IHospitals.Count(); i++)
				{
					var item = IHospitals[i];
					if (item.Name.Contains(search))
					{
						list.Add(item);
					}
					else if (item.Address.Contains(search))
					{
						list.Add(item);
					}
					else if (item.Area.Contains(search))
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
				results.Count = matchObjects.Count();

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
