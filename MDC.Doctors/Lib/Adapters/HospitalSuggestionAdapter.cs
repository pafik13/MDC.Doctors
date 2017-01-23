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
		public struct SearchItem
		{
			public string UUID;
			public string Key;
			public string Area;
			public string Address;
		}

		readonly Activity Context;
		readonly public IHospital[] Hospitals;
		public List<IHospital> ForDisplay;
		public SearchItem[] Searches;
		Filter HFilter;


		public HospitalSuggestionAdapter(Activity context, IList<IHospital> hospitals)
		{
			Context = context;
			Hospitals = hospitals.ToArray();
			Searches = new SearchItem[Hospitals.Count()];
			for (int i = 0; i < Hospitals.Count(); i++)
			{
				Searches[i] = new SearchItem
				{
					Key = string.Copy(Hospitals[i].GetName()),
					Area = string.Copy(Hospitals[i].GetArea()),
					Address = string.Copy(Hospitals[i].GetAddress())
				};
			}
			ForDisplay = null;
		}

		public override IHospital this[int position] {
			get {
				return ForDisplay == null ? Hospitals[position] : ForDisplay[position];
			}
		}

		public override int Count {
			get {
				return ForDisplay == null ? Hospitals.Count() : ForDisplay.Count();
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
			readonly IHospital[] Hospitals;
			
			public HospitalFilter(HospitalSuggestionAdapter adapter)
			{
				Adapter = adapter;
				Hospitals = adapter.Hospitals;
			}

			protected override FilterResults PerformFiltering(ICharSequence constraint)
			{
				var result = new FilterResults();
				if (constraint == null) return result;

				var list = new List<IHospital>();
				var search = constraint.ToString();
				for (int i = 0; i < Adapter.Searches.Count(); i++)
				{
					var item = Adapter.Searches[i];
					if (item.Key.Contains(search))
					{
						list.Add(Hospitals[i]);
					}
					else if (item.Address.Contains(search))
					{
						list.Add(Hospitals[i]);
					}
					else if (item.Area.Contains(search))
					{
						list.Add(Hospitals[i]);
					}
				}
				//for(int i = 0; i < Hospitals.Count(); i++){
				//var hospital = Hospitals[i];

				//}

				Object[] matchObjects;
				matchObjects = new Object[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					matchObjects[i] = new String(list[i].GetName());
				}

				result.Values = matchObjects;
				result.Count = matchObjects.Count();

				Adapter.SetHospitalsForDisplay(list);

				return result;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				Adapter.NotifyDataSetChanged();
				return;		
			}
		}

		public void SetHospitalsForDisplay(List<IHospital> hospitals)
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
			var item = ForDisplay == null ? Hospitals[position] : ForDisplay[position];

			var view = (convertView ?? Context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleDropDownItem1Line, parent, false)
			           ) as TextView;

			view.Text = item.GetName();

			return view;
		}
	}
}
