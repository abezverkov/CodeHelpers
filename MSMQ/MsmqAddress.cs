using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MSMQ
{
    public class MsmqAddress
    {
        public enum MsmqFormatType
        {
            Public,
            Private,
            DistributionList,
            Multicast,
            Machine,
            Direct,
        }

        public enum MsmqDirectFormatType
        {
            Tcp,
            OS,
            Http,
            Https,
        }

        public MsmqDirectFormatType DirectFormatType { get; private set; }
        public bool IsPrivate { get; private set; }
        public bool IsLocal { get; private set; }
        public string QueueName { get; private set; }
        public string SubQueueName { get; private set; }
        public string ComputerName { get; private set; }

        public MsmqAddress(string path)
        {
            this.Parse(path);
        }

        public string ToPathName()
        {
            return IsPrivate ?
                string.Format(@"{0}\private$\{1}", IsLocal ? "." : ComputerName, BuildQueueName()) :
                string.Format(@"{0}\{1}", IsLocal ? "." : ComputerName, BuildQueueName());
        }

        private const string FormatName = "formatname:";
        private const string Public = "public";
        private const string Private = "private";
        private const string DistributionList = "dl";
        private const string Multicast = "multicast";
        private const string Machine = "machine";
        private const string Direct = "direct";

        private string BuildQueueName()
        {
            if (string.IsNullOrWhiteSpace(SubQueueName))
                return QueueName;
            else
                return QueueName + ";" + SubQueueName;
        }

        private bool ParseIsLocal()
        {
            var locals = new string[]{ ".", "localhost", "127.0.0.1","::1" };
            if (locals.Contains(ComputerName)) return true;

            IPAddress ip;
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            if (IPAddress.TryParse(ComputerName, out ip))
            {
                if (localIPs.Contains(ip)) return true;
            }
            else
            {
                try
                {
                    IPAddress[] hostIPs = Dns.GetHostAddresses(ComputerName);
                    if (hostIPs.Intersect(localIPs).Count() > 0) return true;
                }
                catch { }
            }
            return false;
        }

        private void Parse(string path)
        {
            // trim format name
            if (path.StartsWith(FormatName, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(FormatName.Length);
            }
            if (path.Split(',').Length > 1)
                throw new NotSupportedException("Multiple-Element Format Name Syntax not supported.");

            var parts = path.Split('=');
            if (parts.Length > 1)
            {
                var syntax = parts[0].ToLower();
                var formatPath = parts[1];

                // Format name
                switch (syntax)
                {
                    case Public:
                        {
                            var q = formatPath.Split('\\');
                            this.ComputerName = q[0];
                            ParseSubQueue(q[1]);
                            break;
                        }
                    case Private:
                        {
                            this.IsPrivate = true;
                            ParsePrivateFormat(formatPath);
                            break;
                        }
                    case Direct:
                        {
                            ParseDirectFormat(formatPath);
                            break;
                        }
                    case DistributionList:
                    case Multicast:
                    case Machine:
                    default:
                        throw new NotSupportedException("Unspported Format Name Syntax");
                }
            }
            else
            {
                // Path name
                ParsePrivateFormat(path);
            }
            IsLocal = ParseIsLocal();
        }

        private void ParsePrivateFormat(string formatPath)
        {
            var q = formatPath.Split('\\').ToList();
            ParsePrivate(q);
            if (q.Count < 2)
                throw new InvalidOperationException("Invalid Private Format Name Syntax");
            this.ComputerName = q[0];

            ParseSubQueue(q[1]);
        }

        private void ParseDirectFormat(string formatPath)
        {
            /*
            DIRECT=AddressSpecification\QueueName (For public queues)
            DIRECT=AddressSpecification\PRIVATE$\QueueName (For private queues)
            DIRECT=AddressSpecification\QueueName;JOURNAL (For public queue journals)
            DIRECT=AddressSpecification\PRIVATE$\QueueName;JOURNAL (For private queue journals)
            DIRECT=AddressSpecification\SYSTEM$;computersystemqueue (For computer journal and dead-letter queues.)
            DIRECT=URLAddressSpecification/QueueName
            */

            if (formatPath.Contains("/"))
            {
                // url spec: DIRECT=HTTP://157.34.104.22/MSMQ/MyQueue
                var q = formatPath.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (q.Count != 4)
                    throw new InvalidOperationException("Invalid Direct Format Name Syntax");

                if (StringComparer.OrdinalIgnoreCase.Compare(q[0], "http:") == 0)
                {
                    this.DirectFormatType = MsmqDirectFormatType.Http;
                }
                else if (StringComparer.OrdinalIgnoreCase.Compare(q[0], "https:") == 0)
                {
                    this.DirectFormatType = MsmqDirectFormatType.Https;
                }
                else
                    throw new InvalidOperationException("Invalid Direct Format Name Syntax");

                this.ComputerName = q[1];
                this.ParseSubQueue(q[3]);
            }
            else
            {
                // DIRECT=OS:MyServer\MyQueue
                // DIRECT=TCP:157.34.104.22\MyQueue

                var q = formatPath.Split('\\').ToList();
                ParsePrivate(q);


                if (q.Count < 2)
                    throw new InvalidOperationException("Invalid Direct Format Name Syntax");

                var addressSpec = q[0];
                var queuePath = q[1];

                if (addressSpec.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
                {
                    this.DirectFormatType = MsmqDirectFormatType.Tcp;
                    addressSpec = addressSpec.Substring(4);
                }
                if (addressSpec.StartsWith("os:", StringComparison.OrdinalIgnoreCase))
                {
                    this.DirectFormatType = MsmqDirectFormatType.OS;
                    addressSpec = addressSpec.Substring(3);
                }
                this.ComputerName = addressSpec;

                ParseSubQueue(queuePath);
            }
        }

        private void ParsePrivate(IList<string> qParts)
        {
            // Remove Private$ if it exists
            var p = qParts.SingleOrDefault(x => string.Equals(x, "private$", StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                this.IsPrivate = true;
                qParts.Remove(p);
            }
        }

        private void ParseSubQueue(string queuePath)
        {
            var queuePaths = queuePath.Split(';');
            this.QueueName = queuePaths[0];
            this.SubQueueName = queuePaths.Length > 1 ? queuePaths[1] : null;
        }
    }
}
