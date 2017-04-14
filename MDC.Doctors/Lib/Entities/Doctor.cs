using System;

using Realms;

using MDC.Doctors.Lib.Interfaces;

namespace MDC.Doctors.Lib.Entities
{
	/// <summary>
	/// Пол.
	/// </summary>
	public enum Sex
	{
		Male, Female
	}

	/// <summary>
	/// Статус аптеки.
	/// </summary>
	public enum DoctorState
	{
		dsActive, dsReserve, dsFired, dsOnVacation
	}

	/// <summary>
	/// Врач/медработник.
	/// </summary>
	public class Doctor: RealmObject, IEntityFromClient, ISync
	{
		/// <summary>
		/// Уникальный идентификатор врача/медработника. Используется Guid.
		/// </summary>
		/// <value>The UUID.</value>
		[PrimaryKey]
		public string UUID { get; set; }

		/// <summary>
		/// Отметка: активно/резерв + комментарий/уволен. Необходимо использовать SetState и GetState.
		/// </summary>
		/// <value>The state.</value>
		public string State { get; set; }

		public void SetState(DoctorState newState) { State = newState.ToString("G"); }

		public DoctorState GetState() { return (DoctorState)Enum.Parse(typeof(DoctorState), State, true); }

		public string GetStateDesc()
		{
			switch ((DoctorState)Enum.Parse(typeof(DoctorState), State, true))
			{
				case DoctorState.dsActive:
					return "Актив";
				case DoctorState.dsReserve:
					return "Резерв";
				case DoctorState.dsFired:
					return "Уволен";
				case DoctorState.dsOnVacation:
					return "В отпуске";
				default:
					return "<UnknownDoctorState>";
			}
		}

		public static string[] GetStates()
		{
			var states = new string[4];
			states[0] = "Актив";
			states[1] = "Резерв";
			states[2] = "Уволен";
			states[3] = "В отпуске";
			return states;
		}

		/// <summary>
		/// ФИО Используется.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Пол работника аптеки. Необходимо использовать SetSex и GetSex.
		/// </summary>
		/// <value>The sex.</value>
		public string Sex { get; set; }

		public void SetSex(Sex newSex) { Sex = newSex.ToString("G"); }

		public Sex GetSex() { return (Sex)Enum.Parse(typeof(Sex), Sex, true); }

		public string GetSexDesc() { 
			switch ((Sex)Enum.Parse(typeof(Sex), Sex, true))
			{
				case Entities.Sex.Male:
					return "Мужчина";
				case Entities.Sex.Female:
					return "Женщина";
				default:
					return "<UnknownSex>";
			}
		}

		/// <summary>
		/// Специальность врача/медработника.
		/// </summary>
		/// <value>The specialty.</value>
		public string Specialty { get; set; }

		/// <summary>
		/// Специализация врача/медработника.
		/// </summary>
		/// <value>The specialism.</value>
		public string Specialism { get; set; }

		/// <summary>
		/// Должность работника аптеки.
		/// </summary>
		/// <value>The position.</value>
		public string Position { get; set; }

		/// <summary>
		/// Краткая информация о телефонах.
		/// </summary>
		/// <value>The position.</value>
		public string Phone { get; set; }

		/// <summary>
		/// Краткая информация о электронных почтах.
		/// </summary>
		/// <value>The position.</value>
		public string Email { get; set; }

		/// <summary>
		/// Возможность участия в акциях.
		/// </summary>
		/// <value>Can participate in actions?</value>
		public bool CanParticipateInActions { get; set; }

		/// <summary>
		/// Возможность участия в конференциях.
		/// </summary>
		/// <value>Can participate in conference?</value>
		public bool CanParticipateInConference { get; set; }

		/// <summary>
		/// Комментарий по сотруднику.
		/// </summary>
		/// <value>The comment.</value>
		public string Comment { get; set; }

		/// <summary>
		/// Комментарий по сотруднику.
		/// </summary>
		/// <value>The comment.</value>
		public string MainWorkPlace { get; set; }

        /// <summary>
        /// Ссылка на последний визит ко врачу. UUID класса Attendance.
        /// </summary>
        /// <value>The last attendance.</value>
        [Indexed]
        public string LastAttendance { get; set; }

        /// <summary>
        /// Дата последнего визита. Сохраняется значение Attendance.Date. Необходимо для сортировки.
        /// </summary>
        /// <value>The last attendance date.</value>
        public DateTimeOffset? LastAttendanceDate { get; set; }

        /// <summary>
        /// Дата последующего визита. Вычисляется как LastAttendanceDate + Project.DaysToNextAttendance.
        /// </summary>
        /// <value>The next attendance date.</value>
        public DateTimeOffset? NextAttendanceDate { get; set; }

		#region ISync
		/// <summary>
		/// Дата заведения сотрудника. Присваивается при сохранении.
		/// </summary>
		/// <value>The created date.</value>
		public DateTimeOffset CreatedAt { get; set; }

		/// <summary>
		/// Дата обновления сотрудника. Присваивается при сохранении.
		/// </summary>
		/// <value>The updated date.</value>
		public DateTimeOffset UpdatedAt { get; set; }

		public string CreatedBy { get; set; }

		public bool IsSynced { get; set; }

		public string DataSource { get; set; }
		#endregion
	}
}

