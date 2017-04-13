using System;
using System.Messaging;

namespace MSMQ
{
    public static class MsmqAddressExtensions
    {
        public static bool IsAvailable(this MsmqAddress mqAddr)
        {
            var queue = new MessageQueue(mqAddr.OriginalPath);
            try
            {
                queue.Peek(TimeSpan.FromMilliseconds(5));
                return true;
            }
            catch (MessageQueueException mqex)
            {
                return mqex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool CanCreate(this MsmqAddress mqAddr)
        {
            return mqAddr.IsLocal && mqAddr.IsAvailable();
        }

        public static MsmqAddress EnsureCreated(this MsmqAddress mqAddr, bool transactional = false)
        {
            if (!mqAddr.IsLocal)
                throw new InvalidOperationException("Cannot create remote queue.");
            var path = mqAddr.ToPathName();
            if (!MessageQueue.Exists(path))
                MessageQueue.Create(path, transactional);

            return mqAddr;
        }

        public static  MessageQueue GetQueue(this MsmqAddress mqAddr, bool sharedModeDenyReceive = false, bool enableCache = false, QueueAccessMode accessMode = QueueAccessMode.Send)
        {
            if (!mqAddr.IsLocal)
                throw new InvalidOperationException("Queue is not available.");

            return new MessageQueue(mqAddr.OriginalPath, sharedModeDenyReceive, enableCache, accessMode);
        }
    }
}
