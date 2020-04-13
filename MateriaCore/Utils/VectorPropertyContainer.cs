using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public class VectorPropertyContainer
    {
        public delegate void Updated();
        public event Updated OnUpdate;

        protected float xprop;
        public float XProp
        {
            get
            {
                return xprop;
            }
            set
            {
                xprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }

        protected float yprop;
        public float YProp
        {
            get
            {
                return yprop;
            }
            set
            {
                yprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }
        protected float zprop;
        public float ZProp
        {
            get
            {
                return zprop;
            }
            set
            {
                zprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }
        protected float wprop;
        public float WProp
        {
            get
            {
                return xprop;
            }
            set
            {
                xprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }

        public VectorPropertyContainer(MVector v)
        {
            xprop = v.X;
            yprop = v.Y;
            zprop = v.Z;
            wprop = v.W;
        }

        public MVector Vector
        {
            get
            {
                return new MVector(xprop, yprop, zprop, wprop);
            }
            set
            {
                xprop = value.X;
                yprop = value.Y;
                zprop = value.Z;
                wprop = value.W;
            }
        }
    }
}
