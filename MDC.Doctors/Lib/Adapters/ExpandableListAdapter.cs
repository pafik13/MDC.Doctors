using System;
using Android.Widget;
using System.Collections.Generic;
using Android.App;
using Android.Views;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Adapters
{
	public class ExpandableListAdapter : BaseExpandableListAdapter
	{
		readonly Activity Context;
		readonly List<IHospital> Headers;
		readonly Dictionary<string, List<DoctorInfoHolder>> Childs;
		readonly string EmptyUUID;
		readonly Dictionary<string, List<DoctorInfoHolder>> Excluded;

		public ExpandableListAdapter(Activity context, List<IHospital> headers, Dictionary<string, List<DoctorInfoHolder>> childs, List<RouteItem> routeItems = null)
		{
			Context = context;
			Headers = headers;
			Childs = childs;
			EmptyUUID = Guid.Empty.ToString();
			Excluded = new Dictionary<string, List<DoctorInfoHolder>>();
			if (routeItems == null) return;
			
			foreach (var ri in routeItems) {
				var holders = Childs[ri.Hospital];
				foreach (var holder in ri.Hospital) {
					if (holder.Doctor.MainWorkPlace == ri.WorkPlace) {
						if (holders.Remove(holder)) {
							Excluded.Add(holder.Doctor.UUID, holder);
						}
					}

				}
			}
		}
		
		public DoctorInfoHolder ExcludeDoctor (int groupPosition, int childPosition)
		{
			var holders = Childs[Headers[groupPosition].GetUUID()];
			var holder = holders[childPosition];
			if (holders.Remove(holder)) {
				holder.Hospital = Headers[groupPosition];
				Excluded.Add(holder.Doctor.UUID, holder);
				NotifyDataSetChanged();
				return holder;
			}
			return null;
		}
		
		public void IncludeDoctor (string doctorUUID)
		{
			if (Excluded.ContainsKey(doctorUUID)){
				var holder = Excluded[doctorUUID];
				if (Excluded.Remove(doctorUUID)) {
					if (holder.Doctor.Hospital == null) {
						Childs[EmptyUUID].Add(holder);
					} else {
						Childs[holder.Doctor.Hospital].Add(holder);
					}
					NotifyDataSetChanged();
				}
			}
		}
		
		//for child item view
		public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
		{
			return Childs[Headers[groupPosition].GetUUID()][childPosition];
		}
		public override long GetChildId(int groupPosition, int childPosition)
		{
			return childPosition;
		}
		
		public class ChildHolder: Java.Lang.Object
		{
			public TextView DoctorName;
			public TextView DoctorCategoryText;
			public TextView DoctorLastAttendanceDate;
			public TextView CabinetAndTimetable;
		}
		
		public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		{
			var childInfo = (DoctorInfoHolder)GetChild(groupPosition, childPosition);
			
			ChildHolder holder;
			
			if (convertView == null) {
				convertView = Context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorChild, null);
				holder = new ChildHolder
				{
					DoctorName = convertView.FindViewById<TextView>(Resource.Id.rdcDoctorNameTV),
					DoctorCategoryText = convertView.FindViewById<TextView>(Resource.Id.rdcDoctorCategoryTextTV),
					DoctorLastAttendanceDate = convertView.FindViewById<TextView>(Resource.Id.rdcDoctorLastAttendanceDateTV),
					CabinetAndTimetable = convertView.FindViewById<TextView>(Resource.Id.rdcCabinetAndTimetableTV)
				};
				convertView.SetTag(Resource.String.ViewHolder, holder);
			} else {
				holder = convertView.GetTag(Resource.String.ViewHolder) as ChildHolder;
			}			
			
			holder.DoctorName.Text = childInfo.Doctor.Name;
			holder.DoctorCategoryText.Text = childInfo.Doctor.CategoryText;
			if (childInfo.Doctor.LastAttendanceDate.HasValue()) {
				holder.DoctorLastAttendanceDate.Text = childInfo.Doctor.LastAttendanceDate.ToString();
			}
			
			if (childInfo.WorkPlace != null) {
				var text = string.Concat(
					"Кабинет:", Environment.NewLine, childInfo.WorkPlace.Cabinet,
					Environment.NewLine,
					"Время работы:", Environment.NewLine, childInfo.WorkPlace.Timetable,
				);
				
				holder.CabinetAndTimetable.Text = text;
			}
			
			return convertView;
		}
		public override int GetChildrenCount(int groupPosition)
		{
			return Childs[Headers[groupPosition].GetUUID()].Count;
		}
		//For header view
		public override Java.Lang.Object GetGroup(int groupPosition)
		{
			return Headers[groupPosition].GetUUID();
		}
		public override int GroupCount
		{
			get
			{
				return Headers.Count;
			}
		}
		public override long GetGroupId(int groupPosition)
		{
			return groupPosition;
		}
		
		public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		{
			var hospital = Headers[groupPosition];

			convertView = convertView ?? Context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorGroup, null);
			var hospitalName = (TextView)convertView.FindViewById(Resource.Id.rdgHospitalName);
			var hospitalAddress = (TextView)convertView.FindViewById(Resource.Id.rdgHospitalAddress);

			if (hospital == null)
			{
				hospitalName.Text = "Без места работы";
				hospitalAddress.Text = string.Empty;
			}
			else
			{
				hospitalName.Text = hospital.GetName();
				hospitalAddress.Text = hospital.GetAddress();				
			}

			return convertView;
		}
		public override bool HasStableIds
		{
			get
			{
				return false;
			}
		}
		public override bool IsChildSelectable(int groupPosition, int childPosition)
		{
			return true;
		}

		class ViewHolderItem : Java.Lang.Object
		{
		}
	}
}