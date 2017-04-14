using System;

namespace MDC.Doctors.Lib.Interfaces
{
	public enum DataSource
	{
		dsPharmacyApp, dsDoctorApp	
	}

	public interface ISync
	{
		/// <summary>
		/// Gets or sets the data type = ['pharmacy', 'doctor']
		/// </summary>
		/// <value>The data type.</value>
		string DataSource { get; set; }

		/// <summary>
		/// Gets or sets the created by. Link to Agent UUID
		/// </summary>
		/// <value>The created by.</value>
		string CreatedBy { get; set; } 

		/// <summary>
		/// Gets or sets the created at.
		/// </summary>
		/// <value>The created at.</value>
		DateTimeOffset CreatedAt { get; set; }

		/// <summary>
		/// Gets or sets the updated at.
		/// </summary>
		/// <value>The updated at.</value>
		DateTimeOffset UpdatedAt { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:MDC.Doctors.Lib.Interfaces.ISync"/> is synced.
		/// </summary>
		/// <value><c>true</c> if is synced; otherwise, <c>false</c>.</value>
		bool IsSynced { get; set; }
	}
}

