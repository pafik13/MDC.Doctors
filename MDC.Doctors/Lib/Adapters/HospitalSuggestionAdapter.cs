using System.Linq;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;
using MDC.Doctors.Lib.Interfaces;
using Java.Lang;

namespace MDC.Doctors.Adapters
{
	public class HospitalSuggestionAdapter: BaseAdapter<IHospital>, IFilterable
	{
		readonly Activity Context;
		readonly public IHospital[] Hospitals;
		public IHospital[] ForDisplay;
		Filter HFilter;

		public HospitalSuggestionAdapter(Activity context, IList<IHospital> hospitals)
		{
			Context = context;
			Hospitals = hospitals.ToArray();
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
				var list = new List<IHospital>();
				var search = constraint.ToString();
				for(int i = 0; i < Hospitals.Count(); i++){
					var hospital = Hospitals[i];
					if (hospital.GetName().Contains(search)){
						list.Add(Hospitals[i]);
					} else if (hospital.GetAddress().Contains(search)){
						list.Add(Hospitals[i]);
					} else if (hospital.GetArea().Contains(search)){
						list.Add(Hospitals[i]);
					}
				}

				Object[] matchObjects;
				matchObjects = new Object[list.Count];
				for (int i = 0; i < list.Count; i++)
				{
					matchObjects[i] = new String(list[i].GetName());
				}

				result.Values = matchObjects;
				result.Count = matchObjects.Count();

				Adapter.SetHospitalsForDisplay(list.ToArray());

				return result;
			}

			protected override void PublishResults(ICharSequence constraint, FilterResults results)
			{
				Adapter.NotifyDataSetChanged();
				return;		
			}
		}

		public void SetHospitalsForDisplay(IHospital[] hospitals)
		{
			ForDisplay = hospitals;
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
