using System;
using System.Collections;
using BrainTV.Tools.NumberExtensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI
{
    //InputDialog
    public class InputFieldWindow : MonoBehaviour
    {
        public bool IsClosed { get; private set; } = false;
        public string StringValue { get { return m_InputField.text; } set { m_InputField.text = value; } }
        public int IntValue { get { return StringValue.TryParseInt(out int result) ? result : 0; } }

        [SerializeField]
        private Text m_Header = null;
        [SerializeField]
        private Text m_Message = null;
        [SerializeField]
        private InputField m_InputField = null;
        [SerializeField]
        private Button m_OkButton = null;
        [SerializeField]
        private Button m_CancelButton = null;

        public void Initialize(string header, string message, UnityAction yesAction, UnityAction cancelAction)
        {
            m_Header.text = header;
            m_Message.text = message;

            m_OkButton.onClick.AddListener(yesAction);
            m_CancelButton.onClick.AddListener(cancelAction);
        }

        public void Close()
        {
            Destroy(gameObject);
        }
    }
}
