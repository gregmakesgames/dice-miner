using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace DiceMiner.Gameplay
{
    public enum VisualMessageType
    {
        None,
        
        TileDestroyed,
    }

    public static class VisualMessageBroker
    {
        private class Subscription : IDisposable
        {
            public VisualMessageType messageType;
            public object sender;
            public System.Action unsubscribeAction;
            public Func<object, VisualMessageType, object[], UniTask> getter;

            public void Dispose()
            {
                if (unsubscribeAction != null)
                {
                    unsubscribeAction?.Invoke();
                    unsubscribeAction = null;
                }
            }
        }

        private static Dictionary<VisualMessageType, List<Subscription>> subscriptionsByType =
            new Dictionary<VisualMessageType, List<Subscription>>();

        private static List<Subscription> subscriptionsBySender = new List<Subscription>();

        public static IDisposable Subscribe(VisualMessageType messageType,
            Func<object, VisualMessageType, object[], UniTask> getter)
        {
            Subscription sub = new Subscription()
            {
                messageType = messageType,
                sender = null,
                getter = getter
            };

            if (!subscriptionsByType.ContainsKey(messageType))
                subscriptionsByType.Add(messageType, new List<Subscription>());
            subscriptionsByType[messageType].Add(sub);
            sub.unsubscribeAction = () => subscriptionsByType[messageType].Remove(sub);

            return sub;
        }

        public static IDisposable Subscribe(object sender, VisualMessageType messageType,
            Func<object, VisualMessageType, object[], UniTask> getter)
        {
            Subscription sub = new Subscription()
            {
                messageType = messageType,
                sender = sender,
                getter = getter
            };

            if (!subscriptionsByType.ContainsKey(messageType))
                subscriptionsByType.Add(messageType, new List<Subscription>());
            subscriptionsByType[messageType].Add(sub);
            sub.unsubscribeAction = () => subscriptionsByType[messageType].Remove(sub);

            return sub;
        }

        public static IDisposable Subscribe(object sender, Func<object, VisualMessageType, object[], UniTask> getter)
        {
            Subscription sub = new Subscription()
            {
                messageType = VisualMessageType.None,
                sender = sender,
                getter = getter
            };

            subscriptionsBySender.Add(sub);
            sub.unsubscribeAction = () => subscriptionsBySender.Remove(sub);

            return sub;
        }

        public static UniTask TryVisualize(object sender, VisualMessageType messageType, params object[] args)
        {
            var tasks = new List<UniTask>();
            if (subscriptionsByType.ContainsKey(messageType))
            {
                foreach (var subscription in subscriptionsByType[messageType])
                {
                    if (subscription.sender == sender || subscription.sender == null)
                    {
                        var task = subscription.getter.Invoke(sender, messageType, args);
                        tasks.Add(task);
                    }
                }
            }

            foreach (var subscription in subscriptionsBySender)
            {
                var task = subscription.getter.Invoke(sender, messageType, args);
                tasks.Add(task);
            }

            return tasks.Count == 0 ? UniTask.CompletedTask : UniTask.WhenAll(tasks);
        }

        public static void Dispose()
        {
            for (int i = subscriptionsBySender.Count - 1; i >= 0; i--)
            {
                subscriptionsBySender[i].Dispose();
            }

            foreach (var subs in subscriptionsByType)
            {
                for (int i = subs.Value.Count - 1; i >= 0; i--)
                {
                    subs.Value[i].Dispose();
                }
            }
        }
    }
}