﻿using System;
using System.Linq;
using System.Collections.Generic;

using Realms;

using MDC.Doctors.Lib.Interfaces;
using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib
{
	public static class DBHelper
	{
		public static void GetDB(ref Realm db)
		{
			if (db == null) {
				//db = Realm.GetInstance(new RealmConfiguration()
				//{
				//	ShouldDeleteIfMigrationNeeded = true
				//});
				db = Realm.GetInstance();
				return;	
			}

			if (db.IsClosed)
			{
				db = Realm.GetInstance();
				return;
			}
		}

		public static IQueryable<T> GetAll<T>(Realm db) where T: RealmObject
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}
			return db.All<T>();
		}

        public static List<T> GetList<T>(Realm db) where T : RealmObject
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }

            return db.All<T>().ToList();
        }

		public static T Create<T>(Realm db, Transaction trans) where T : RealmObject, IEntityFromClient, ISync, new()
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}
			if (trans == null)
			{
				throw new ArgumentNullException(nameof(trans));
			}
			var item = new T();
			item.UUID = Guid.NewGuid().ToString();
			item.DataSource = DataSource.dsDoctorApp.ToString("G");
			item.CreatedBy = string.IsNullOrEmpty(Helper.AgentUUID) ? @"AgentUUID is Empty" : Helper.AgentUUID;
			item.CreatedAt = DateTimeOffset.Now;
			item.UpdatedAt = DateTimeOffset.Now;

			return item;		
		}

		public static void Delete<T>(Realm db, Transaction trans, T toDelete) where T : RealmObject, IEntityFromClient, ISync, new()
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}
			if (trans == null)
			{
				throw new ArgumentNullException(nameof(trans));
			}
			if (toDelete == null)
			{
				throw new ArgumentNullException(nameof(toDelete));
			}

			var item = new EntityDelete();
			item.UUID = toDelete.UUID;
			item.Entity = typeof(T).Name;
			item.DataSource = DataSource.dsDoctorApp.ToString("G");
			item.CreatedBy = string.IsNullOrEmpty(Helper.AgentUUID) ? @"AgentUUID is Empty" : Helper.AgentUUID;
			item.CreatedAt = DateTimeOffset.Now;
			item.UpdatedAt = DateTimeOffset.Now;
			Save(db, trans, item);

			db.Remove(toDelete);
		}

        public static void Save<T>(Realm db, Transaction trans, T item) where T : RealmObject, IEntityFromClient, ISync
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            if (trans == null)
            {
                throw new ArgumentNullException(nameof(trans));
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            db.Add(item, true);
        }

		public static void Save<T>(Realm db, Transaction trans, IList<T> items) where T : RealmObject
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}
			if (trans == null)
			{
				throw new ArgumentNullException(nameof(trans));
			}
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			foreach (var item in items)
			{
				db.Add(item, true);
			}
		}

		internal static void Save<T>(Realm db, IList<T> list) where T : RealmObject, IEntityFromServer
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}

			using (var trans = db.BeginWrite())
			{
				foreach (var item in list)
				{
					db.Add(item, true);
				}
				trans.Commit();
			}
		}

		public static IQueryable<IHospital> GetSet<T>(Realm db) where T : RealmObject, IHospital
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}

			return db.All<T>();
		}

		public static T Get<T>(Realm db, string UUID) where T : RealmObject
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}

			return db.Find<T>(UUID);
		}

		public static IHospital GetHospital(Realm db, string hospitalUUID)
		{
			var hInputed = db.All<HospitalInputed>().FirstOrDefault(hi => hi.UUID == hospitalUUID);
			if (hInputed == null)
			{
				var hChecked = db.All<HospitalChecked>().FirstOrDefault(hc => hc.uuid == hospitalUUID);
				return hChecked;
			}
			return hInputed;
		}

		public static IList<RouteItem> GetRouteItems(Realm db, DateTimeOffset selectedDate)
		{
			if (db == null)
			{
				throw new ArgumentNullException(nameof(db));
			}

			var date = selectedDate.UtcDateTime.Date;
			var list = new List<RouteItem>();
			foreach (var routeItem in db.All<RouteItem>())
			{
				if (routeItem.Date.Date == date)
				{
					list.Add(routeItem);
				}
			}

			return list;		
		}
	}
}

