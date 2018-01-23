using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class Item<T> : MonoBehaviour
    {
        #region Properties
        protected T m_Object;
        public virtual T Object
        {
            get { return m_Object; }
            set { m_Object = value; }
        }
        public virtual bool interactable { get; set; }
        #endregion
    }
}

