using UnityEngine;
using UnityEngine.UI;

namespace Energy8.Identity.UI.Core.Compoents.Gradient
{
	public class SetDirty : MonoBehaviour
	{
		public Graphic m_graphic;
		// Use this for initialization
		void Reset()
		{
			m_graphic = GetComponent<Graphic>();
		}

		// Update is called once per frame
		void Update()
		{
			m_graphic.SetVerticesDirty();
		}
	}
}