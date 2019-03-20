using System;
using System.ServiceModel;
using ConcentrationLibrary;

namespace ConcentrationService
{
    class ConcentrationService
    {
        [STAThread]
        static void Main(string[] args)
        {
            ServiceHost servHost = null;
            try
            {
                // Address
                servHost = new ServiceHost(typeof(Concentration));
                servHost.Open();
                
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
