using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.PlcBaseCommunication;
using VP.FF.PT.Common.PlcCommunicationBeckhoff;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.CommonPlc.PlcCommunicationSample
{
    class Program
    {
        private const string AdsAddress = "10.38.60.148.1.1";
        private const int AdsPort = 851;

        static void Main(string[] args)
        {
            // ControllerTreeImporter
            IControllerTreeImporter controllerTreeImporter = new BeckhoffOnlineControllerTreeImporter();
            IController rootController = controllerTreeImporter.ImportControllerTree(AdsAddress, AdsPort);

            rootController.SendParameter(new Tag("udiWaitPick_ms", string.Empty, "UDINT") { Value = 1000 });
            rootController.SendParameter("udiWaitPick_ms", 2000);
            rootController.Commands.First().Fire();

            // AlarmsImporter
            IAlarmsImporter alarmsImporter = new BeckhoffOnlineAlarmsImporter();
            IAlarmManager alarmManager = alarmsImporter.ImportAlarms(rootController, AdsAddress, AdsPort);


            // TagImporter
            ITagImporter tagImporter = new BeckhoffOnlineTagImporter();
            ICollection<Tag> importedTags = tagImporter.ImportTags(AdsAddress, AdsPort);

            var listener = new BeckhoffTagListener(AdsAddress, AdsPort);
            foreach (var importedTag in importedTags)
            {
                listener.AddTagsRecursively(importedTag);
            }
            listener.RefreshAll();


            foreach (var importedTag in importedTags)
            {
                CoutTags(importedTag);
            }


            // TagListener
            ITagListener tagListener = new BeckhoffPollingTagListener(AdsAddress, AdsPort);
            tagListener.TagChanged += TagListenerTagChanged;

            Tag tag = new Tag("In_bolCyl_AB_Retracted_NO", "Io");

            tag.ValueChanged += TagValueChanged;

            tagListener.AddTag(tag);

            tagListener.RefreshAll();
            tagListener.StartListening();



            // TagController
            ITagController tagController = new BeckhoffTagController(AdsAddress, AdsPort);
            tagController.StartConnection();
            tagController.WriteTag(tag, true);



            while (true)
                Thread.Sleep(500);
        }

        static void CoutTags(Tag tag)
        {
            foreach (var child in tag.Childs)
            {
                Console.WriteLine(child.FullName() + " = " + child.Value);
                CoutTags(child);
            }
        }

        static void TagListenerTagChanged(object sender, TagChangedEventArgs e)
        {

        }

        static void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {

        }



    }
}
