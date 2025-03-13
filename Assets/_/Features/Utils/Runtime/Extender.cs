using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Runtime
{
    public static class Extender
    {
        #region Main

        public static T GetRandom<T>(this IEnumerable<T> ienumerable)
        {
            int index = Utility.m_rng.Next(0, ienumerable.Count());
            return ienumerable.ElementAt(index);
        }

        public static T GetRandom<T>(this IEnumerable<T> ienumerable, out int index)
        {
            index = Utility.m_rng.Next(0, ienumerable.Count());
            return ienumerable.ElementAt(index);
        }

        public static Vector3 ToVector3(this Vector2 v2)
        {
            return new Vector3(v2.x, 0, v2.y);
        }

        public static Vector2 ToVector2(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }
        public static Vector3 AxisToZero(this Vector3 v3, params Axis[] parameters)
        {
            Vector3 newVector = Vector3.zero;

            foreach (var param in parameters)
            {
                switch (param)
                {
                    case Axis.X:
                        newVector.x = v3.x;
                        break;

                    case Axis.Y:
                        newVector.y = v3.y;
                        break;

                    case Axis.Z:
                        newVector.z = v3.z;
                        break;
                }
            }

            return newVector;
        }

        #endregion
    }
    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2
    }

}