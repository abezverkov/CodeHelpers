using System;
using NUnit.Framework;

namespace MSMQ
{
    [TestFixture]
    public class MsmqAddress_Tests
    {
        [Test]
        public void ShouldParsePublicQueue_Path1()
        {
            // Arrage
            string path = @"ComputerName\QueueName";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual("ComputerName", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.IsFalse(addr.IsPrivate);
        }

        [Test]
        public void ShouldParsePublicQueue_Path2()
        {
            // Arrage
            string path = @".\QueueName";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual(".", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.IsFalse(addr.IsPrivate);
        }

        [Test]
        public void ShouldParsePublicQueue_Path3()
        {
            // Arrage
            string path = @".\QueueName;SubQueue";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual(".", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.AreEqual("SubQueue", addr.SubQueueName);
            Assert.IsFalse(addr.IsPrivate);
        }

        [Test]
        public void ShouldParsePrivateQueue_Path1()
        {
            // Arrage
            string path = @"ComputerName\Private$\QueueName";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual("ComputerName", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.IsTrue(addr.IsPrivate);
        }

        [Test]
        public void ShouldParsePrivateQueue_Path2()
        {
            // Arrage
            string path = @".\Private$\QueueName";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual(".", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.IsTrue(addr.IsPrivate);
        }

        [Test]
        public void ShouldParsePrivateQueue_Path3()
        {
            // Arrage
            string path = @".\Private$\QueueName;SubQueue";
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual(".", addr.ComputerName);
            Assert.AreEqual("QueueName", addr.QueueName);
            Assert.AreEqual("SubQueue", addr.SubQueueName);
            Assert.IsTrue(addr.IsPrivate);
        }

        [TestCase(@"FormatName:PRIVATE=.\QueueName")]
        [TestCase(@"FormatName:PRIVATE=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:.\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:.\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://localhost/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://127.0.0.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://localhost/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://127.0.0.1/MSMQ/QueueName")]
        public void ShouldParse_FormatDirect_IsLocal(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);

            // Assert
            Assert.True(addr.IsLocal);
        }

        [TestCase(@"FormatName:PRIVATE=.\QueueName")]
        [TestCase(@"FormatName:PRIVATE=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:.\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:.\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://127.0.0.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://127.0.0.1/MSMQ/QueueName")]
        public void ShouldParse_FormatDirect_IsLocal2(string path)
        {
            // Arrage
            var hostname = System.Net.Dns.GetHostName();
            path = path.Replace("127.0.0.1", hostname);
            path = path.Replace(".", hostname);
            var addr = new MsmqAddress(path);

            // Assert
            Assert.True(addr.IsLocal);
        }


        [TestCase(@"FormatName:PRIVATE=.\QueueName")]
        [TestCase(@"FormatName:PRIVATE=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:.\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:.\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://10.10.1.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://10.10.1.1/MSMQ/QueueName")]
        public void ShouldParse_FormatDirect_QueueName(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual("QueueName", addr.QueueName);            
        }

        [TestCase(@"FormatName:PRIVATE=.\QueueName")]
        [TestCase(@"FormatName:PRIVATE=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:.\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\private$\QueueName")]
        public void ShouldParse_FormatDirect_Private(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);

            // Assert
            Assert.True(addr.IsPrivate);
        }

        [TestCase(@"FormatName:PUBLIC=.\QueueName")]
        [TestCase(@"FormatName:PUBLIC=.\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:.\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://10.10.1.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://10.10.1.1/MSMQ/QueueName")]
        public void ShouldParse_FormatDirect_NotPrivate(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);

            // Assert
            Assert.False(addr.IsPrivate);
        }

        [TestCase(@"FormatName:PRIVATE=10.10.1.1\QueueName")]
        [TestCase(@"FormatName:PRIVATE=10.10.1.1\QueueName;SubQueue")]
        [TestCase(@"FormatName:PUBLIC=10.10.1.1\QueueName")]
        [TestCase(@"FormatName:PUBLIC=10.10.1.1\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://10.10.1.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://10.10.1.1/MSMQ/QueueName")]
        public void ShouldParse_FormatDirect_ComputerName(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);

            // Assert
            Assert.AreEqual("10.10.1.1", addr.ComputerName);
        }

        [TestCase(@"FormatName:PUBLIC=10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://10.10.1.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://10.10.1.1/MSMQ/QueueName")]
        public void ShouldFormatToPathName_Public(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@"10.10.1.1\QueueName", newPath);
        }

        [TestCase(@"FormatName:PUBLIC=127.0.0.1\QueueName")]
        [TestCase(@"FormatName:PUBLIC=localhost\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:127.0.0.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:localhost\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://127.0.0.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://127.0.0.1/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTP://localhost/MSMQ/QueueName")]
        [TestCase(@"FormatName:DIRECT=HTTPS://localhost/MSMQ/QueueName")]
        public void ShouldFormatToPathName_Public_Local(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@".\QueueName", newPath);
        }

        [TestCase(@"FormatName:PRIVATE=10.10.1.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\private$\QueueName")]
        public void ShouldFormatToPathName_Private(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@"10.10.1.1\private$\QueueName", newPath);
        }

        [TestCase(@"FormatName:PRIVATE=127.0.0.1\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:127.0.0.1\private$\QueueName")]
        [TestCase(@"FormatName:PRIVATE=localhost\QueueName")]
        [TestCase(@"FormatName:DIRECT=OS:localhost\private$\QueueName")]
        [TestCase(@"FormatName:DIRECT=TCP:127.0.0.1\private$\QueueName")]
        public void ShouldFormatToPathName_Private_Local(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@".\private$\QueueName", newPath);
        }


        [TestCase(@"FormatName:PRIVATE=10.10.1.1\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\private$\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\private$\QueueName;SubQueue")]
        public void ShouldFormatToPathName_Private_SubQueue(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@"10.10.1.1\private$\QueueName;SubQueue", newPath);
        }

        [TestCase(@"FormatName:PUBLIC=10.10.1.1\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=OS:10.10.1.1\QueueName;SubQueue")]
        [TestCase(@"FormatName:DIRECT=TCP:10.10.1.1\QueueName;SubQueue")]
        public void ShouldFormatToPathName_Public_SubQueue(string path)
        {
            // Arrage
            var addr = new MsmqAddress(path);
            var newPath = addr.ToPathName();

            // Assert
            Assert.AreEqual(@"10.10.1.1\QueueName;SubQueue", newPath);
        }
    }
}
