using System;
using UnityEngine;

/// <summary>
/// Application-wide message bus. A handler is registered per (recipient, context) and invoked
/// when a message is sent on that context. Production code goes through <see cref="Default"/>;
/// tests create isolated instances.
///
/// Contract (each point was a silent failure mode of the previous implementation):
/// - one handler per (recipient, context): a duplicate registration is rejected with an error
///   in the console instead of being dropped silently,
/// - a throwing handler is logged and skipped; the remaining recipients still get the message,
/// - recipients receive messages in registration order (was: dictionary enumeration order),
/// - Send allocates nothing (was: a LINQ scan of every subscription on every call, including
///   the per-frame video time tick).
/// </summary>
public class Messenger
{
    public static Messenger Default { get; } = new Messenger();

    // One copy-on-write subscriber array per MessageContext value. Send reads the current
    // snapshot lock-free, so it is safe from any thread and re-entrant: a handler that
    // registers or unregisters during dispatch only affects the NEXT Send. Writers swap the
    // array under the lock.
    private readonly Channel[] m_Channels;
    private readonly object m_WriteLock = new object();

    public Messenger()
    {
        int contextCount = Enum.GetValues(typeof(MessageContext)).Length;
        m_Channels = new Channel[contextCount];
        for (int i = 0; i < contextCount; i++)
            m_Channels[i] = new Channel();
    }

    /// <summary>
    /// Registers a handler for messages of type T sent on the given context. One handler per
    /// (recipient, context): a duplicate registration is ignored and logged as an error.
    /// </summary>
    public void Register<T>(object recipient, Action<T> action, MessageContext context)
    {
        if (recipient == null || action == null)
        {
            Debug.LogError("Messenger: Register called with a null " + (recipient == null ? "recipient" : "handler") + " on context " + context + ", ignored.");
            return;
        }

        lock (m_WriteLock)
        {
            Channel channel = m_Channels[(int)context];
            Subscription[] items = channel.Items;
            for (int i = 0; i < items.Length; i++)
            {
                if (Equals(items[i].Recipient, recipient))
                {
                    Debug.LogError("Messenger: " + recipient.GetType().Name + " is already registered on context " + context + " - duplicate handler ignored. Unregister first if re-registering is intended.");
                    return;
                }
            }

            Subscription[] updated = new Subscription[items.Length + 1];
            Array.Copy(items, updated, items.Length);
            updated[items.Length] = new Subscription(recipient, action);
            channel.Items = updated;
        }
    }

    /// <summary>
    /// Removes the recipient's handler from the given context. No-ops if it was not registered.
    /// </summary>
    public void Unregister(object recipient, MessageContext context)
    {
        lock (m_WriteLock)
        {
            Channel channel = m_Channels[(int)context];
            Subscription[] items = channel.Items;
            for (int i = 0; i < items.Length; i++)
            {
                if (Equals(items[i].Recipient, recipient))
                {
                    Subscription[] updated = new Subscription[items.Length - 1];
                    Array.Copy(items, 0, updated, 0, i);
                    Array.Copy(items, i + 1, updated, i, items.Length - i - 1);
                    channel.Items = updated;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Sends a message to every handler registered on the context, in registration order. A
    /// throwing handler is logged and skipped without aborting dispatch to the others.
    /// </summary>
    public void Send<T>(T message, MessageContext context)
    {
        Subscription[] items = m_Channels[(int)context].Items;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].Handler is Action<T> handler)
            {
                try
                {
                    handler(message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Messenger: handler of " + items[i].Recipient.GetType().Name + " threw on context " + context + " - dispatch to the remaining recipients continues.");
                    Debug.LogException(ex);
                }
            }
            else
            {
                Debug.LogError("Messenger: " + items[i].Recipient.GetType().Name + " is registered on context " + context + " for a different message type than the " + typeof(T).Name + " being sent - handler skipped.");
            }
        }
    }

    private sealed class Channel
    {
        public volatile Subscription[] Items = Array.Empty<Subscription>();
    }

    private readonly struct Subscription
    {
        public readonly object Recipient;
        public readonly object Handler;

        public Subscription(object recipient, object handler)
        {
            Recipient = recipient;
            Handler = handler;
        }
    }
}
