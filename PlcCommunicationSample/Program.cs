using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.PlcBaseCommunication;
using VP.FF.PT.Common.PlcCommunication.PlcBaseCommunication.Impl;
using VP.FF.PT.Common.PlcCommunicationBeckhoff;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.CommonPlc.PlcCommunicationSample
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    class TestData
    {
        public int Test;
        public short intDataState;
    }

    class Program
    {
        private const string AdsAddress = "10.38.10.83.1.1";
        private const int AdsPort = 851;

        static void Main(string[] args)
        {
            // TagListener
            ITagListener tagListener = new BeckhoffPollingTagListener(AdsAddress, AdsPort);
            tagListener.TagChanged += TagListenerTagChanged;

            Tag tag = new Tag("In_bolHOR_2_Retracted_NO", "Io");

            tag.ValueChanged += TagValueChanged;

            tagListener.AddTag(tag);

            tagListener.RefreshAll();
            tagListener.StartListening();


            // TagController
            ITagController tagController = new BeckhoffTagController(AdsAddress, AdsPort);
            tagController.StartConnection();
            tagController.WriteTag(tag, true);


            // IDataChannelManager
            IDataChannelManager dataChannel = new DataChannelManager(tagListener, tagController);
            dataChannel.CommunicationProblemOccured += dataChannel_CommunicationProblemOccured;

            var dataChannelTag = new Tag("fbMOD_2.stDataChannelTest_DtChnToPlc", "MiddlePRG_1", "T_Data_DtChn");

            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 1 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 2 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 3 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 4 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 5 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 6 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 7 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 8 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 9 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 10 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 11 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 12 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 13 });
            dataChannel.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 14 });

            // this is optional
            dataChannel.WaitWriteComplete();



            // ControllerTreeImporter
            IControllerTreeImporter controllerTreeImporter = new BeckhoffOnlineControllerTreeImporter();
            IController rootController = controllerTreeImporter.ImportControllerTree(AdsAddress, AdsPort);

            rootController.SendParameter(new Tag("udiWaitPick_ms", string.Empty, "UDINT") { Value = 1000 });
            rootController.SendParameter("udiWaitPick_ms", 2000);
            rootController.Commands.First().Fire();

            // AlarmsImporter
            //IAlarmsImporter alarmsImporter = new BeckhoffOnlineAlarmsImporter();
            //IAlarmManager alarmManager = alarmsImporter.ImportAlarms(rootController, AdsAddress, AdsPort);


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

            while (true)
                Thread.Sleep(500);
        }

        static void dataChannel_CommunicationProblemOccured(object sender, Exception e)
        {
            Console.WriteLine(e);
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
