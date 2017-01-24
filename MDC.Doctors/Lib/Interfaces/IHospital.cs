namespace MDC.Doctors.Lib.Interfaces
{
	public interface IHospital
	{
		string GetUUID();
		
		string GetName();

		string GetAddress();

		string GetArea();

		// Phone (+2)
		string GetPhone();
	}
}

