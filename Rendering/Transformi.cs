using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Materia.Rendering
{
    public class Transformi
    {
        public Transformi Parent { get; protected set; }

        private List<Transformi> children;
        public List<Transformi> Children
        {
            get
            {
                return children.ToList();
            }
        }

        public Quaterniond Rotation
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Rotation * LocalRotation;
                }

                return LocalRotation;
            }
            set
            {
                if (Parent != null)
                {
                    Quaterniond pcopy = Parent.Rotation;
                    pcopy.Conjugate();
                    LocalRotation = value * pcopy;
                }
            }
        }

        private Vector3i localEulerAngles;

        public Vector3i LocalEulerAngles
        {
            get
            {
                return localEulerAngles;
            }
            set
            {
                localEulerAngles = value;
                Vector3i angles = value * ((float)Math.PI / 180.0f);
                LocalRotation = Quaterniond.FromEulerAngles(angles.X, angles.Y, angles.Z);
            }
        }

        public Quaterniond LocalRotation { get; set; }

        public Vector3i Scale
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.Scale * LocalScale;
                }

                return LocalScale;
            }
        }

        public Vector3i LocalScale { get; set; }

        public Vector3i Position
        {
            get
            {
                if (Parent != null)
                {
                    return LocalPosition + Parent.Position;
                }

                return LocalPosition;
            }
            set
            {
                if (Parent != null)
                {
                    LocalPosition = value - Parent.Position;
                    return;
                }

                LocalPosition = value;
            }
        }

        public Vector3i LocalPosition { get; set; }

        public Matrix4d WorldMatrix
        {
            get
            {
                return Matrix4d.CreateScale(Scale.X, Scale.Y, Scale.Z) * Matrix4d.CreateFromQuaternion(Rotation) * Matrix4d.CreateTranslation(Position.X, Position.Y, Position.Z);
            }
        }

        public Matrix4d LocalMatrix
        {
            get
            {
                return Matrix4d.CreateScale(LocalScale.X, LocalScale.Y, LocalScale.Z) * Matrix4d.CreateFromQuaternion(LocalRotation) * Matrix4d.CreateTranslation(LocalPosition.X, LocalPosition.Y, LocalPosition.Z);
            }
        }

        public Vector3i Forward
        {
            get
            {
                Quaterniond rot = Rotation;
                rot.Conjugate();
                return rot * Vector3i.UnitZ;
            }
        }

        public Vector3i Right
        {
            get
            {
                Quaterniond rot = Rotation;
                rot.Conjugate();
                return rot * Vector3i.UnitX;
            }
        }

        public Vector3i Up
        {
            get
            {
                Quaterniond rot = Rotation;
                rot.Conjugate();
                return rot * Vector3i.UnitY;
            }
        }

        public Transformi(Transformi parent = null)
        {
            Parent = parent;
            children = new List<Transformi>();
            LocalScale = new Vector3i(1, 1, 1);
            LocalPosition = Vector3i.Zero;
            LocalRotation = Quaterniond.Identity;
        }

        public void Add(Transformi c)
        {
            if (c.Parent != null)
            {
                c.Parent.Remove(c);
            }
            c.Parent = this;
            children.Add(c);
        }

        public void Remove(Transformi c)
        {
            children.Remove(c);
            c.Parent = null;
        }
    }
}
