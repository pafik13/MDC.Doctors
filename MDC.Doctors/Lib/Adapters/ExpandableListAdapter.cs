using System;
using Android.Widget;
using System.Collections.Generic;
using Android.App;
using Android.Views;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors
{
	public class ExpandableListAdapter : BaseExpandableListAdapter
	{
		readonly Activity _context;
		readonly List<string> _listDataHeader; // header titles
									  // child data in format of header title, child title
		readonly Dictionary<string, List<string>> _listDataChild;
		readonly string EmptyUUID;
		readonly Dictionary<string, List<DoctorInfoHolder>> Source;

		public ExpandableListAdapter(Activity context, List<string> listDataHeader, Dictionary<string, List<string>> listChildData,
		                             Dictionary<string, List<DoctorInfoHolder>> source = null)
		{
			_context = context;
			_listDataHeader = listDataHeader;
			_listDataChild = listChildData;
			EmptyUUID = Guid.Empty.ToString();
			Source = source;
		}
		//for cchild item view
		public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
		{
			return _listDataChild[_listDataHeader[groupPosition]][childPosition];
		}
		public override long GetChildId(int groupPosition, int childPosition)
		{
			return childPosition;
		}

		public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		{
			string childText = (string)GetChild(groupPosition, childPosition);

			convertView = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorChild, null);

			TextView txtListChild = (TextView)convertView.FindViewById(Resource.Id.rdcDoctorNameTV);
			txtListChild.Text = childText;
			return convertView;
		}
		public override int GetChildrenCount(int groupPosition)
		{
			return _listDataChild[_listDataHeader[groupPosition]].Count;
		}
		//For header view
		public override Java.Lang.Object GetGroup(int groupPosition)
		{
			return _listDataHeader[groupPosition];
		}
		public override int GroupCount
		{
			get
			{
				return _listDataHeader.Count;
			}
		}
		public override long GetGroupId(int groupPosition)
		{
			return groupPosition;
		}
		public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		{
			string hospitalUUID = (string)GetGroup(groupPosition);

			convertView = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.RouteDoctorGroup, null);
			var hospitalName = (TextView)convertView.FindViewById(Resource.Id.rdgHospitalName);
			var hospitalAddress = (TextView)convertView.FindViewById(Resource.Id.rdgHospitalAddress);

			if (hospitalUUID == EmptyUUID)
			{
				hospitalName.Text = "Без места работы";
				hospitalAddress.Text = string.Empty;
			}
			else
			{
				var hospital = Source[hospitalUUID][0].Hospital;
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