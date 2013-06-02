using System.Threading;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.PlcBaseCommunication;
using VP.FF.PT.Common.PlcCommunicationBeckhoff;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.CommonPlc.PlcCommunicationSample
{
    class Program
    {
        private const string adsAddress = "192.168.10.113.1.1";
        private const int adsPort = 851;

        static void Main(string[] args)
        {
            // TagListener
            ITagListener tagListener = new BeckhoffPollingTagListener(adsAddress, adsPort);
            tagListener.TagChanged += TagListenerTagChanged;

            Tag tag = new Tag("In_bolCyl_AB_Retracted_NO", "Io");

            tag.ValueChanged += TagValueChanged;

            tagListener.AddTag(tag);

            tagListener.RefreshAll();
            tagListener.StartListening();



            // TagController
            ITagController tagController = new BeckhoffTagController(adsAddress, adsPort);
            tagController.StartConnection();
            tagController.WriteTag(tag, true);



            // TagImporter
            //ITagImporter tagImporter = new BeckhoffOnlineTagImporter();
            //ICollection<Tag> importedTags = tagImporter.ImportTags(adsAddress, adsPort);


            // ControllerTreeImporter
            IControllerTreeImporter controllerTreeImporter = new BeckhoffOnlineControllerTreeImporter(new BeckhoffOnlineTagImporter());
            Controller rootController = controllerTreeImporter.ImportControllerTree(adsAddress, adsPort);


            while (true)
                Thread.Sleep(500);
        }

        static void TagListenerTagChanged(object sender, TagChangedEventArgs e)
        {

        }

        static void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {

        }



    }
}
