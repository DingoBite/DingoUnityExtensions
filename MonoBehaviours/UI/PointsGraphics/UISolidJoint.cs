using UnityEngine;
using UnityEngine.UI;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public class UISolidJoint : UIJoint
    {
        [SerializeField] protected Graphic Graphic;
        [SerializeField] private Color _color = Color.white;

        public override Color Color
        {
            get => _color;
            set
            {
                value.a = Alpha;
                _color = value;
                Graphic.color = value;
            }
        }

        private void Reset()
        {
            Graphic = GetComponent<Graphic>();
        }
    }
}