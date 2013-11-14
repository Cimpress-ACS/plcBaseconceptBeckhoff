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
            tagListener.RefreshRate = 200;
            tagListener.TagChanged += TagListenerTagChanged;

            var tag = new Tag("In_bolHOR_2_Retracted_NO", "Io");
            var test = new Tag("bolTest", "Test");
            test.ValueChanged += test_ValueChanged;

            tag.ValueChanged += TagValueChanged;

            tagListener.AddUdtHandler<TestData>("T_Ctrl_SIf_MOD_DtChnToPLC");
            tagListener.AddTag(tag);
            tagListener.AddTag(test);

            tagListener.RefreshAll();
            tagListener.StartListening();

            // TagController
            ITagController tagController = new BeckhoffTagController(AdsAddress, AdsPort);
            tagController.StartConnection();
            Console.WriteLine("PLC connection established = " + tagController.IsConnected);
            tagController.WriteTag(tag, true);

            // string tests
            var defaultStringTestTag = new Tag("strDefaultString", "Test", "STRING(80)");
            tagController.WriteTag(defaultStringTestTag, "123456789abcdefghijklmnopqrstuvwxyz");
            tagController.WriteTag(defaultStringTestTag, "123");
            tagController.WriteTag(defaultStringTestTag, "01234567890123456789012345678901234567890123456789012345678901234567890123456789");

            var longStringTestTag = new Tag("strLongString", "Test", "STRING(160)");
            tagController.WriteTag(longStringTestTag, "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789");

            var shortStringTestTag = new Tag("strShortString", "Test", "STRING(5)");
            tagController.WriteTag(shortStringTestTag, "12345");
            
            // ControllerTreeImporter
            IControllerTreeImporter controllerTreeImporter = new BeckhoffOnlineControllerTreeImporter();
            IController rootController = controllerTreeImporter.ImportControllerTree(AdsAddress, AdsPort);
            Console.WriteLine("imported " + rootController.Name + " controller");

            rootController.SendParameter(new Tag("udiWaitPick_ms", string.Empty, "UDINT") { Value = 1000 });
            rootController.SendParameter("udiWaitPick_ms", 2000);
            rootController.Commands.First().Fire();

            var s = (from c in rootController.Commands
                     where c.Name.Equals("Run", StringComparison.InvariantCultureIgnoreCase)
                     select c).First();

            // start root controller, otherwise the DataChannes would not work
            s.Fire();
            Console.WriteLine(s.Name + " Command fired");

            // AlarmsImporter
            IAlarmsImporter alarmsImporter = new BeckhoffOnlineAlarmsImporter(AdsAddress, AdsPort);
            var alarmManager = alarmsImporter.ImportAlarms(rootController);



            // IDataChannelListener
            IDataChannelListener<TestData> dataChannelListener = new DataChannelListener<TestData>(tagListener, tagController);
            dataChannelListener.SetChannel(new Tag("fbMOD_2.SIf.DtChnToLine", "MiddlePRG_1", "T_Ctrl_SIf_MOD_DtChnToPLC"));

            dataChannelListener.DataReceived += DataChannelListenerDataReceived;
            dataChannelListener.CommunicationProblemOccured += DataChannelListenerCommunicationProblemOccured;


            // IDataChannelWriter
            IDataChannelWriter dataChannelWriter = new DataChannelWriter(tagListener, tagController);
            dataChannelWriter.CommunicationProblemOccured += DataChannelWriterCommunicationProblemOccured;

            var dataChannelTag = new Tag("fbMOD_2.SIf.DtChnToPLC", "MiddlePRG_1", "T_Ctrl_SIf_MOD_DtChnToPLC");

            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 1 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 2 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 3 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 4 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 5 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 6 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 7 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 8 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 9 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 10 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 11 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 12 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 13 });
            dataChannelWriter.AddAsyncWriteTask(dataChannelTag, new TestData { Test = 14 });
 
            // this is optional
            //dataChannelWriter.WaitWriteComplete();
            Console.WriteLine("wrote 14 values over DataChannelManager");

            
            // TagImporter
            ITagImporter tagImporter = new BeckhoffOnlineTagImporter();
            ICollection<Tag> importedTags = tagImporter.ImportTags(AdsAddress, AdsPort);

            var listener = new BeckhoffPollingTagListener(AdsAddress, AdsPort);
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
            {
                Thread.Sleep(1000);
                Console.WriteLine("IsConnected = " + tagController.IsConnected);
            }
        }

        static void test_ValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            Console.WriteLine(sender.Value);
        }

        static void DataChannelListenerCommunicationProblemOccured(object sender, Exception e)
        {
            Console.WriteLine(e);
        }

        static void DataChannelListenerDataReceived(object sender, TestData e)
        {
            Console.WriteLine("Received data from DataChannelListener: " + e.Test);
        }

        static void DataChannelWriterCommunicationProblemOccured(object sender, Exception e)
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
