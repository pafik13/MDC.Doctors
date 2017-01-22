using MDC.Doctors.Lib.Entities;

namespace MDC.Doctors.Lib.Interfaces
{
	public interface IAttendanceControl
	{
		// Добавить в начало строки и снять экран блокировки
		void OnAttendanceStart(Attendance newAttendance);

		// Разблокировать строки для ввода данных
		void OnAttendanceResume(Attendance oldAttendance);

		// Заблокировать строки от ввода данных
		void OnAttendancePause();

		// Сохранить данные
		void OnAttendanceStop();
	}
}

