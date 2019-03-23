// Names:   Jordan Travaux & Abel Emun
// Date:    March 18, 2019
// Purpose: Self-hosted service for the Concentration game

using System;
using System.ServiceModel;
using ConcentrationLibrary;

namespace ConcentrationService
{
    class ConcentrationService
    {
        [STAThread]
        static void Main(string[] args) {
            ServiceHost servHost = null;
            try {
                servHost = new ServiceHost(typeof(Concentration));
                servHost.Open();
                Console.WriteLine("Service started. Press a key to quit.");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            finally {
                Console.ReadKey();
                if (servHost != null)
                    servHost.Close();
            }
        }
    }
}
