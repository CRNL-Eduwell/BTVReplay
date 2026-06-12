using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for the message bus contract: one handler per (recipient, context) rejected
/// loudly on duplicates, per-handler exception isolation (one throwing handler must not abort
/// dispatch to the rest), registration-order delivery, and snapshot semantics (registering or
/// unregistering during dispatch only affects the next Send). Every test uses an isolated
/// Messenger instance - Messenger.Default is shared with the live application.
/// </summary>
public class MessengerTests
{
    private class TestMessage
    {
        public int Value;
    }

    private class OtherMessage
    {
    }

    private Messenger m_Messenger;

    [SetUp]
    public void SetUp()
    {
        m_Messenger = new Messenger();
    }

    [Test]
    public void Send_DeliversToTheHandlerRegisteredOnTheSameContext()
    {
        TestMessage received = null;
        m_Messenger.Register<TestMessage>(this, m => received = m, MessageContext.UiToTrace);

        TestMessage sent = new TestMessage { Value = 42 };
        m_Messenger.Send(sent, MessageContext.UiToTrace);

        Assert.AreSame(sent, received);
    }

    [Test]
    public void Send_DoesNotDeliverToHandlersOnOtherContexts()
    {
        int calls = 0;
        m_Messenger.Register<TestMessage>(this, m => calls++, MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToVideo);

        Assert.AreEqual(0, calls);
    }

    [Test]
    public void Send_DeliversToAllRecipientsInRegistrationOrder()
    {
        List<string> order = new List<string>();
        object recipientA = new object(), recipientB = new object(), recipientC = new object();
        m_Messenger.Register<TestMessage>(recipientA, m => order.Add("A"), MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientB, m => order.Add("B"), MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientC, m => order.Add("C"), MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);

        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, order);
    }

    [Test]
    public void Unregister_StopsDelivery_AndLeavesOtherRecipientsIntact()
    {
        int callsA = 0, callsB = 0;
        object recipientA = new object(), recipientB = new object();
        m_Messenger.Register<TestMessage>(recipientA, m => callsA++, MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientB, m => callsB++, MessageContext.UiToTrace);

        m_Messenger.Unregister(recipientA, MessageContext.UiToTrace);
        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);

        Assert.AreEqual(0, callsA);
        Assert.AreEqual(1, callsB);
    }

    [Test]
    public void Unregister_OfAnUnknownRecipient_IsASilentNoOp()
    {
        Assert.DoesNotThrow(() => m_Messenger.Unregister(new object(), MessageContext.UiToTrace));
    }

    [Test]
    public void Register_DuplicateRecipientOnSameContext_IsRejectedLoudly_AndKeepsTheFirstHandler()
    {
        int firstCalls = 0, secondCalls = 0;
        m_Messenger.Register<TestMessage>(this, m => firstCalls++, MessageContext.UiToTrace);

        LogAssert.Expect(LogType.Error, new Regex("already registered.*duplicate handler ignored"));
        m_Messenger.Register<TestMessage>(this, m => secondCalls++, MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        Assert.AreEqual(1, firstCalls);
        Assert.AreEqual(0, secondCalls);
    }

    [Test]
    public void Register_SameRecipientOnTwoContexts_IsIndependent()
    {
        int callsTrace = 0, callsVideo = 0;
        m_Messenger.Register<TestMessage>(this, m => callsTrace++, MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(this, m => callsVideo++, MessageContext.UiToVideo);

        m_Messenger.Unregister(this, MessageContext.UiToTrace);
        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        m_Messenger.Send(new TestMessage(), MessageContext.UiToVideo);

        Assert.AreEqual(0, callsTrace);
        Assert.AreEqual(1, callsVideo);
    }

    [Test]
    public void Register_AfterUnregister_WorksWithoutDuplicateError()
    {
        int calls = 0;
        m_Messenger.Register<TestMessage>(this, m => calls++, MessageContext.UiToTrace);
        m_Messenger.Unregister(this, MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(this, m => calls += 10, MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);

        Assert.AreEqual(10, calls);
    }

    [Test]
    public void Send_ThrowingHandler_DoesNotAbortDispatchToTheRemainingRecipients()
    {
        List<string> delivered = new List<string>();
        object recipientA = new object(), recipientB = new object(), recipientC = new object();
        m_Messenger.Register<TestMessage>(recipientA, m => delivered.Add("A"), MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientB, m => throw new System.InvalidOperationException("boom"), MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientC, m => delivered.Add("C"), MessageContext.UiToTrace);

        LogAssert.Expect(LogType.Error, new Regex("handler of Object threw on context UiToTrace"));
        LogAssert.Expect(LogType.Exception, new Regex("boom"));
        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);

        CollectionAssert.AreEqual(new[] { "A", "C" }, delivered);
    }

    [Test]
    public void Send_HandlerRegisteredForAnotherMessageType_IsSkippedLoudly()
    {
        int otherCalls = 0, goodCalls = 0;
        object wrongTypeRecipient = new object();
        m_Messenger.Register<OtherMessage>(wrongTypeRecipient, m => otherCalls++, MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(this, m => goodCalls++, MessageContext.UiToTrace);

        LogAssert.Expect(LogType.Error, new Regex("different message type.*handler skipped"));
        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);

        Assert.AreEqual(0, otherCalls);
        Assert.AreEqual(1, goodCalls);
    }

    [Test]
    public void Handler_UnregisteringItselfDuringDispatch_StillCompletesTheCurrentDispatch()
    {
        List<string> delivered = new List<string>();
        object recipientA = new object(), recipientB = new object(), recipientC = new object();
        m_Messenger.Register<TestMessage>(recipientA, m => delivered.Add("A"), MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientB, m =>
        {
            delivered.Add("B");
            m_Messenger.Unregister(recipientB, MessageContext.UiToTrace);
        }, MessageContext.UiToTrace);
        m_Messenger.Register<TestMessage>(recipientC, m => delivered.Add("C"), MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        CollectionAssert.AreEqual(new[] { "A", "B", "C" }, delivered, "the in-flight dispatch iterates the snapshot taken at Send");

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        CollectionAssert.AreEqual(new[] { "A", "B", "C", "A", "C" }, delivered, "the unregistration applies from the next Send");
    }

    [Test]
    public void Handler_RegisteringANewRecipientDuringDispatch_NewRecipientOnlyGetsTheNextSend()
    {
        List<string> delivered = new List<string>();
        object recipientA = new object(), lateRecipient = new object();
        bool registered = false;
        m_Messenger.Register<TestMessage>(recipientA, m =>
        {
            delivered.Add("A");
            if (!registered)
            {
                registered = true;
                m_Messenger.Register<TestMessage>(lateRecipient, msg => delivered.Add("late"), MessageContext.UiToTrace);
            }
        }, MessageContext.UiToTrace);

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        CollectionAssert.AreEqual(new[] { "A" }, delivered, "a recipient registered mid-dispatch must not receive the in-flight message");

        m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace);
        CollectionAssert.AreEqual(new[] { "A", "A", "late" }, delivered);
    }

    [Test]
    public void Register_WithANullHandler_IsRejectedLoudly()
    {
        LogAssert.Expect(LogType.Error, new Regex("null handler"));
        m_Messenger.Register<TestMessage>(this, null, MessageContext.UiToTrace);

        Assert.DoesNotThrow(() => m_Messenger.Send(new TestMessage(), MessageContext.UiToTrace));
    }
}
