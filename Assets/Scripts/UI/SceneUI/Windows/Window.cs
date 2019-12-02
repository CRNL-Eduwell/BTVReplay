using UnityEngine;
using UnityEngine.Events;

namespace BTV.UI
{
    public abstract class Window : MonoBehaviour
    {
        public virtual bool Interactable
        {
            get
            {
                return m_Interactable;
            }
            set
            {
                m_Interactable = value;
            }
        }
        [SerializeField] protected bool m_Interactable = true;

        #region Public Methods
        public virtual void Close()
        {
            Destroy(gameObject);
        }
        #endregion

        #region Protected Methods
        protected virtual void Awake()
        {
            Initialize();
        }
        protected virtual void Initialize()
        {
            SetFields();
        }

        protected virtual void SetFields()
        {

        }
        #endregion
    }
}